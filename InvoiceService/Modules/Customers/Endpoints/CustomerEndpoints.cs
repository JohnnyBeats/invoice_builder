using FluentValidation;
using InvoiceService.Modules.Customers.DTOs;
using InvoiceService.Modules.Customers.Services;
using InvoiceService.Shared.Middleware;
using InvoiceService.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace InvoiceService.Modules.Customers.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers");

        group.MapGet("/", GetAll)
            .WithName("GetAllCustomers")
            .WithSummary("List all customers")
            .WithDescription("Returns a paginated list of all customers, ordered by creation date.")
            .Produces<PagedResult<CustomerResponse>>()
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetCustomerById")
            .WithSummary("Get a customer by ID")
            .WithDescription("Returns the full details of a single customer.")
            .Produces<CustomerResponse>()
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithOpenApi();

        group.MapPost("/", Create)
            .WithName("CreateCustomer")
            .WithSummary("Create a new customer")
            .WithDescription("Creates a new customer record. All fields except VatTaxId are required.")
            .Produces<CustomerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .WithOpenApi();

        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateCustomer")
            .WithSummary("Update an existing customer")
            .WithDescription("Updates all fields of an existing customer.")
            .Produces<CustomerResponse>()
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithOpenApi();

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteCustomer")
            .WithSummary("Delete a customer")
            .WithDescription("Permanently deletes a customer. Will fail if the customer has associated invoices.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }

    private static async Task<IResult> GetAll(
        ICustomerService service,
        int page = 1,
        int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(new PaginationParams(page, pageSize), ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetById(
        Guid id,
        ICustomerService service,
        CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return Results.Ok(result);
    }

    private static async Task<IResult> Create(
        CreateCustomerRequest request,
        IValidator<CreateCustomerRequest> validator,
        ICustomerService service,
        CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var result = await service.CreateAsync(request, ct);
        return Results.Created($"/api/customers/{result.Id}", result);
    }

    private static async Task<IResult> Update(
        Guid id,
        UpdateCustomerRequest request,
        IValidator<UpdateCustomerRequest> validator,
        ICustomerService service,
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
        ICustomerService service,
        CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return Results.NoContent();
    }
}
