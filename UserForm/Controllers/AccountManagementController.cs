using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FormGenerator.DTOS;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using UserForm.Models.DBModels.Users;

namespace UserForm.Controllers;

[ApiController]
[Route("api/[controller]")]
class AccountManagementController(
    UserManager<UserDetails> userManager,
    IConfiguration configuration,
    RoleManager<IdentityRole> roleManager)
    : Controller
{

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto model)
    {
        var user = await userManager.FindByEmailAsync(model.Email);
        if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }

        return Unauthorized();
    }

    
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        var user = new UserDetails { UserName = model.Email, Email = model.Email };
        var result = await userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Ensure role exists
        if (!await roleManager.RoleExistsAsync(model.Role))
            await roleManager.CreateAsync(new IdentityRole(model.Role));

        // Assign role to user
        await userManager.AddToRoleAsync(user, model.Role);

        return Ok("User registered with role: " + model.Role);
    }


}