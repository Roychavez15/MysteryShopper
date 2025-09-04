namespace MysteryShopper.API.Contracts.DTOs
{

    public record SurveyTemplateDto(Guid Id, Guid CompanyId, string Title, string? Description);
    public record SurveyTemplateCreateDto(Guid CompanyId, string Title, string? Description);
    public record SurveyTemplateUpdateDto(string Title, string? Description);

}
