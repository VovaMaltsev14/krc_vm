using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;

namespace ShopInfrastructure.ViewModel;

public class RegisterViewModel
{
    [Required]
    [Display(Name="Email")]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    
    [Required]
    [Display(Name="Phone number")]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; }
    
    [Required]
    [Display(Name="Дата народження")]
    public DateTime Birthdate { get; set; }
    
    [Required]
    [Display(Name="Пароль")]
    public string Password { get; set; }
    
    [Required]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    [Display(Name="Підтвердження паролю")]
    [DataType(DataType.Password)]
    public string PasswordConfirm { get; set; }
}