using System.ComponentModel.DataAnnotations;

namespace Product.Data;

public class ProductItem
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MinLength(3)]
    public string Name { get; set; }
    
    [Required]
    [Range(0.1, int.MaxValue)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; }

    [Required] 
    public string ImgPath { get; set; }    
    
    [Required]
    public bool IsActive { get; set; } = true;
    
    public bool IsDeleted { get; set; } = false;
}