using System.ComponentModel.DataAnnotations;

namespace GestaoResiduosESG.Application.ViewModels.PontoColeta;

public class PontoColetaUpdateViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A localização é obrigatória")]
    [MaxLength(200, ErrorMessage = "A localização deve ter no máximo 200 caracteres")]
    public string Localizacao { get; set; } = string.Empty;

    [Required(ErrorMessage = "A capacidade máxima é obrigatória")]
    [Range(0.01, double.MaxValue, ErrorMessage = "A capacidade máxima deve ser maior que zero")]
    public decimal CapacidadeMaxima { get; set; }
}
