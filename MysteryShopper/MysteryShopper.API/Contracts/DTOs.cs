using MysteryShopper.API.Domain;

namespace MysteryShopper.API.Contracts
{
    public record RegisterRequest(string Email, string Password, string Role, Guid? CompanyId);
    public record LoginRequest(string Email, string Password);
    public record AuthResponse(string Token);

    public record CompanyCreateDto(string Name, string? Notes);
    public record AgencyCreateDto(Guid CompanyId, string Name, string? Address);
    public record EmployeeCreateDto(Guid AgencyId, string FullName, string? Position);

    public record SurveyTemplateCreateDto(Guid CompanyId, string Title, string? Description);
    public record QuestionCreateDto(Guid SurveyTemplateId, string Text, QuestionType Type, decimal Weight, string? OptionsJson, bool AllowComment, bool AllowMedia);

    public record AssignmentCreateDto(Guid SurveyTemplateId, Guid AgencyId, Guid? EmployeeId, string EvaluatorUserId, DateTime DueDate);

    public record StartResponseDto(Guid AssignmentId);
    public record AnswerUpsertDto(Guid QuestionId, string? TextValue, decimal? NumberValue, bool? BoolValue, string? SelectedOptionsJson, string? Comment);
    public record SubmitResponseDto(Guid ResponseId, string? OverallComment);
}
