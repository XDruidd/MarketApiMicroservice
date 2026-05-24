using System.ComponentModel.DataAnnotations;

namespace Register.DTOs;

public class LoginDto
{
    [Required]
    [StringLength(256)]
    [DataType(DataType.EmailAddress)]
    public string Username { get; set; }
    [Required]
    [DataType(DataType.Password)]
    [MaxLength(32)]
    public string Password { get; set; }
}