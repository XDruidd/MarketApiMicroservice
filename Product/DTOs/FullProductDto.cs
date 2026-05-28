using System.ComponentModel.DataAnnotations;

namespace Product.DTOs;

public class FullProductDto
{
    public int Id { get; set; }

    [MinLength(3)]
    public string? Name { get; set; }
    
    [Range(0.1, int.MaxValue)]
    public decimal? Price { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? QuantityInStock { get; set; }
    
    public string ImgPatch{ get; set; }
    
    public bool? IsActive { get; set; } = true;
}