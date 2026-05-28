using Product.DTOs;

namespace Product.Services.Inteface;

public interface IProductServices
{
    Task<List<ProductDto>> GetProducts(int pageNumber = 1, int pageSize = 10);

    Task<int> GetProductsCountPage(int pageSize = 10);
    
    Task<FullProductDto> PostProduct(ProductFromFormDto product);
    
    Task<bool> DeleteProduct(int productId);
    Task<bool> PathProduct(int productId, UpdateProductDto update);
}