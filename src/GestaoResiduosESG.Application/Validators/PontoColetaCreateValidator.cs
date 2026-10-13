using FluentValidation;
using GestaoResiduosESG.Application.ViewModels.PontoColeta;

namespace GestaoResiduosESG.Application.Validators;

public class PontoColetaCreateValidator : AbstractValidator<PontoColetaCreateViewModel>
{
    public PontoColetaCreateValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("O nome é obrigatório")
            .MaximumLength(100).WithMessage("O nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Localizacao)
            .NotEmpty().WithMessage("A localização é obrigatória")
            .MaximumLength(200).WithMessage("A localização deve ter no máximo 200 caracteres");

        RuleFor(x => x.CapacidadeMaxima)
            .GreaterThan(0).WithMessage("A capacidade máxima deve ser maior que zero");
    }
}
