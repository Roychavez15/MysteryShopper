namespace MysteryShopper.API.Contracts.DTOs
{
    public record CompanyDto(Guid Id, string Name, string? Notes, DateTime CreatedAt, string? CreatedBy);
    public record CompanyCreateDto(string Name, string? Notes);
    public record CompanyUpdateDto(string Name, string? Notes);
}
