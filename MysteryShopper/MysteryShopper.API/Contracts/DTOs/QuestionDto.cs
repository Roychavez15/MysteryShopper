using MysteryShopper.API.Domain;

namespace MysteryShopper.API.Contracts.DTOs
{
    //public record QuestionDto(Guid Id, Guid SurveyTemplateId, string Text, string Type, int Weight);
    //public record QuestionCreateDto(Guid SurveyTemplateId, string Text, string Type, int Weight);
    //public record QuestionUpdateDto(string Text, string Type, int Weight);
    public record QuestionDto(Guid Id, Guid SurveyTemplateId, string Text, QuestionType Type, decimal Weight, string? OptionsJson, bool AllowComment, bool AllowMedia);
    public record QuestionCreateDto(Guid SurveyTemplateId, string Text, QuestionType Type, decimal Weight, string? OptionsJson, bool AllowComment, bool AllowMedia);
    public record QuestionUpdateDto(string Text, QuestionType Type, decimal Weight, string? OptionsJson, bool AllowComment, bool AllowMedia);

}
