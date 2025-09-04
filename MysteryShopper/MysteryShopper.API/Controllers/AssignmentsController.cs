using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MysteryShopper.API.Contracts;
using MysteryShopper.API.Domain.Identity;
using MysteryShopper.API.Domain;
using MysteryShopper.API.Infrastructure;

namespace MysteryShopper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Client)]
    public class AssignmentsController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        public AssignmentsController(IUnitOfWork uow) { _uow = uow; }

        [HttpPost]
        public async Task<ActionResult<SurveyAssignment>> Create(AssignmentCreateDto dto)
        {
            var a = new SurveyAssignment
            {
                SurveyTemplateId = dto.SurveyTemplateId,
                AgencyId = dto.AgencyId,
                EmployeeId = dto.EmployeeId,
                EvaluatorUserId = dto.EvaluatorUserId,
                DueDate = dto.DueDate
            };
            await _uow.Assignments.AddAsync(a);
            await _uow.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = a.Id }, a);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<SurveyAssignment?>> Get(Guid id)
        {
            var a = await _uow.Assignments.GetByIdAsync(id);
            return a is null ? NotFound() : Ok(a);
        }
    }
}
