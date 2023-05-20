using System.ComponentModel.DataAnnotations;

namespace SimpleCRUD.View_Models;

public class TwoFactorAuthViewModel
{
    [Required]
    public string Token { get; set; }
    [Required]
    public string Code { get; set; }

    public string QRCodeUrl { get; set; }
}