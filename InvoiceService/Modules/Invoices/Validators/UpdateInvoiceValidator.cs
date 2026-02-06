using FluentValidation;
using InvoiceService.Modules.Invoices.Domain;
using InvoiceService.Modules.Invoices.DTOs;

namespace InvoiceService.Modules.Invoices.Validators;

public class UpdateInvoiceValidator : AbstractValidator<UpdateInvoiceRequest>
{
    public UpdateInvoiceValidator()
    {
        RuleFor(x => x.InvoiceDate)
            .NotEmpty().WithMessage("Invoice date is required.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required.")
            .GreaterThanOrEqualTo(x => x.InvoiceDate)
            .WithMessage("Due date must be on or after the invoice date.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.TaxRate)
            .GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative.")
            .LessThanOrEqualTo(100).WithMessage("Tax rate cannot exceed 100%.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000);

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => Enum.TryParse<InvoiceStatus>(s, true, out _))
            .WithMessage("Invalid invoice status. Valid values: Draft, Sent, Paid, Overdue, Cancelled.");

        RuleFor(x => x.SenderId)
            .NotEmpty().WithMessage("Sender is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer is required.");

        RuleFor(x => x.LineItems)
            .NotEmpty().WithMessage("At least one line item is required.");

        RuleForEach(x => x.LineItems).SetValidator(new CreateInvoiceLineItemValidator());
    }
}
