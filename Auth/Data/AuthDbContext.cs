using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AuthDbContext : IdentityDbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region seed data for roles
        var adminRoleId = "1c4d89d9-f6b0-4139-ab1c-f64f9388dd87";
        var customerRoleId = "788912e9-158a-406c-b789-673f64ded9d7";

        var roles = new List<IdentityRole>
        {
            new IdentityRole
            {
                Id = adminRoleId,
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = adminRoleId
            },
            new IdentityRole
            {
                Id = customerRoleId,
                Name = "Customer",
                NormalizedName = "CUSTOMER",
                ConcurrencyStamp = customerRoleId
            }
        };

        modelBuilder.Entity<IdentityRole>().HasData(roles);
        #endregion

        base.OnModelCreating(modelBuilder);
    }
}