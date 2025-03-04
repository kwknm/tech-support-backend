using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TechSupport.Contracts.Requests;
using TechSupport.Database;

namespace TechSupport.Validators;

public class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest>
{
    public CreateTicketRequestValidator(ApplicationDbContext context)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Тема не должна быть пустой")
            .Length(6, 128).WithMessage("Минимальная длина 6 символов, максимальная – 128");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Описание обязательно");
        RuleFor(x => x.IssueTypeId)
            .MustAsync(async (issueTypeId, ct) =>
            {
                return await context.IssueTypes.AsNoTracking().AnyAsync(issue => issue.Id == issueTypeId, ct);
            }).WithMessage("Неверный тип проблемы").WithName("Тип проблемы");
        RuleFor(x => x.Terms).Must(x => x)
            .WithMessage("Пожалуйста, подтвердите согласие на обработку персональных данных");
    }
}