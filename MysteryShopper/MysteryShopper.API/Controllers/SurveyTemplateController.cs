using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using MysteryShopper.API.Contracts.DTOs;
using MysteryShopper.API.Domain;
using MysteryShopper.API.Infrastructure;
using Microsoft.AspNetCore.Identity;
using MysteryShopper.API.Domain.Identity;

namespace MysteryShopper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyTemplateController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public SurveyTemplateController(IUnitOfWork uow, IMapper mapper, AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _uow = uow;
            _mapper = mapper;
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (User.IsInRole(Roles.Admin))
            {
                var all = _uow.SurveyTemplates.Query().ToList();
                return Ok(_mapper.Map<IEnumerable<SurveyTemplateDto>>(all));
            }
            else if (User.IsInRole(Roles.Client))
            {
                if (currentUser?.CompanyId == null) return Forbid();

                var list = _uow.SurveyTemplates.Query()
                    .Where(s => s.CompanyId == currentUser.CompanyId.Value)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<SurveyTemplateDto>>(list));
            }

            return Forbid();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var survey = await _uow.SurveyTemplates.GetByIdAsync(id);
            if (survey == null) return NotFound();

            if (User.IsInRole(Roles.Admin))
                return Ok(_mapper.Map<SurveyTemplateDto>(survey));

            if (User.IsInRole(Roles.Client) && survey.CompanyId == currentUser?.CompanyId)
                return Ok(_mapper.Map<SurveyTemplateDto>(survey));

            return Forbid();
        }


        [HttpPost]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Create(SurveyTemplateCreateDto dto)
        {
            var entity = _mapper.Map<SurveyTemplate>(dto);
            await _uow.SurveyTemplates.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<SurveyTemplateDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Update(Guid id, SurveyTemplateUpdateDto dto)
        {
            var entity = await _uow.SurveyTemplates.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _uow.SurveyTemplates.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<SurveyTemplateDto>(entity));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hard = false)
        {
            var entity = await _uow.SurveyTemplates.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (hard)
            {
                _db.Remove(entity);
            }
            else
            {
                await _uow.SurveyTemplates.SoftDeleteAsync(id);
            }
            await _uow.SaveChangesAsync();
            return NoContent();
        }
    }

}
