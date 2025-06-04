using BE.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BE.Models; // Make sure this is here

namespace BE.Data;

public class BEContext : IdentityDbContext<BEUser>
{
    public BEContext(DbContextOptions<BEContext> options)
        : base(options)
    {
    }

    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        // NEW: Configure precision and scale for decimal properties
        builder.Entity<CartItem>(entity =>
        {
            // Configure the 'Price' property in CartItem
            // Example: DECIMAL(18, 2) means 18 total digits, 2 after the decimal point.
            // Adjust precision and scale as needed for your application's prices.
            // If you have prices like 123.456, you might need (18, 3) or higher.
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });
    }
}