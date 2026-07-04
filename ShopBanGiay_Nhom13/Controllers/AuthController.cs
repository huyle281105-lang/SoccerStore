using ShopBanGiay_Nhom13.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace ShopBanGiay_Nhom13.Controllers
{
    public class AuthController : Controller
    {
        private readonly SOCCERSTOREEntities csdl = new SOCCERSTOREEntities();
        private const string SessionCustomerKey = "KHACHHANG";
        private const string LegacySessionCustomerKey = "KHACHHANGs";

        private void LoadMasterData()
        {
            ViewBag.lsp = csdl.LOAISPs.ToList();
            ViewBag.thuonghieu = csdl.THUONGHIEUx.ToList();
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

        public ActionResult SignIn()
        {
            LoadMasterData();
            return View("~/Views/Home/SignIn.cshtml");
        }

        [HttpPost]
        public ActionResult SignIn(string txtName, string txtPass)
        {
            LoadMasterData();
            var kh = csdl.KHACHHANGs.FirstOrDefault(x => x.EMAIL == txtName && x.PASSWORD_KH == txtPass);

            if (string.IsNullOrEmpty(txtName) || string.IsNullOrEmpty(txtPass))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View("~/Views/Home/SignIn.cshtml");
            }

            if (kh == null || kh.PASSWORD_KH != txtPass)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng!";
                return View("~/Views/Home/SignIn.cshtml");
            }

            SetCurrentCustomer(kh);
            if (kh.ROLES != null && kh.ROLES.Trim().ToLower() == "admin")
            {
                return RedirectToAction("Dashboard", "Home");
            }

            return RedirectToAction("Index", "Home");
        }

        public ActionResult SignUp()
        {
            LoadMasterData();
            return View("~/Views/Home/SignUp.cshtml");
        }

        [HttpPost]
        public ActionResult SignUp(string txtEmail, string txtPhone, string txtPass, string txtRepass)
        {
            LoadMasterData();

            if (string.IsNullOrEmpty(txtEmail) || string.IsNullOrEmpty(txtPhone) || string.IsNullOrEmpty(txtPass) || string.IsNullOrEmpty(txtRepass))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View("~/Views/Home/SignUp.cshtml");
            }

            if (txtPass != txtRepass)
            {
                ViewBag.Error = "Mật khẩu không trùng khớp!";
                return View("~/Views/Home/SignUp.cshtml");
            }

            if (csdl.KHACHHANGs.Any(x => x.EMAIL == txtEmail))
            {
                ViewBag.Error = "Email đã được sử dụng!";
                return View("~/Views/Home/SignUp.cshtml");
            }

            var kh = new KHACHHANG
            {
                MAKH = GenerateNextCode("KH", csdl.KHACHHANGs.Select(x => x.MAKH)),
                EMAIL = txtEmail,
                SODIENTHOAI = txtPhone,
                PASSWORD_KH = txtPass,
                TENKH = txtEmail,
                ROLES = "user"
            };

            csdl.KHACHHANGs.Add(kh);
            csdl.SaveChanges();
            SetCurrentCustomer(kh);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Logout()
        {
            SetCurrentCustomer(null);
            return RedirectToAction("SignIn");
        }
    }
}
