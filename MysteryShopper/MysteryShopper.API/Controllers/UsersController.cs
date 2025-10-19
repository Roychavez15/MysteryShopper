using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MysteryShopper.API.Contracts.DTOs;
using MysteryShopper.API.Domain.Identity;

namespace MysteryShopper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // POST api/users
        [HttpPost]
        [Authorize(Roles = Roles.Admin + "," + Roles.Client)]
        public async Task<IActionResult> CreateUser(UserCreateDto dto)
        {
            if (!await _roleManager.RoleExistsAsync(dto.Role))
                return BadRequest($"Role {dto.Role} not valid.");

            var currentUser = await _userManager.GetUserAsync(User);

            // si es Client → solo puede crear Evaluator
            if (User.IsInRole(Roles.Client) && dto.Role != Roles.Evaluator)
                return Forbid("Clients can only create evaluators.");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            // si es Client → asignar automáticamente su compañía
            if (User.IsInRole(Roles.Client))
                user.CompanyId = currentUser!.CompanyId;

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, dto.Role);

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                Role = dto.Role,
                IsActive = true
            });
        }

        // GET api/users?role=Evaluator
        [HttpGet]
        [Authorize(Roles = Roles.Admin + "," + Roles.Client)]
        public async Task<IActionResult> GetUsers([FromQuery] string? role = null)
        {
            var users = _userManager.Users.ToList();

            var dtos = new List<UserDto>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                if (role == null || roles.Contains(role))
                {
                    dtos.Add(new UserDto
                    {
                        Id = u.Id,
                        Email = u.Email!,
                        Role = roles.FirstOrDefault() ?? "",
                        IsActive = true
                    });
                }
            }

            return Ok(dtos);
        }

        // DELETE api/users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin + "," + Roles.Client)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }
    }

}
