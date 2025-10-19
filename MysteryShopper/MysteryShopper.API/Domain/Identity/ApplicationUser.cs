namespace MysteryShopper.API.Domain.Identity
{
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser
    {        
        public Guid? CompanyId { get; set; }
        public Company? Company { get; set; }
    }

    public static class Roles
    {
        public const string Admin = "ADMIN";      // Super admin: manages Companies
        public const string Client = "CLIENTE";   // Client user: manages its Agencies, Employees, Surveys
        public const string Evaluator = "EVALUADOR"; // Mystery shopper evaluator
    }
}
