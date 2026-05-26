using Microsoft.EntityFrameworkCore;
using Product.Data;
using Product.DTOs;
using Product.Services.Inteface;

namespace Product.Services;

public class ProductServices : IProductServices
{
    private readonly ProductDbContext _dbContext;

    public ProductServices(ProductDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<ProductDto>> GetProducts(int pageNumber = 1, int pageSize = 10)
    {
        var products = await _dbContext.ProductItems
            .Where(p => p.IsActive == true && p.IsDeleted == false)
            .Select(product => new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                QuantityInStock = product.QuantityInStock,
                ImgPatch = product.ImgPath
            })
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return products;
    }

    public async Task<int> GetProductsCountPage(int pageSize = 10)
    {
        var count = await _dbContext
            .ProductItems
            .Where(p => p.IsActive == true && p.IsDeleted == false)
            .CountAsync();
        var totalPages = (int)Math.Ceiling((double)count / pageSize);

        return totalPages;
    }

    private async Task<string> UploudImg(ProductFromBodyDto product)
    {
        var uploadsFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads"
        );
        
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);
        
        var fileName = Guid.NewGuid() + 
                       Path.GetExtension(product.Image.FileName);
        var filePath = Path.Combine(uploadsFolder, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        { 
            await product.Image.CopyToAsync(stream);
        }
        
        var imageUrl = "/Product/image/" + fileName;
        return imageUrl;
    }
    public async Task<UpdateProductIdDto> PostProduct(ProductFromBodyDto product)
    {
        var imageUrl = await UploudImg(product);
        
        var newProduct = await _dbContext.ProductItems.AddAsync(new ProductItem() {
            Name = product.Name,
            Price = product.Price,
            QuantityInStock = product.QuantityInStock,
            IsActive = product.IsActive,
            ImgPath = imageUrl
        });
        await _dbContext.SaveChangesAsync();
        
        return new UpdateProductIdDto
        {
            Id = newProduct.Entity.Id,
            Name = newProduct.Entity.Name,
            Price = newProduct.Entity.Price,
            ImgPatch = newProduct.Entity.ImgPath,
            QuantityInStock = newProduct.Entity.QuantityInStock,
            IsActive = newProduct.Entity.IsActive
        };
    }

    private async Task<ProductItem?> GetProduct(int productId)
    {
        return await _dbContext.ProductItems.FirstOrDefaultAsync(p => p.Id == productId);
    }
    
    public async Task<bool> DeleteProduct(int productId)
    {
        ProductItem? product = await GetProduct(productId);
        if (product == null || product.IsDeleted) return false;
        
        product.IsDeleted = true;
        
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PathProduct(int productId, UpdateProductDto update)
    {
        ProductItem? product = await GetProduct(productId);
        if (product == null || product.IsDeleted) return false;
        
        if (update.Name != null) { product.Name = update.Name;}
        if (update.Price.HasValue) { product.Price = update.Price.Value;}
        if (update.QuantityInStock.HasValue) {product.QuantityInStock = update.QuantityInStock.Value;}

        if (update.IsActive.HasValue)
        {
            product.IsActive = update.IsActive.Value;
        }
        
        await _dbContext.SaveChangesAsync();
        return true;
    }
}