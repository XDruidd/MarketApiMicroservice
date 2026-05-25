using Microsoft.EntityFrameworkCore;

namespace Product.Data;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) {}
    
    public DbSet<ProductItem> ProductItems { get; set; }

}