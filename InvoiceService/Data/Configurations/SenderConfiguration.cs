using InvoiceService.Modules.Senders.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InvoiceService.Data.Configurations;

public class SenderConfiguration : IEntityTypeConfiguration<Sender>
{
    public void Configure(EntityTypeBuilder<Sender> builder)
    {
        builder.ToTable("senders");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.ContactPerson)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Address)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(s => s.Phone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(s => s.VatTaxId)
            .HasMaxLength(50);

        builder.Property(s => s.Iban)
            .HasMaxLength(34);

        builder.HasIndex(s => s.CompanyName);
        builder.HasIndex(s => s.Email);
    }
}
