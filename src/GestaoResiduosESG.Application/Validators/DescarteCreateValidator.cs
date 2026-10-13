using FluentValidation;
using GestaoResiduosESG.Application.ViewModels.Descarte;

namespace GestaoResiduosESG.Application.Validators;

public class DescarteCreateValidator : AbstractValidator<DescarteCreateViewModel>
{
    public DescarteCreateValidator()
    {
        RuleFor(x => x.ResiduoId)
            .GreaterThan(0).WithMessage("ResiduoId inválido");

        RuleFor(x => x.PontoColetaId)
            .GreaterThan(0).WithMessage("PontoColetaId inválido");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("A quantidade deve ser maior que zero");

        RuleFor(x => x.DataDescarte)
            .NotEmpty().WithMessage("A data do descarte é obrigatória")
            .LessThanOrEqualTo(DateTime.Now).WithMessage("A data do descarte não pode ser futura");

        RuleFor(x => x.Responsavel)
            .NotEmpty().WithMessage("O responsável é obrigatório")
            .MaximumLength(100).WithMessage("O responsável deve ter no máximo 100 caracteres");
    }
}
