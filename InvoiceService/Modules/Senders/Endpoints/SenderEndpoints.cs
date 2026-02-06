using FluentValidation;
using InvoiceService.Modules.Senders.DTOs;
using InvoiceService.Modules.Senders.Services;
using InvoiceService.Shared.Middleware;
using InvoiceService.Shared.Models;

namespace InvoiceService.Modules.Senders.Endpoints;

public static class SenderEndpoints
{
    public static void MapSenderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/senders").WithTags("Senders");

        group.MapGet("/", GetAll)
            .WithName("GetAllSenders")
            .WithSummary("List all senders")
            .WithDescription("Returns a paginated list of all senders, ordered by creation date.")
            .Produces<PagedResult<SenderResponse>>()
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetSenderById")
            .WithSummary("Get a sender by ID")
            .WithDescription("Returns the full details of a single sender.")
            .Produces<SenderResponse>()
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group.MapPost("/", Create)
            .WithName("CreateSender")
            .WithSummary("Create a new sender")
            .WithDescription("Creates a new sender record. VatTaxId and Iban are optional.")
            .Produces<SenderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi();

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateSender")
            .WithSummary("Update an existing sender")
            .WithDescription("Updates all fields of an existing sender.")
            .Produces<SenderResponse>()
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithOpenApi();

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteSender")
            .WithSummary("Delete a sender")
            .WithDescription("Permanently deletes a sender. Will fail if the sender has associated invoices.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }

    private static async Task<IResult> GetAll(
        ISenderService service,
        int page = 1,
        int pageSize = 10,
        string? search = null,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(new PaginationParams(page, pageSize, search), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        ISenderService service,
        CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateSenderRequest request,
        IValidator<CreateSenderRequest> validator,
        ISenderService service,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var result = await service.CreateAsync(request, ct);
        return Results.Created($"/api/senders/{result.Id}", result);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateSenderRequest request,
        IValidator<UpdateSenderRequest> validator,
        ISenderService service,
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
        ISenderService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}
