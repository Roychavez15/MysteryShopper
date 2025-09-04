namespace MysteryShopper.API.Contracts.DTOs
{
    //public record AgencyDto(Guid Id, Guid CompanyId, string Name, string? Address);
    //public record AgencyCreateDto(Guid CompanyId, string Name, string? Address);
    //public record AgencyUpdateDto(string Name, string? Address);
    public record AgencyDto(Guid Id, Guid CompanyId, string Name, string? Address);
    public record AgencyCreateDto(Guid CompanyId, string Name, string? Address);
    public record AgencyUpdateDto(string Name, string? Address);


}
