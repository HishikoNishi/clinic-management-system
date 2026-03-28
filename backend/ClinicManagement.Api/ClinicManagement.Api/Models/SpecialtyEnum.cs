using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ClinicManagement.Api.Models
{
    public enum SpecialtyEnum
    {
        // Nội
        [Display(Name = "Nội tổng quát")]
        NoiTongQuat,
        [Display(Name = "Nội tim mạch")]
        NoiTimMach,
        [Display(Name = "Nội tiêu hóa")]
        NoiTieuHoa,

        // Ngoại
        [Display(Name = "Ngoại tổng quát")]
        NgoaiTongQuat,
        [Display(Name = "Chấn thương chỉnh hình")]
        ChanThuongChinhHinh,

        // Sản
        [Display(Name = "Sản phụ khoa")]
        SanPhuKhoa,
        [Display(Name = "Khám thai")]
        KhamThai,

        // Nhi
        [Display(Name = "Nhi tổng quát")]
        NhiTongQuat,

        // Răng – Hàm – Mặt
        [Display(Name = "Nha tổng quát")]
        NhaTongQuat,
        [Display(Name = "Niềng răng")]
        NiengRang,

        // Tai – Mũi – Họng
        [Display(Name = "Khám TMH")]
        KhamTMH,

        // Khám tổng quát
        [Display(Name = "Khám sức khỏe")]
        KhamSucKhoe,
        [Display(Name = "Tiêm chủng")]
        TiemChung
    }

}

