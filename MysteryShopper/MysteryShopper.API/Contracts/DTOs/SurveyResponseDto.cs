namespace MysteryShopper.API.Contracts.DTOs
{
    public record SurveyResponseDto(Guid Id, Guid AssignmentId, DateTime StartedAt, DateTime? SubmittedAt, string? OverallComment, decimal Score);
    public record SurveyResponseCreateDto(Guid AssignmentId);
    public record SurveyResponseUpdateDto(string? OverallComment, DateTime? SubmittedAt);

}
