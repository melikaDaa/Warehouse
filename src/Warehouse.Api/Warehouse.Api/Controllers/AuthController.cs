using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Warehouse.Api.Domain.Entities;
using Warehouse.Api.DTOs;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized("Invalid credentials");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return Unauthorized("Invalid credentials");

        var token = await GenerateJwtTokenAsync(user);
        return Ok(new
        {
            token,
            email = user.Email
        });
    }

    // فقط ادمین می‌تواند کاربر جدید بسازد
    [HttpPost("register")]
    [Authorize(Roles = "SystemAdmin")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // ۱. چک یوزر تکراری
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest("User already exists");

        // ۲. تعیین نقش معتبر (بدون اجازه ساخت SystemAdmin)
        var allowedRoles = new[] { "NormalUser", "WarehouseManager", "Auditor" };

        string roleToAssign;

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            // اگر نقشی نیامده بود → NormalUser
            roleToAssign = "NormalUser";
        }
        else if (allowedRoles.Contains(request.Role))
        {
            roleToAssign = request.Role;
        }
        else
        {
            // هر چیز غیر از این‌ها (از جمله SystemAdmin) مردود است
            return BadRequest("Invalid role. Allowed roles: NormalUser, WarehouseManager, Auditor.");
        }

        // ۳. ساخت یوزر
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        // ۴. اختصاص نقش معتبر
        var roleResult = await _userManager.AddToRoleAsync(user, roleToAssign);
        if (!roleResult.Succeeded)
        {
            return BadRequest(roleResult.Errors);
        }

        return Ok(new
        {
            message = "User created",
            email = user.Email,
            role = roleToAssign
        });
    }


    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"];
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var expiryMinutes = int.Parse(jwtSection["ExpiryMinutes"]!);

        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // اضافه کردن Roleها به Token
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var keyBytes = Encoding.UTF8.GetBytes(key!);
        var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
