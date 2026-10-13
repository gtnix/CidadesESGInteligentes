using System.ComponentModel.DataAnnotations;

namespace GestaoResiduosESG.Application.ViewModels.Auth;

public class LoginViewModel
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
