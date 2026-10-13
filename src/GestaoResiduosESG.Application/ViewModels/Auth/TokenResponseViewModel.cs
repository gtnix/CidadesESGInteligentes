namespace GestaoResiduosESG.Application.ViewModels.Auth;

public class TokenResponseViewModel
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
