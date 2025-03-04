using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TechSupport.Contracts.Requests;
using TechSupport.Database;

namespace TechSupport.Validators;

public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator(ApplicationDbContext context)
    {
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Неверный Email адрес")
            .MustAsync(async (email, ct) =>
            {
                return await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, ct) is null;
            }).WithMessage("Такой Email уже используется");
        RuleFor(x => x.Password).Length(3, 32)
            .Equal(x => x.PasswordConfirm).WithMessage("Пароли не совпадают").WithName("Пароль");
        RuleFor(x => x.FirstName).Length(1, 50);
        RuleFor(x => x.LastName).Length(1, 50);
    }
}