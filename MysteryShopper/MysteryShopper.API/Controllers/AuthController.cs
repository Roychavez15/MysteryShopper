using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using MysteryShopper.API.Contracts;
using MysteryShopper.API.Domain.Identity;
using System.Text;

namespace MysteryShopper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly IConfiguration _cfg;

        public AuthController(UserManager<ApplicationUser> userMgr, IConfiguration cfg)
        { _userMgr = userMgr; _cfg = cfg; }

        [HttpPost("register")] // Admin creates Admin/Client/Evaluator; Client can create Evaluators under its company
        public async Task<IActionResult> Register(RegisterRequest req)
        {
            if (req.Role is not (Roles.Admin or Roles.Client or Roles.Evaluator))
                return BadRequest("Invalid role");

            var user = new ApplicationUser { UserName = req.Email, Email = req.Email, EmailConfirmed = true, CompanyId = req.CompanyId };
            var result = await _userMgr.CreateAsync(user, req.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            await _userMgr.AddToRoleAsync(user, req.Role);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest req)
        {
            var user = await _userMgr.FindByEmailAsync(req.Email);
            if (user is null) return Unauthorized();
            if (!await _userMgr.CheckPasswordAsync(user, req.Password)) return Unauthorized();

            var roles = await _userMgr.GetRolesAsync(user);
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Role, roles.First())
        };

            var jwt = _cfg.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            return new AuthResponse(tokenStr);
        }
    }

}
