using GestaoResiduosESG.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestaoResiduosESG.Application.ViewModels.Residuo;

public class ResiduoUpdateViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório")]
    [MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A categoria é obrigatória")]
    [EnumDataType(typeof(CategoriaResiduo), ErrorMessage = "Categoria inválida")]
    public CategoriaResiduo Categoria { get; set; }

    [Required(ErrorMessage = "A unidade de medida é obrigatória")]
    [MaxLength(20, ErrorMessage = "A unidade de medida deve ter no máximo 20 caracteres")]
    public string UnidadeMedida { get; set; } = string.Empty;
}
