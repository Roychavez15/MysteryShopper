using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MysteryShopper.API.Contracts;
using MysteryShopper.API.Domain.Identity;
using MysteryShopper.API.Domain;
using MysteryShopper.API.Infrastructure;
using MysteryShopper.API.Contracts.DTOs;

namespace MysteryShopper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Client)]
    public class SurveysController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public SurveysController(IUnitOfWork uow) { _uow = uow; }

        [HttpPost("templates")]
        public async Task<ActionResult<SurveyTemplate>> CreateTemplate(SurveyTemplateCreateDto dto)
        {
            var s = new SurveyTemplate { CompanyId = dto.CompanyId, Title = dto.Title, Description = dto.Description };
            await _uow.SurveyTemplates.AddAsync(s);
            await _uow.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTemplate), new { id = s.Id }, s);
        }

        [HttpGet("templates/{id}")]
        public async Task<ActionResult<SurveyTemplate?>> GetTemplate(Guid id)
        {
            var t = await _uow.SurveyTemplates.GetByIdAsync(id, x => x.Questions);
            return t is null ? NotFound() : Ok(t);
        }

        [HttpPost("questions")]
        public async Task<ActionResult<Question>> AddQuestion(QuestionCreateDto dto)
        {
            var q = new Question
            {
                SurveyTemplateId = dto.SurveyTemplateId,
                Text = dto.Text,
                Type = dto.Type,
                Weight = dto.Weight,
                OptionsJson = dto.OptionsJson,
                AllowComment = dto.AllowComment,
                AllowMedia = dto.AllowMedia
            };
            await _uow.Questions.AddAsync(q);
            await _uow.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTemplate), new { id = dto.SurveyTemplateId }, q);
        }
    }

}
