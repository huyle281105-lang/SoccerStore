using ShopBanGiay_Nhom13.Models;
using System.Linq;
using System.Web.Mvc;

namespace ShopBanGiay_Nhom13.Controllers
{
    public class CartController : Controller
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

        public ActionResult Add(string masp, int quantity = 1)
        {
            var kh = GetCurrentCustomer();
            if (kh == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var item = csdl.GIOHANGs.FirstOrDefault(g => g.MAKH == kh.MAKH && g.MASP == masp);
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
                    SOLUONG = quantity
                });
            }

            csdl.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            var kh = GetCurrentCustomer();
            if (kh == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            LoadMasterData();
            var cart = csdl.GIOHANGs.Where(g => g.MAKH == kh.MAKH).ToList();
            return View("~/Views/Home/GioHang.cshtml", cart);
        }

        [HttpPost]
        public ActionResult Update(string masp, int soluong)
        {
            var kh = GetCurrentCustomer();
            if (kh == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var item = csdl.GIOHANGs.FirstOrDefault(g => g.MAKH == kh.MAKH && g.MASP == masp);
            if (item != null)
            {
                item.SOLUONG = soluong;
                csdl.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Remove(string masp)
        {
            var kh = GetCurrentCustomer();
            if (kh == null)
            {
                return RedirectToAction("SignIn", "Auth");
            }

            var item = csdl.GIOHANGs.FirstOrDefault(g => g.MAKH == kh.MAKH && g.MASP == masp);
            if (item != null)
            {
                csdl.GIOHANGs.Remove(item);
                csdl.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
