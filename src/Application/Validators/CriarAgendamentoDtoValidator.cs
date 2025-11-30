using Application.DTOs;
using FluentValidation;

namespace Application.Validators;

public class CriarAgendamentoDtoValidator : AbstractValidator<CriarAgendamentoDto>
{
    public CriarAgendamentoDtoValidator()
    {
        RuleFor(x => x.ClienteNome)
            .NotEmpty().WithMessage("O nome do cliente é obrigatório.")
            .MaximumLength(200).WithMessage("O nome do cliente não pode exceder 200 caracteres.");

        RuleFor(x => x.ClienteTelefone)
            .NotEmpty().WithMessage("O telefone do cliente é obrigatório.")
            .MaximumLength(20).WithMessage("O telefone não pode exceder 20 caracteres.");

        RuleFor(x => x.DataHoraInicio)
            .NotEmpty().WithMessage("A data/hora de início é obrigatória.")
            .Must(BeFutureDate).WithMessage("A data/hora de início deve ser futura.");

        RuleFor(x => x.DataHoraFim)
            .NotEmpty().WithMessage("A data/hora de fim é obrigatória.")
            .Must(BeFutureDate).WithMessage("A data/hora de fim deve ser futura.")
            .GreaterThan(x => x.DataHoraInicio)
            .WithMessage("A data/hora de fim deve ser posterior à data/hora de início.");

        RuleFor(x => x.Observacoes)
            .MaximumLength(1000).WithMessage("As observações não podem exceder 1000 caracteres.")
            .When(x => !string.IsNullOrEmpty(x.Observacoes));
    }

    private static bool BeFutureDate(DateTime date)
    {
        return date > DateTime.Now;
    }
}

