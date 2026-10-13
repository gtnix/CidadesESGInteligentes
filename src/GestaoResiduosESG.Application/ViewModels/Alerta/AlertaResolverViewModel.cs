using System.ComponentModel.DataAnnotations;

namespace GestaoResiduosESG.Application.ViewModels.Alerta;

public class AlertaResolverViewModel
{
    [MaxLength(500, ErrorMessage = "A observação deve ter no máximo 500 caracteres")]
    public string? Observacao { get; set; }
}
