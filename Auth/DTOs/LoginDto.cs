using System.ComponentModel.DataAnnotations;

namespace Auth.DTOs;

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