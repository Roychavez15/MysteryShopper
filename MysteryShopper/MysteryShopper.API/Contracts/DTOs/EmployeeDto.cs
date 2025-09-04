namespace MysteryShopper.API.Contracts.DTOs
{
    public record EmployeeDto(Guid Id, Guid AgencyId, string Name, string Role, DateTime CreatedAt, string? CreatedBy);
    public record EmployeeCreateDto(Guid AgencyId, string Name, string Role);
    public record EmployeeUpdateDto(string Name, string Role);
}
