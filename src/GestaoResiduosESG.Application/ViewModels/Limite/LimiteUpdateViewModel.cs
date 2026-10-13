using System.ComponentModel.DataAnnotations;

namespace GestaoResiduosESG.Application.ViewModels.Limite;

public class LimiteUpdateViewModel
{
    [Required(ErrorMessage = "O limite mínimo é obrigatório")]
    [Range(0, double.MaxValue, ErrorMessage = "O limite mínimo deve ser maior ou igual a zero")]
    public decimal LimiteMinimo { get; set; }

    [Required(ErrorMessage = "O limite máximo é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O limite máximo deve ser maior que zero")]
    public decimal LimiteMaximo { get; set; }

    [Range(1, 100, ErrorMessage = "O percentual de alerta deve ser entre 1 e 100")]
    public int PercentualAlerta { get; set; } = 80;
}
