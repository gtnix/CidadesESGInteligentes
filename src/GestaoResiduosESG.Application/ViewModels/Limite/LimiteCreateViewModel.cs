using System.ComponentModel.DataAnnotations;

namespace GestaoResiduosESG.Application.ViewModels.Limite;

public class LimiteCreateViewModel
{
    [Required(ErrorMessage = "O ponto de coleta é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "PontoColetaId inválido")]
    public int PontoColetaId { get; set; }

    [Required(ErrorMessage = "O resíduo é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "ResiduoId inválido")]
    public int ResiduoId { get; set; }

    [Required(ErrorMessage = "O limite mínimo é obrigatório")]
    [Range(0, double.MaxValue, ErrorMessage = "O limite mínimo deve ser maior ou igual a zero")]
    public decimal LimiteMinimo { get; set; }

    [Required(ErrorMessage = "O limite máximo é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "O limite máximo deve ser maior que zero")]
    public decimal LimiteMaximo { get; set; }

    [Range(1, 100, ErrorMessage = "O percentual de alerta deve ser entre 1 e 100")]
    public int PercentualAlerta { get; set; } = 80;
}
