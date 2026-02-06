namespace InvoiceService.Modules.Senders.DTOs;

public record SenderResponse(
    Guid Id,
    string CompanyName,
    string ContactPerson,
    string Address,
    string Email,
    string Phone,
    string? VatTaxId,
    string? Iban,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateSenderRequest(
    string CompanyName,
    string ContactPerson,
    string Address,
    string Email,
    string Phone,
    string? VatTaxId,
    string? Iban);

public record UpdateSenderRequest(
    string CompanyName,
    string ContactPerson,
    string Address,
    string Email,
    string Phone,
    string? VatTaxId,
    string? Iban);
