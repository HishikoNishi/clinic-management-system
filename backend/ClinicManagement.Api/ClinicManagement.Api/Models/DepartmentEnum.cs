using System.ComponentModel.DataAnnotations;
namespace ClinicManagement.Api.Enums
{
    public enum DepartmentEnum
    {
        [Display(Name = "Khoa Nội")]
        Noi,
        [Display(Name = "Khoa Ngoại")]
        Ngoai,
        [Display(Name = "Khoa Sản")]
        San,
        [Display(Name = "Khoa Nhi")]
        Nhi,
        [Display(Name = "Răng – Hàm – Mặt")]
        RangHamMat,
        [Display(Name = "Tai – Mũi – Họng")]
        TaiMuiHong,
        [Display(Name = "Khám tổng quát")]
        TongQuat
    }
}
