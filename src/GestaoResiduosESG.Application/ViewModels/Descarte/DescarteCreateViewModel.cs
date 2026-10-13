using System.ComponentModel.DataAnnotations;

namespace GestaoResiduosESG.Application.ViewModels.Descarte;

public class DescarteCreateViewModel
{
    [Required(ErrorMessage = "O resíduo é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "ResiduoId inválido")]
    public int ResiduoId { get; set; }

    [Required(ErrorMessage = "O ponto de coleta é obrigatório")]
    [Range(1, int.MaxValue, ErrorMessage = "PontoColetaId inválido")]
    public int PontoColetaId { get; set; }

    [Required(ErrorMessage = "A data do descarte é obrigatória")]
    public DateTime DataDescarte { get; set; }

    [Required(ErrorMessage = "A quantidade é obrigatória")]
    [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero")]
    public decimal Quantidade { get; set; }

    [Required(ErrorMessage = "O responsável é obrigatório")]
    [MaxLength(100, ErrorMessage = "O responsável deve ter no máximo 100 caracteres")]
    public string Responsavel { get; set; } = string.Empty;
}
