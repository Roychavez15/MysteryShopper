using System.ComponentModel.DataAnnotations;

namespace MysteryShopper.API.Contracts.DTOs
{
    public class UserCreateDto
    {
        [Required] public string Email { get; set; } = default!;
        [Required] public string Password { get; set; } = default!;
        [Required] public string Role { get; set; } = default!;
    }
}
