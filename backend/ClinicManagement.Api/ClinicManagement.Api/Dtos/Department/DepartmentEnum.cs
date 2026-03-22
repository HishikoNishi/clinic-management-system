using System.ComponentModel.DataAnnotations;

public enum DepartmentEnum
{
    [Display(Name = "Nội")]
    Noi,
    [Display(Name = "Ngoại")]
    Ngoai,
    [Display(Name = "Nhi")]
    Nhi,
    [Display(Name = "Sản")]
    San,
    [Display(Name = "Mắt")]
    Mat,
    [Display(Name = "Tai Mũi Họng")]
    TaiMuiHong,
    [Display(Name = "Da Liễu")]
    DaLieu,
    [Display(Name = "Tim Mạch")]
    TimMach,
    [Display(Name = "Thần Kinh")]
    ThanKinh,
    [Display(Name = "Răng Hàm Mặt")]
    RangHamMat
}
