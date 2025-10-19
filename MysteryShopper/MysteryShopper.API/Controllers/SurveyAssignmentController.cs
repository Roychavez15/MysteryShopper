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
    public class SurveyAssignmentController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public SurveyAssignmentController(IUnitOfWork uow, IMapper mapper, AppDbContext db, UserManager<ApplicationUser> userManager)
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
                return Ok(_mapper.Map<IEnumerable<SurveyAssignmentDto>>(_uow.Assignments.Query().ToList()));
            }
            else if (User.IsInRole(Roles.Client))
            {
                if (currentUser?.CompanyId == null) return Forbid();

                var list = _uow.Assignments.Query()
                    .Where(sa => sa.Agency.CompanyId == currentUser.CompanyId.Value)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<SurveyAssignmentDto>>(list));
            }

            return Forbid();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var assignment = await _uow.Assignments.GetByIdAsync(id);

            if (assignment == null) return NotFound();

            if (User.IsInRole(Roles.Admin))
                return Ok(_mapper.Map<SurveyAssignmentDto>(assignment));

            if (User.IsInRole(Roles.Client) && assignment.Agency.CompanyId == currentUser?.CompanyId)
                return Ok(_mapper.Map<SurveyAssignmentDto>(assignment));

            return Forbid();
        }
    

    [HttpPost]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Create(SurveyAssignmentCreateDto dto)
        {
            var entity = _mapper.Map<SurveyAssignment>(dto);
            await _uow.Assignments.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<SurveyAssignmentDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Update(Guid id, SurveyAssignmentUpdateDto dto)
        {
            var entity = await _uow.Assignments.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _uow.Assignments.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<SurveyAssignmentDto>(entity));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hard = false)
        {
            var entity = await _uow.Assignments.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (hard)
            {
                _db.Remove(entity);
            }
            else
            {
                await _uow.Assignments.SoftDeleteAsync(id);
            }
            await _uow.SaveChangesAsync();
            return NoContent();
        }
    }

}
