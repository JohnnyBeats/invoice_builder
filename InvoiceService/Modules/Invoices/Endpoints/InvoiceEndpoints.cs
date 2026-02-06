using FluentValidation;
using InvoiceService.Modules.Invoices.DTOs;
using InvoiceService.Modules.Invoices.Services;
using InvoiceService.Shared.Middleware;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Invoices.Endpoints;

public static class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").WithTags("Invoices");

        group.MapGet("/", GetAll)
            .WithName("GetAllInvoices")
            .WithSummary("List all invoices")
            .WithDescription("Returns a paginated list of invoices with summary information including status, grand total, and associated customer/sender names.")
            .Produces<PagedResult<InvoiceListResponse>>()
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetInvoiceById")
            .WithSummary("Get an invoice by ID")
            .WithDescription("Returns the full invoice details including all line items, calculated subtotal, tax amount, and grand total.")
            .Produces<InvoiceResponse>()
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group.MapPost("/", Create)
            .WithName("CreateInvoice")
            .WithSummary("Create a new invoice")
            .WithDescription("Creates a new invoice with line items. An invoice number is auto-generated. At least one line item is required. Subtotal, tax, and grand total are calculated automatically.")
            .Produces<InvoiceResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi();

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateInvoice")
            .WithSummary("Update an existing invoice")
            .WithDescription("Updates an invoice including its status and line items. All existing line items are replaced with the provided set. Valid statuses: Draft, Sent, Paid, Overdue, Cancelled.")
            .Produces<InvoiceResponse>()
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithOpenApi();

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteInvoice")
            .WithSummary("Delete an invoice")
            .WithDescription("Permanently deletes an invoice and all its associated line items.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group.MapGet("/{id:guid}/pdf", ExportPdf)
            .WithName("ExportInvoicePdf")
            .WithSummary("Download invoice as PDF")
            .WithDescription("Generates and downloads a PDF document for the specified invoice. The PDF includes full invoice details, line items, and calculated totals.")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }

    private static async Task<IResult> GetAll(
        IInvoiceService service,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(new PaginationParams(page, pageSize), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        IInvoiceService service,
        CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateInvoiceRequest request,
        IValidator<CreateInvoiceRequest> validator,
        IInvoiceService service,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var result = await service.CreateAsync(request, ct);
        return Results.Created($"/api/invoices/{result.Id}", result);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateInvoiceRequest request,
        IValidator<UpdateInvoiceRequest> validator,
        IInvoiceService service,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var result = await service.UpdateAsync(id, request, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> Delete(
        Guid id,
        IInvoiceService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ExportPdf(
        Guid id,
        IPdfService pdfService,
        CancellationToken ct)
    {
        var pdfBytes = await pdfService.GenerateInvoicePdfAsync(id, ct);
        return Results.File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
    }
}
