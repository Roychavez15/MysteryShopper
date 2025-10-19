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
    public class SurveyResponseController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public SurveyResponseController(IUnitOfWork uow, IMapper mapper, AppDbContext db, UserManager<ApplicationUser> userManager)
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
                return Ok(_mapper.Map<IEnumerable<SurveyResponseDto>>(_uow.Responses.Query().ToList()));
            }
            else if (User.IsInRole(Roles.Client))
            {
                if (currentUser?.CompanyId == null) return Forbid();

                var list = _uow.Responses.Query()
                    .Where(r => r.Assignment.Agency.CompanyId == currentUser.CompanyId.Value)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<SurveyResponseDto>>(list));
            }
            else if (User.IsInRole(Roles.Evaluator))
            {
                var list = _uow.Responses.Query()
                    .Where(r => r.CreatedBy == currentUser!.Id)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<SurveyResponseDto>>(list));
            }

            return Forbid();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var response = await _uow.Responses.GetByIdAsync(id);

            if (response == null) return NotFound();

            if (User.IsInRole(Roles.Admin))
                return Ok(_mapper.Map<SurveyResponseDto>(response));

            if (User.IsInRole(Roles.Client) && response.Assignment.Agency.CompanyId == currentUser?.CompanyId)
                return Ok(_mapper.Map<SurveyResponseDto>(response));

            if (User.IsInRole(Roles.Evaluator) && response.CreatedBy == currentUser!.Id)
                return Ok(_mapper.Map<SurveyResponseDto>(response));

            return Forbid();
        }
    

    [HttpPost]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Create(SurveyResponseCreateDto dto)
        {
            var entity = _mapper.Map<SurveyResponse>(dto);
            await _uow.Responses.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<SurveyResponseDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Update(Guid id, SurveyResponseUpdateDto dto)
        {
            var entity = await _uow.Responses.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _uow.Responses.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<SurveyResponseDto>(entity));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hard = false)
        {
            var entity = await _uow.Responses.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (hard)
            {
                _db.Remove(entity);
            }
            else
            {
                await _uow.Responses.SoftDeleteAsync(id);
            }
            await _uow.SaveChangesAsync();
            return NoContent();
        }
    }

}
