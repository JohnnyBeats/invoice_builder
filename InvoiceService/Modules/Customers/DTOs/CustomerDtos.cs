namespace InvoiceService.Modules.Customers.DTOs;

public record CustomerResponse(
    Guid Id,
    string CompanyName,
    string ContactPerson,
    string Address,
    string Email,
    string PostalCode,
    string? VatTaxId,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateCustomerRequest(
    string CompanyName,
    string ContactPerson,
    string Address,
    string Email,
    string PostalCode,
    string? VatTaxId);

public record UpdateCustomerRequest(
    string CompanyName,
    string ContactPerson,
    string Address,
    string Email,
    string PostalCode,
    string? VatTaxId);
