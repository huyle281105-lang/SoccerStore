using ShopBanGiay_Nhom13.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
//using System.Xml.Linq;
using System.IO;
using System.Text.RegularExpressions;

namespace ShopBanGiay_Nhom13.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        SOCCERSTOREEntities csdl = new SOCCERSTOREEntities();
        private const string SessionCustomerKey = "KHACHHANG";
        private const string LegacySessionCustomerKey = "KHACHHANGs";

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

        private void SetCurrentCustomer(KHACHHANG khachHang)
        {
            Session[SessionCustomerKey] = khachHang;
            Session[LegacySessionCustomerKey] = khachHang;
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
                if (match.Success)
                {
                    int number;
                    if (int.TryParse(match.Groups[1].Value, out number) && number > max)
                    {
                        max = number;
                    }
                }
            }

            return prefix + (max + 1).ToString("D" + width);
        }
        public ActionResult Index()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            ViewBag.spNB = csdl.SANPHAMNOIBATs
                                        .Where(spnb => spnb.NGAYBD <= DateTime.Now && spnb.NGAYKT >= DateTime.Now)
                                        .Select(spnb => spnb.SANPHAM)
                                        .Distinct()
                                        .ToList();
            return View();
        }
        public ActionResult SignIn()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();
            return View();
        }
        [HttpPost]
        public ActionResult SignIn(string txtName, string txtPass)
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();
            var kh = csdl.KHACHHANGs.FirstOrDefault(x => x.EMAIL == txtName && x.PASSWORD_KH == txtPass);

            if (string.IsNullOrEmpty(txtName) ||
                string.IsNullOrEmpty(txtPass))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }

            if (kh == null || kh.PASSWORD_KH != txtPass)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng!";
                return View();
            }

            SetCurrentCustomer(kh);
            if (kh.ROLES != null && kh.ROLES.Trim().ToLower() == "admin")
            {
                return RedirectToAction("Dashboard");
            }
            else
            {
                // Là User hoặc Role khác
                return RedirectToAction("Index");
            }
        }
        public ActionResult SignUp()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult SignUp(string txtEmail, string txtPhone, string txtPass, string txtRepass)
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();
            if (string.IsNullOrEmpty(txtEmail) ||
                string.IsNullOrEmpty(txtPhone) ||
                string.IsNullOrEmpty(txtPass) ||
                string.IsNullOrEmpty(txtRepass))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View();
            }
            if (txtPass != txtRepass)
            {
                ViewBag.Error = "Mật khẩu không trùng khớp!";
                return View();
            }

            // Tạo mã KH tự động
            if (csdl.KHACHHANGs.Any(x => x.EMAIL == txtEmail))
            {
                ViewBag.Error = "Email đã được sử dụng!";
                return View();
            }

            string makh = GenerateNextCode("KH", csdl.KHACHHANGs.Select(x => x.MAKH));

            KHACHHANG kh = new KHACHHANG()
            {
                MAKH = makh,
                EMAIL = txtEmail,
                SODIENTHOAI = txtPhone,
                PASSWORD_KH = txtPass,
                TENKH = txtEmail,
                ROLES = "user"
            };

            csdl.KHACHHANGs.Add(kh);
            csdl.SaveChanges();

            SetCurrentCustomer(kh);

            return RedirectToAction("Index");
        }
        public ActionResult Logout()
        {
            SetCurrentCustomer(null);
            return RedirectToAction("SignIn", "Auth");
        }
        public ActionResult DanhMucSanPham()
        {
            return RedirectToAction("LocSanPham", new { phanLoai = "Giay", maLoai = (string)null });
        }
        public ActionResult LocSanPham(string MAL, string math)
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            List<SANPHAM> dssp = csdl.SANPHAMs.Where(x => x.LOAISP.MAL == MAL).ToList();

            //Loc San Pham Theo Thuong Hieu
            if (math !=null)
            {

                List<SANPHAM> dsth = csdl.SANPHAMs.Where(x => x.THUONGHIEU.MATH == math).ToList();

                var listMaLoaiLienQuan = dsth.Select(sp => sp.MAL).Distinct().ToList();

                var dsLoaiCon = csdl.LOAISPs
                                      .Where(lsp => listMaLoaiLienQuan.Contains(lsp.MAL))
                                      .OrderBy(lsp => lsp.MAL_CHA)
                                      .ToList();


                var model = new DanhMucViewModel
                {
                    DanhSachSP = dssp,
                    DanhSachLoaiSP = dsLoaiCon,
                    PhanLoai = "ThuongHieu",
                    MaLoaiHienTai = null
                };
                ViewBag.math = math;
                return View("DanhMucSanPham", model);
            }
            else
            {
                var loaiSpHienTai = csdl.LOAISPs.FirstOrDefault(x => x.MAL == MAL);


                string phanLoai;
                string maCha;

                if (loaiSpHienTai != null)
                {
                    maCha = loaiSpHienTai.MAL_CHA;
                    phanLoai = (maCha == null ? "Giay" : "PhuKien");
                }
                else
                {
                    phanLoai = "Giay";
                    maCha = null;
                }
                var dsLoaiCon = new List<LOAISP>();

                if (maCha == null)
                {
                    dsLoaiCon = csdl.LOAISPs.Where(x => x.MAL_CHA == null).ToList();
                }
                else
                {
                    dsLoaiCon = csdl.LOAISPs.Where(x => x.MAL_CHA == maCha).ToList();
                }
                var model = new DanhMucViewModel
                {
                    DanhSachSP = dssp,
                    DanhSachLoaiSP = dsLoaiCon,
                    PhanLoai = phanLoai,
                    MaLoaiHienTai = null

                };
                return View("DanhMucSanPham", model);
            }
        }

        public ActionResult ThuongHieu(string math)
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            List<SANPHAM> dsth = csdl.SANPHAMs.Where(x => x.THUONGHIEU.MATH == math).ToList();

            var listMaLoaiLienQuan = dsth.Select(sp => sp.MAL).Distinct().ToList();

            var dsLoaiCon = csdl.LOAISPs
                                  .Where(lsp => listMaLoaiLienQuan.Contains(lsp.MAL))
                                  .OrderBy(lsp => lsp.MAL_CHA)
                                  .ToList();

            var model = new DanhMucViewModel
            {
                DanhSachSP = dsth,
                DanhSachLoaiSP = dsLoaiCon,
                PhanLoai = "ThuongHieu",
                MaLoaiHienTai = null

            };
            ViewBag.math = math;
            return View("DanhMucSanPham", model);
        }
        public ActionResult Giay()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            var model = new DanhMucViewModel
            {
                DanhSachSP = csdl.SANPHAMs.Where(x => x.LOAISP.MAL_CHA == null).ToList(),
                DanhSachLoaiSP = csdl.LOAISPs.Where(x => x.MAL_CHA == null).ToList(),
                PhanLoai = "Giay"
            };
            return View("DanhMucSanPham", model);
        }
        public ActionResult PhuKien()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            var model = new DanhMucViewModel
            {
                DanhSachSP = csdl.SANPHAMs.Where(x => x.LOAISP.MAL_CHA == "L001").ToList(),
                DanhSachLoaiSP = csdl.LOAISPs.Where(x => x.MAL_CHA == "L001").ToList(),
                PhanLoai = "PhuKien"
            };
            return View("DanhMucSanPham", model);
        }
        public ActionResult ChiTietSanPham(string masp)
        {
            if (masp == null) return RedirectToAction("Index", "Home");
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            SANPHAM ctsp = csdl.SANPHAMs.FirstOrDefault(x => x.MASP == masp);
            return View(ctsp);
        }
        public ActionResult flashSale()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            List<SANPHAM> dsSale = csdl.SANPHAMs.Where(sp => sp.MAKM != null).ToList();
            return View(dsSale);
        }


        public ActionResult Dashboard()
        {
            var kh = GetCurrentCustomer();
            if (kh == null || (kh.ROLES != null && kh.ROLES.Trim().ToLower() != "admin"))
            {
                return RedirectToAction("SignIn", "Auth");
            }
            var sanpham = csdl.SANPHAMs.ToList();
            ViewBag.Categories = csdl.LOAISPs
                .Select(l => l.TENL)
                .Distinct()
                .ToList();

            return View(sanpham);
        }


        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    TempData["Error"] = "Không xác định được sản phẩm cần xóa.";
                    return RedirectToAction("Dashboard");
                }

                var sp = csdl.SANPHAMs.FirstOrDefault(x => x.MASP == id);
                if (sp != null)
                {
                    csdl.SANPHAMs.Remove(sp);
                    csdl.SaveChanges();
                    TempData["Success"] = $"Đã xóa vĩnh viễn sản phẩm: {sp.TENSP}";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy sản phẩm cần xóa.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa sản phẩm: " + ex.Message;
            }
            return RedirectToAction("Dashboard");
        }
        public ActionResult SearchProducts(string Search)
        {
            ViewBag.lsp = csdl.LOAISPs.ToList(); ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList(); var model = new DanhMucViewModel
            {
                DanhSachLoaiSP = csdl.LOAISPs.Where(x => x.MAL_CHA == null).ToList(), // Giả sử hiển thị loại giày chính
                PhanLoai = "TimKiem",
                MaLoaiHienTai = null
            };

            if (string.IsNullOrWhiteSpace(Search))
            {
                ViewBag.Error = "Vui lòng nhập từ khóa tìm kiếm!";
                model.DanhSachSP = new List<SANPHAM>();
                return View("DanhMucSanPham", model); // Trả về DanhMucViewModel
            }
            var result = csdl.SANPHAMs.Where(sp => sp.TENSP.Contains(Search) || sp.LOAISP.TENL.Contains(Search) || sp.THUONGHIEU.TENTH.Contains(Search)).ToList();

            if (result.Count == 0)
            {
                ViewBag.Error = "Không tìm thấy sản phẩm nào phù hợp!";
            }

            model.DanhSachSP = result;
            return View("DanhMucSanPham", model);
        }
        public ActionResult ChinhSachBaoMat()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            return View();
        }
        public ActionResult ChinhSachBaoHanh()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            return View();
        }
        public ActionResult ChinhSachDoiTra()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            return View();
        }
        public ActionResult HuongDanMuaHang()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();
            return View();
        }
        public ActionResult HuongDanChonSize()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();
            return View();
        }
        
        [HttpGet]
        public ActionResult ThemSanPham()
        {
            string lastId = csdl.SANPHAMs
                .OrderByDescending(sp => sp.MASP)
                .Select(sp => sp.MASP)
                .FirstOrDefault();

            int number = 1;
            if (!string.IsNullOrEmpty(lastId))
                number = int.Parse(lastId.Substring(2)) + 1;

            ViewBag.NextMASP = "SP" + number.ToString("000");

            ViewBag.Loai = csdl.LOAISPs.ToList();
            ViewBag.ThuongHieu = csdl.THUONGHIEUx.ToList();
            ViewBag.KhuyenMai = csdl.KHUYENMAIs.ToList();

            return View();
        }

        [HttpPost]
        public ActionResult ThemSanPham(SANPHAM sp, HttpPostedFileBase upload)
        {
            try
            {
                var check = csdl.SANPHAMs.FirstOrDefault(x => x.MASP == sp.MASP);
                if (check != null)
                {
                    TempData["Error"] = "Mã sản phẩm đã tồn tại! Vui lòng nhập mã khác.";
                    return View();
                }

                if (upload != null && upload.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(upload.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                    upload.SaveAs(path);
                    sp.HINHANH = fileName;
                }

                if (string.IsNullOrEmpty(sp.MASP) ||
                    string.IsNullOrEmpty(sp.TENSP) ||
                    sp.GIA == null ||
                    sp.SOLUONG == null ||
                    string.IsNullOrEmpty(sp.MAL) ||
                    string.IsNullOrEmpty(sp.MATH))
                {
                    TempData["Error"] = "Vui lòng nhập đầy đủ thông tin!";
                    return View();
                }

                // Nếu không có KM thì để null
                if (string.IsNullOrEmpty(sp.MAKM))
                    sp.MAKM = null;

                csdl.SANPHAMs.Add(sp);
                csdl.SaveChanges();

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)


            {
                TempData["Error"] = "Lỗi: " + ex.Message;
                return View();
            }
        }
        public ActionResult HoaDon()
        {
            var list = csdl.HOADONs.ToList();
            return View(list);
        }

        public ActionResult NguoiDung()
        {
            return View(csdl.KHACHHANGs.ToList());
        }

        public ActionResult EditSanPham(string id)
        {
            if (id == null)
                return RedirectToAction("Dashboard");

            var sp = csdl.SANPHAMs.FirstOrDefault(x => x.MASP == id);
            if (sp == null)
                return RedirectToAction("Dashboard");

            ViewBag.Loai = csdl.LOAISPs.ToList();
            ViewBag.ThuongHieu = csdl.THUONGHIEUx.ToList();
            ViewBag.KhuyenMai = csdl.KHUYENMAIs.ToList();

            return View("EditSanPham", sp);
        }

        [HttpPost]
        public ActionResult EditSanPham(SANPHAM sp, HttpPostedFileBase upload)
        {
            var old = csdl.SANPHAMs.FirstOrDefault(x => x.MASP == sp.MASP);
            if (old == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm!";
                return RedirectToAction("Dashboard");
            }

            if (upload != null && upload.ContentLength > 0)
            {
                string fileName = Path.GetFileName(upload.FileName);
                string path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                upload.SaveAs(path);
                old.HINHANH = fileName;
            }

            old.TENSP = sp.TENSP;
            old.SOLUONG = sp.SOLUONG;
            old.GIA = sp.GIA;
            old.MOTA = sp.MOTA;
            old.MAL = sp.MAL;
            old.MATH = sp.MATH;
            old.MAKM = sp.MAKM;

            csdl.SaveChanges();

            TempData["Success"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction("Dashboard");
        }
        public ActionResult InAnQuanAo()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();
            return View();
        }
        public ActionResult ThanhLyVaKyGui()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            List<string> dsMaSPThanhLy = new List<string> { "SP039", "SP028", "SP034", "SP041", "SP051", "SP070", "SP072", "SP105", "SP118", "SP069" };

            var dsSanPhamThanhLy = csdl.SANPHAMs
                                         .Where(sp => dsMaSPThanhLy.Contains(sp.MASP))
                                         .ToList();

            return View(dsSanPhamThanhLy);
        }
        public ActionResult ThemVaoGioHang(string masp, int quantity = 1)
        {
            KHACHHANG kh = GetCurrentCustomer();
            if (kh== null)
                return RedirectToAction("SignIn", "Auth");

            var item = csdl.GIOHANGs
                         .FirstOrDefault(g => g.MAKH == kh.MAKH && g.MASP == masp);

            if (item != null)
            {
                item.SOLUONG += quantity;
            }
            else
            {
                csdl.GIOHANGs.Add(new GIOHANG
                {
                    MAKH = kh.MAKH,
                    MASP = masp,
                    SOLUONG= quantity
                });
            }

            csdl.SaveChanges();
            return RedirectToAction("Index", "Cart");
        }

        public ActionResult GioHang()
        {
            KHACHHANG kh = GetCurrentCustomer();
            if (kh == null)
                return RedirectToAction("SignIn", "Auth");
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();

            var cart = csdl.GIOHANGs
                         .Where(g => g.MAKH == kh.MAKH)
                         .ToList();
            return View(cart);
        }

        [HttpPost]
        public ActionResult Update(string masp, int soluong)
        {
            KHACHHANG kh = GetCurrentCustomer();
            if (kh == null)
                return RedirectToAction("SignIn", "Auth");

            var item = csdl.GIOHANGs.FirstOrDefault(g => g.MAKH == kh.MAKH && g.MASP== masp);

            if (item != null)
            {
                item.SOLUONG = soluong;
                csdl.SaveChanges();
            }

            return RedirectToAction("Index", "Cart");
        }

        public ActionResult Remove(string masp)
        {
            KHACHHANG kh = GetCurrentCustomer();
            if (kh == null)
                return RedirectToAction("SignIn", "Auth");

            var item = csdl.GIOHANGs.FirstOrDefault(g => g.MAKH == kh.MAKH && g.MASP == masp);

            if (item != null)
            {
                csdl.GIOHANGs.Remove(item);
                csdl.SaveChanges();
            }

            return RedirectToAction("Index", "Cart");
        }

        public ActionResult ThanhToan()
        {
            return RedirectToAction("ThanhToan", "Order");
        }
        [HttpPost]
        public ActionResult LuuHoaDon(string TenNguoiNhan, string DiaChi, string GhiChu, DateTime? NgayHenGiao)
        {
            return RedirectToAction("ThanhToan", "Order");
        }

        public ActionResult DatHanhThanhCong(string id)
        {
            return RedirectToAction("DatHanhThanhCong", "Order", new { id = id });
        }
    }
}
