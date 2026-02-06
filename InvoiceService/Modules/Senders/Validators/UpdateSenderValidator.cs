using FluentValidation;
using InvoiceService.Modules.Senders.DTOs;

namespace InvoiceService.Modules.Senders.Validators;

public class UpdateSenderValidator : AbstractValidator<UpdateSenderRequest>
{
    public UpdateSenderValidator()
    {
        RuleFor(x => x.CompanyName)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(200);

        RuleFor(x => x.ContactPerson)
            .NotEmpty().WithMessage("Contact person is required.")
            .MaximumLength(150);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(254);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(30);

        RuleFor(x => x.VatTaxId)
            .MaximumLength(50);

        RuleFor(x => x.Iban)
            .MaximumLength(34);
    }
}
