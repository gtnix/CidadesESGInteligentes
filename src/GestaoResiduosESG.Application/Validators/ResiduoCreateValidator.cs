using FluentValidation;
using GestaoResiduosESG.Application.ViewModels.Residuo;
using GestaoResiduosESG.Domain.Enums;

namespace GestaoResiduosESG.Application.Validators;

public class ResiduoCreateValidator : AbstractValidator<ResiduoCreateViewModel>
{
    public ResiduoCreateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Categoria)
            .IsInEnum().WithMessage("Categoria inválida. Valores aceitos: Organico, Reciclavel, Perigoso, NaoReciclavel, Eletronico");

        RuleFor(x => x.UnidadeMedida)
            .NotEmpty().WithMessage("A unidade de medida é obrigatória")
            .MaximumLength(20).WithMessage("A unidade de medida deve ter no máximo 20 caracteres");
    }
}
