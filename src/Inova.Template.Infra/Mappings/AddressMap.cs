using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Inova.Template.Domain.Models;

namespace Inova.Template.Infra.Mappings
{
    public class AddressMap : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("Address", "dbo");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CEP)
                .HasColumnType("VARCHAR(8)")
                .HasMaxLength(8)
                .IsRequired();

            builder.HasMany(x => x.Customers)
                .WithOne(x => x.Address)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
