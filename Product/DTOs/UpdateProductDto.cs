using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Product.DTOs;

public class UpdateProductDto
{
    [MinLength(3)]
    public string? Name { get; set; }
    
    [Range(0.1, int.MaxValue)]
    public decimal? Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? QuantityInStock { get; set; }
    public string ImgPatch { get; set; }
    
    public bool? IsActive { get; set; } = true;
}

public class UpdateProductIdDto:UpdateProductDto
{
    public int Id { get; set; }
}