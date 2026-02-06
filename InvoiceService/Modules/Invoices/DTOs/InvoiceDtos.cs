using InvoiceService.Modules.Invoices.Domain;

namespace InvoiceService.Modules.Invoices.DTOs;

public record InvoiceResponse(
    Guid Id,
    string InvoiceNumber,
    DateTime InvoiceDate,
    DateTime DueDate,
    string Currency,
    decimal TaxRate,
    string? Notes,
    string Status,
    Guid SenderId,
    string SenderCompanyName,
    Guid CustomerId,
    string CustomerCompanyName,
    List<InvoiceLineItemResponse> LineItems,
    decimal SubTotal,
    decimal TaxAmount,
    decimal GrandTotal,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record InvoiceListResponse(
    Guid Id,
    string InvoiceNumber,
    DateTime InvoiceDate,
    DateTime DueDate,
    string Currency,
    string Status,
    string SenderCompanyName,
    string CustomerCompanyName,
    decimal GrandTotal,
    DateTime CreatedAt);

public record InvoiceLineItemResponse(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Total);

public record CreateInvoiceRequest(
    DateTime InvoiceDate,
    DateTime DueDate,
    string Currency,
    decimal TaxRate,
    string? Notes,
    Guid SenderId,
    Guid CustomerId,
    List<CreateInvoiceLineItemRequest> LineItems);

public record UpdateInvoiceRequest(
    DateTime InvoiceDate,
    DateTime DueDate,
    string Currency,
    decimal TaxRate,
    string? Notes,
    string Status,
    Guid SenderId,
    Guid CustomerId,
    List<CreateInvoiceLineItemRequest> LineItems);

public record CreateInvoiceLineItemRequest(
    string Description,
    decimal Quantity,
    decimal UnitPrice);
