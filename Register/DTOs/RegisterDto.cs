using System.ComponentModel.DataAnnotations;

namespace Register.DTOs;

public class RegisterDto
{
    [Required]
    [StringLength(256)]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [MaxLength(32)]
    public string Password { get; set; }
}