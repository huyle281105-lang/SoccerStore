using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ShopBanGiay_Nhom13.Models;

namespace ShopBanGiay_Nhom13.Models
{
    public class DanhMucViewModel
    {
        public List<SANPHAM> DanhSachSP { get; set; }
        public List<LOAISP> DanhSachLoaiSP { get; set; }
        public string PhanLoai { get; set; }
        public string MaLoaiHienTai { get; set; }
        public string MaThuongHieuHienTai { get; set; }
    }
}