using System.Web;
using System.Web.Mvc;

namespace ShopBanGiay_Nhom13
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}