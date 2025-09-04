using AutoMapper;
using MysteryShopper.API.Contracts.DTOs;
using MysteryShopper.API.Domain;

namespace MysteryShopper.API.Infrastructure.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Company, CompanyDto>();
            CreateMap<CompanyCreateDto, Company>();
            CreateMap<CompanyUpdateDto, Company>();

            CreateMap<Agency, AgencyDto>();
            CreateMap<AgencyCreateDto, Agency>();
            CreateMap<AgencyUpdateDto, Agency>();

            CreateMap<Employee, EmployeeDto>();
            CreateMap<EmployeeCreateDto, Employee>();
            CreateMap<EmployeeUpdateDto, Employee>();

            CreateMap<SurveyTemplate, SurveyTemplateDto>();
            CreateMap<SurveyTemplateCreateDto, SurveyTemplate>();
            CreateMap<SurveyTemplateUpdateDto, SurveyTemplate>();

            CreateMap<Question, QuestionDto>();
            CreateMap<QuestionCreateDto, Question>();
            CreateMap<QuestionUpdateDto, Question>();

            CreateMap<SurveyAssignment, SurveyAssignmentDto>();
            CreateMap<SurveyAssignmentCreateDto, SurveyAssignment>();

            CreateMap<SurveyResponse, SurveyResponseDto>();
            CreateMap<SurveyResponseCreateDto, SurveyResponse>();

            CreateMap<Answer, AnswerDto>();
            CreateMap<AnswerCreateDto, Answer>();

            CreateMap<MediaFile, MediaFileDto>();
        }
    }
}
