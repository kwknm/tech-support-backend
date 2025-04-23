using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TechSupport.Contracts.Requests;
using TechSupport.Database;

namespace TechSupport.Validators;

public class CreateFaqRequestValidator : AbstractValidator<CreateFaqRequest>
{
    public CreateFaqRequestValidator()
    {
        RuleFor(x => x.Title).Length(6, 128).NotEmpty();
        RuleFor(x => x.Content).Length(32, 4096).NotEmpty();
    }
}