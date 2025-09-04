namespace MysteryShopper.API.Contracts.DTOs
{
    //public record SurveyAssignmentDto(Guid Id, Guid SurveyTemplateId, Guid AgencyId, Guid EvaluatorId, DateTime AssignedAt);
    //public record SurveyAssignmentCreateDto(Guid SurveyTemplateId, Guid AgencyId, Guid EvaluatorId);
    public record SurveyAssignmentDto(Guid Id, Guid SurveyTemplateId, Guid AgencyId, Guid? EmployeeId, string EvaluatorUserId, DateTime DueDate, bool Completed);
    public record SurveyAssignmentCreateDto(Guid SurveyTemplateId, Guid AgencyId, Guid? EmployeeId, string EvaluatorUserId, DateTime DueDate);
    public record SurveyAssignmentUpdateDto(Guid AgencyId, Guid? EmployeeId, string EvaluatorUserId, DateTime DueDate, bool Completed);

}
