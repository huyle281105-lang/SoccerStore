using System.Data.Entity;
using ShopBanGiay_Nhom13.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
namespace ShopBanGiay_Nhom13.Controllers
{
    public class OrderController : Controller
    {
        private readonly SOCCERSTOREEntities csdl = new SOCCERSTOREEntities();
        private const string SessionCustomerKey = "KHACHHANG";
        private const string LegacySessionCustomerKey = "KHACHHANGs";

        private void LoadMasterData()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();
        }

        private KHACHHANG GetCurrentCustomer()
        {
            var kh = Session[SessionCustomerKey] as KHACHHANG;
            if (kh == null)
            {
                kh = Session[LegacySessionCustomerKey] as KHACHHANG;
                if (kh != null)
                {
                    Session[SessionCustomerKey] = kh;
                }
            }

            return kh;
        }

        private string GenerateNextCode(string prefix, IEnumerable<string> existingCodes, int width = 3)
        {
            var regex = new Regex("^" + Regex.Escape(prefix) + "(\\d+)$");
            int max = 0;
            foreach (var code in existingCodes)
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    continue;
                }

                var match = regex.Match(code.Trim());
                if (!match.Success)
                {
                    continue;
                }

                int number;
                if (int.TryParse(match.Groups[1].Value, out number) && number > max)
                {
                    max = number;
                }
            }

            return prefix + (max + 1).ToString("D" + width);
        }

        public ActionResult ThanhToan()
        {
            var kh = GetCurrentCustomer();
            if (kh == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            LoadMasterData();
            var cart = csdl.GIOHANGs.Where(g => g.MAKH == kh.MAKH).ToList();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Cart");
            }

            decimal total = 0m;
            foreach (var item in cart)
            {
                var sp = csdl.SANPHAMs.FirstOrDefault(s => s.MASP == item.MASP);
                if (sp?.GIA != null && item.SOLUONG != null)
                {
                    total += sp.GIA.Value * item.SOLUONG.Value;
                }
            }

            ViewBag.Total = total;
            return View("~/Views/Home/ThanhToan.cshtml", cart);
        }

        [HttpPost]
        public ActionResult LuuHoaDon(string TenNguoiNhan, string DiaChi, string GhiChu, DateTime? NgayHenGiao)
        {
            var kh = GetCurrentCustomer();
            if (kh == null) return RedirectToAction("SignIn", "Auth");

            // 1. Lấy giỏ hàng và ép kiểu về List để ngắt kết nối truy vấn (tránh lỗi Reader)
            var customerId = kh.MAKH;
            var cart = csdl.GIOHANGs.Where(g => g.MAKH == customerId).ToList();

            if (!cart.Any()) return RedirectToAction("Index", "Cart");

            try
            {
                // 2. Tạo đối tượng Hóa đơn mới
                var hd = new HOADON
                {
                    MAHD = GenerateNextCode("HD", csdl.HOADONs.Select(x => x.MAHD).ToList()), // Thêm ToList() ở đây
                    MAKH = kh.MAKH,
                    NGAYTAO = DateTime.Now,
                    DIACHI = DiaChi,
                    GHICHU = GhiChu,
                    NGAYHENGIAO = NgayHenGiao ?? DateTime.Now.AddDays(3),
                    TONGTIEN = 0 // Sẽ tính ở dưới
                };

                decimal total = 0;

                // 3. Duyệt danh sách cart đã có sẵn trong bộ nhớ (In-memory)
                foreach (var item in cart)
                {
                    // Tìm sản phẩm trực tiếp từ DB
                    var sp = csdl.SANPHAMs.Find(item.MASP); // Dùng Find sẽ nhanh và ổn định hơn FirstOrDefault

                    if (sp == null || (sp.SOLUONG ?? 0) < (item.SOLUONG ?? 0))
                    {
                        TempData["Error"] = $"Sản phẩm mã {item.MASP} không đủ tồn kho.";
                        return RedirectToAction("Index", "Cart");
                    }

                    // Tính tiền
                    total += (item.SOLUONG ?? 0) * (sp.GIA ?? 0);

                    // Thêm Chi tiết hóa đơn
                    var cthd = new CHITIETHOADON
                    {
                        MAHD = hd.MAHD,
                        MASP = item.MASP,
                        SOLUONG = item.SOLUONG
                    };
                    csdl.CHITIETHOADONs.Add(cthd);

                    // Trừ kho
                    sp.SOLUONG -= item.SOLUONG;

                    // Xóa giỏ hàng
                    csdl.GIOHANGs.Remove(item);
                }

                hd.TONGTIEN = total;
                csdl.HOADONs.Add(hd);

                // 4. Lệnh quan trọng nhất: Chỉ SaveChanges 1 lần duy nhất
                csdl.SaveChanges();

                return RedirectToAction("DatHanhThanhCong", new { id = hd.MAHD });
            }
            catch (Exception ex)
            {
                // Để biết chính xác lỗi gì, bạn hãy đặt Debug tại đây và xem biến ex.InnerException
                string error = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["Error"] = "Lỗi lưu hóa đơn: " + error;
                return RedirectToAction("Index", "Cart");
            }
        }

        public ActionResult DatHanhThanhCong(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index", "Home");
            }

            LoadMasterData();
            var hd = csdl.HOADONs.FirstOrDefault(x => x.MAHD == id);
            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "Home");
            }

            var firstItem = hd.CHITIETHOADONs.FirstOrDefault();
            if (firstItem != null)
            {
                ViewBag.SanPham = csdl.SANPHAMs.FirstOrDefault(x => x.MASP == firstItem.MASP);
            }

            return View("~/Views/Home/DatHanhThanhCong.cshtml", hd);
        }
    }
}
