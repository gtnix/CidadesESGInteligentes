using FluentValidation;
using GestaoResiduosESG.Application.ViewModels.Limite;

namespace GestaoResiduosESG.Application.Validators;

public class LimiteCreateValidator : AbstractValidator<LimiteCreateViewModel>
{
    public LimiteCreateValidator()
    {
        RuleFor(x => x.PontoColetaId)
            .GreaterThan(0).WithMessage("PontoColetaId inválido");

        RuleFor(x => x.ResiduoId)
            .GreaterThan(0).WithMessage("ResiduoId inválido");

        RuleFor(x => x.LimiteMinimo)
            .GreaterThanOrEqualTo(0).WithMessage("O limite mínimo deve ser maior ou igual a zero");

        RuleFor(x => x.LimiteMaximo)
            .GreaterThan(0).WithMessage("O limite máximo deve ser maior que zero")
            .GreaterThan(x => x.LimiteMinimo).WithMessage("O limite máximo deve ser maior que o limite mínimo");

        RuleFor(x => x.PercentualAlerta)
            .InclusiveBetween(1, 100).WithMessage("O percentual de alerta deve ser entre 1 e 100");
    }
}
