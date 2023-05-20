using System.ComponentModel.DataAnnotations;

namespace SimpleCRUD.View_Models;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}