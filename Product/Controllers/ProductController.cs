using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.DTOs;
using Product.Services.Inteface;

namespace Product.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductServices _productServices;
    
    public ProductController(ILogger<ProductController> logger, IProductServices productServices)
    {
        _logger = logger;
        _productServices = productServices;
    }
    
    [HttpGet("page/count")]
    public async Task<IActionResult> GetPage()
    {
        return Ok(new {countPage = await _productServices.GetProductsCountPage()});
    }
    
    [HttpGet("{pageId}")]
    public async Task<IActionResult> Get(int pageId)
    {
        if (pageId < 1 || pageId > await _productServices.GetProductsCountPage()) { return BadRequest(); } 
        
        var products = await _productServices.GetProducts(pageId);
        if (products.Count == 0) { return NoContent(); }
        
        return Ok(products);
    }

    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Post([FromForm] ProductFromFormDto product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (product.Image == null || product.Image.Length == 0)
        {
            return BadRequest();
        }
        var allowedTypes = new[] { "image/jpeg", "image/png"};
        if (!allowedTypes.Contains(product.Image.ContentType))
            return BadRequest("Not a valid image type");

        if (product.Image.Length > 5 * 1024 * 1024)
        {
            return BadRequest("Too long image size");
        }
        
        var created = await _productServices.PostProduct(product);
        return StatusCode(201, created);
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productServices.DeleteProduct(id);
        if(deleted){ return Ok(new {message = "Deleted"} ); }
        
        return BadRequest(new { message = "Not found" });
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Patch(int id, [FromForm] UpdateProductDto product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (product.Image == null || product.Image.Length == 0)
        {
            return BadRequest();
        }
        var allowedTypes = new[] { "image/jpeg", "image/png"};
        if (!allowedTypes.Contains(product.Image.ContentType))
            return BadRequest("Not a valid image type");

        if (product.Image.Length > 5 * 1024 * 1024)
        {
            return BadRequest("Too long image size");
        }
        
        var path = await _productServices.PathProduct(id, product);
        if (path) { return Ok(new {message = "Updated"}); }
        
        return BadRequest();
    }
    
}