using FluentValidation;
using InvoiceService.Data;
using InvoiceService.Modules.Customers.Endpoints;
using InvoiceService.Modules.Customers.Repository;
using InvoiceService.Modules.Customers.Services;
using InvoiceService.Modules.Invoices.Endpoints;
using InvoiceService.Modules.Invoices.Repository;
using InvoiceService.Modules.Invoices.Services;
using InvoiceService.Modules.Senders.Endpoints;
using InvoiceService.Modules.Senders.Repository;
using InvoiceService.Modules.Senders.Services;
using InvoiceService.Shared.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "Invoice Builder API",
            Version = "v1",
            Description = "RESTful API for managing customers, senders, and invoices with PDF export capabilities."
        };
        return Task.CompletedTask;
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

builder.Services.AddScoped<ISenderRepository, SenderRepository>();
builder.Services.AddScoped<ISenderService, SenderService>();

builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IInvoiceService, InvoiceAppService>();

builder.Services.AddScoped<IPdfService, PdfService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Invoice Builder API v1");
});

app.UseHttpsRedirection();

app.MapCustomerEndpoints();
app.MapSenderEndpoints();
app.MapInvoiceEndpoints();

app.Run();

public partial class Program { }
