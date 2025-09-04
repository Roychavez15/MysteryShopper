namespace MysteryShopper.API.Contracts.DTOs
{
    //public record AnswerDto(Guid Id, Guid ResponseId, Guid QuestionId, string? Value, string? Comment);
    //public record AnswerCreateDto(Guid ResponseId, Guid QuestionId, string? Value, string? Comment);
    public record AnswerDto(Guid Id, Guid ResponseId, Guid QuestionId, string? TextValue, decimal? NumberValue, bool? BoolValue, string? SelectedOptionsJson, string? Comment);
    public record AnswerCreateDto(Guid ResponseId, Guid QuestionId, string? TextValue, decimal? NumberValue, bool? BoolValue, string? SelectedOptionsJson, string? Comment);
    public record AnswerUpdateDto(string? TextValue, decimal? NumberValue, bool? BoolValue, string? SelectedOptionsJson, string? Comment);

}
