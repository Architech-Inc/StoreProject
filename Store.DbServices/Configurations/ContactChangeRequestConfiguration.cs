using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Store.Models.Entities.Contacts;

namespace Store.DbServices.Configurations;

public class ContactChangeRequestConfiguration : IEntityTypeConfiguration<ContactChangeRequest>
{
    public void Configure(EntityTypeBuilder<ContactChangeRequest> builder)
    {
        builder.ToTable("ContactChangeRequests");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
               .WithMany(u => u.ContactChangeRequests)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Status)
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.HasOne(x => x.ApprovedBy)
               .WithMany(u => u.ApprovedContactChanges)
               .HasForeignKey(x => x.ApprovedById)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
