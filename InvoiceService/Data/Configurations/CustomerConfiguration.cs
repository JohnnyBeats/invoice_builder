using InvoiceService.Modules.Customers.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceService.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ContactPerson)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(c => c.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.VatTaxId)
            .HasMaxLength(50);

        builder.HasIndex(c => c.CompanyName);
        builder.HasIndex(c => c.Email);
    }
}
