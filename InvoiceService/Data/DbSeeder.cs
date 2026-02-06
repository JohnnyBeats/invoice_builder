using InvoiceService.Modules.Customers.Domain;
using InvoiceService.Modules.Invoices.Domain;
using InvoiceService.Modules.Senders.Domain;
using Microsoft.EntityFrameworkCore;

namespace InvoiceService.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Customers.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // --- Customers ---
        var customers = new List<Customer>
        {
            new()
            {
                Id = Guid.NewGuid(), CompanyName = "Acme Corporation", ContactPerson = "John Smith",
                Address = "123 Business Ave, New York, NY 10001", Email = "john.smith@acme.com",
                PostalCode = "10001", VatTaxId = "US-VAT-98765", CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CompanyName = "TechStart Solutions", ContactPerson = "Sarah Johnson",
                Address = "456 Innovation Blvd, San Francisco, CA 94102", Email = "sarah@techstart.io",
                PostalCode = "94102", VatTaxId = "US-VAT-12345", CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CompanyName = "Global Retail Ltd", ContactPerson = "Michael Chen",
                Address = "789 Commerce St, London, EC2V 8AS", Email = "m.chen@globalretail.co.uk",
                PostalCode = "EC2V 8AS", VatTaxId = "GB-VAT-55443", CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CompanyName = "Nordic Design AB", ContactPerson = "Emma Lindqvist",
                Address = "Storgatan 12, 111 51 Stockholm", Email = "emma@nordicdesign.se",
                PostalCode = "111 51", VatTaxId = null, CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CompanyName = "Cape Ventures (Pty) Ltd", ContactPerson = "David Nkosi",
                Address = "22 Long Street, Cape Town, 8001", Email = "david@capeventures.co.za",
                PostalCode = "8001", VatTaxId = "ZA-VAT-4400112233", CreatedAt = now, UpdatedAt = now
            }
        };

        db.Customers.AddRange(customers);

        // --- Senders ---
        var senders = new List<Sender>
        {
            new()
            {
                Id = Guid.NewGuid(), CompanyName = "Alignd Consulting", ContactPerson = "Alex Morgan",
                Address = "100 Startup Lane, Austin, TX 73301", Email = "billing@alignd.io",
                Phone = "+1 512-555-0199", VatTaxId = "US-VAT-77889", Iban = "US12345678901234567890",
                CreatedAt = now, UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(), CompanyName = "Freelance Dev Studio", ContactPerson = "Priya Patel",
                Address = "42 Code Street, Bangalore, 560001", Email = "invoices@devstudio.in",
                Phone = "+91 80-5555-1234", VatTaxId = "IN-GST-29ABCDE", Iban = null,
                CreatedAt = now, UpdatedAt = now
            }
        };

        db.Senders.AddRange(senders);

        // --- Invoices ---
        var invoices = new List<Invoice>
        {
            new()
            {
                Id = Guid.NewGuid(), InvoiceNumber = "INV-0001",
                InvoiceDate = now.AddDays(-45), DueDate = now.AddDays(-15),
                Currency = "USD", TaxRate = 15.00m, Notes = "Payment due within 30 days of invoice date.",
                Status = InvoiceStatus.Paid,
                SenderId = senders[0].Id, CustomerId = customers[0].Id,
                CreatedAt = now.AddDays(-45), UpdatedAt = now.AddDays(-10),
                LineItems = new List<InvoiceLineItem>
                {
                    new() { Id = Guid.NewGuid(), Description = "Website Redesign", Quantity = 1, UnitPrice = 4500.00m, CreatedAt = now, UpdatedAt = now },
                    new() { Id = Guid.NewGuid(), Description = "SEO Optimization", Quantity = 3, UnitPrice = 800.00m, CreatedAt = now, UpdatedAt = now },
                    new() { Id = Guid.NewGuid(), Description = "Content Writing (per article)", Quantity = 10, UnitPrice = 150.00m, CreatedAt = now, UpdatedAt = now }
                }
            },
            new()
            {
                Id = Guid.NewGuid(), InvoiceNumber = "INV-0002",
                InvoiceDate = now.AddDays(-30), DueDate = now,
                Currency = "USD", TaxRate = 15.00m, Notes = "Net 30. Late payments subject to 2% monthly interest.",
                Status = InvoiceStatus.Sent,
                SenderId = senders[0].Id, CustomerId = customers[1].Id,
                CreatedAt = now.AddDays(-30), UpdatedAt = now.AddDays(-30),
                LineItems = new List<InvoiceLineItem>
                {
                    new() { Id = Guid.NewGuid(), Description = "Cloud Architecture Consulting (hours)", Quantity = 40, UnitPrice = 175.00m, CreatedAt = now, UpdatedAt = now },
                    new() { Id = Guid.NewGuid(), Description = "CI/CD Pipeline Setup", Quantity = 1, UnitPrice = 3200.00m, CreatedAt = now, UpdatedAt = now }
                }
            },
            new()
            {
                Id = Guid.NewGuid(), InvoiceNumber = "INV-0003",
                InvoiceDate = now.AddDays(-60), DueDate = now.AddDays(-30),
                Currency = "GBP", TaxRate = 20.00m, Notes = null,
                Status = InvoiceStatus.Overdue,
                SenderId = senders[0].Id, CustomerId = customers[2].Id,
                CreatedAt = now.AddDays(-60), UpdatedAt = now.AddDays(-60),
                LineItems = new List<InvoiceLineItem>
                {
                    new() { Id = Guid.NewGuid(), Description = "E-commerce Platform Development", Quantity = 1, UnitPrice = 12000.00m, CreatedAt = now, UpdatedAt = now },
                    new() { Id = Guid.NewGuid(), Description = "Payment Gateway Integration", Quantity = 1, UnitPrice = 2500.00m, CreatedAt = now, UpdatedAt = now },
                    new() { Id = Guid.NewGuid(), Description = "QA & Testing (hours)", Quantity = 20, UnitPrice = 95.00m, CreatedAt = now, UpdatedAt = now },
                    new() { Id = Guid.NewGuid(), Description = "Project Management (hours)", Quantity = 15, UnitPrice = 120.00m, CreatedAt = now, UpdatedAt = now }
                }
            },
            new()
            {
                Id = Guid.NewGuid(), InvoiceNumber = "INV-0004",
                InvoiceDate = now.AddDays(-5), DueDate = now.AddDays(25),
                Currency = "USD", TaxRate = 0.00m, Notes = "Milestone 1 of 3. No tax — reverse charge applies.",
                Status = InvoiceStatus.Draft,
                SenderId = senders[1].Id, CustomerId = customers[3].Id,
                CreatedAt = now.AddDays(-5), UpdatedAt = now.AddDays(-5),
                LineItems = new List<InvoiceLineItem>
                {
                    new() { Id = Guid.NewGuid(), Description = "Mobile App UI/UX Design", Quantity = 1, UnitPrice = 6000.00m, CreatedAt = now, UpdatedAt = now },
                    new() { Id = Guid.NewGuid(), Description = "Prototype Development (React Native)", Quantity = 1, UnitPrice = 8500.00m, CreatedAt = now, UpdatedAt = now }
                }
            },
            new()
            {
                Id = Guid.NewGuid(), InvoiceNumber = "INV-0005",
                InvoiceDate = now.AddDays(-10), DueDate = now.AddDays(20),
                Currency = "ZAR", TaxRate = 15.00m, Notes = "Bank details on file. EFT preferred.",
                Status = InvoiceStatus.Sent,
                SenderId = senders[0].Id, CustomerId = customers[4].Id,
                CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-10),
                LineItems = new List<InvoiceLineItem>
                {
                    new() { Id = Guid.NewGuid(), Description = "API Integration Consulting (hours)", Quantity = 25, UnitPrice = 1200.00m, CreatedAt = now, UpdatedAt = now },
                    new() { Id = Guid.NewGuid(), Description = "Database Migration", Quantity = 1, UnitPrice = 18000.00m, CreatedAt = now, UpdatedAt = now },
                    new() { Id = Guid.NewGuid(), Description = "Staff Training Session", Quantity = 2, UnitPrice = 5000.00m, CreatedAt = now, UpdatedAt = now }
                }
            }
        };

        db.Invoices.AddRange(invoices);

        await db.SaveChangesAsync();
    }
}
