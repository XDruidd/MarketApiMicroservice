using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Register.DTOs;

namespace Market.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        private readonly UserManager<IdentityUser> _userManager;
        private readonly TokenService _tokenService;

        public AuthController(ILogger<AuthController> logger, UserManager<IdentityUser> userManager, TokenService tokenService)
        {
            _logger = logger;

            _userManager = userManager;

            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var identityUser = new IdentityUser
            {
                Email = registerDto.Email,
                UserName = registerDto.Email,

            };
            var identityResult = await _userManager.CreateAsync(identityUser, registerDto.Password);
            if (identityResult.Succeeded)
            {
                string role = registerDto.Email == "admin@gmail.com" ? "Admin" : "Customer";
                
                identityResult = await _userManager.AddToRolesAsync(identityUser, [role]);
                if (identityResult.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    var token = _tokenService.CreateJWTTokenAsync(identityUser, roles);
                    return Ok(new { Message = "Register successful", Token = token });
                }

                return BadRequest(new
                {
                    Message = "User registration is successfull but cannot assign specifiedrole to it!",
                    Errors = identityResult.Errors.Select(e => e.Description)
                });
            }

            return BadRequest(new
            {
                Message = "User registration failed!",
                Errors = identityResult.Errors.Select(e => e.Description)
            });

        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var identityUser = await _userManager.FindByNameAsync(loginDto.Username);
            if (identityUser != null)
            {
                var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, loginDto.Password);
                if (isPasswordValid)
                {
                    //Create token along with claims with roles
                    var roles = await _userManager.GetRolesAsync(identityUser);
                    if (roles != null && roles.Count > 0)
                    {

                        var token = _tokenService.CreateJWTTokenAsync(identityUser, roles);
                        if (roles.Contains("Admin"))
                        {
                            return Ok(new 
                            { 
                                Message = "Login successful", 
                                Token = token, 
                                Role = "Admin" 
                            });
                        }
                        return Ok(new { Message = "Login successful", Token = token});
                    }

                    return Unauthorized(new { Message = "User has no roles assigned contact admin" });

                }
                return Unauthorized(new { Message = "Invalid password" });
            }

            return Unauthorized(new { Message = "Invalid username" });
        }
}
public class TokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateJWTTokenAsync(IdentityUser user, IList<string> roles)
    {
        // create claims from roles and user information
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = credentials,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

}