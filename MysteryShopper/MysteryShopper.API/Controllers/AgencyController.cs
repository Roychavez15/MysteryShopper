using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MysteryShopper.API.Contracts.DTOs;
using MysteryShopper.API.Domain;
using MysteryShopper.API.Domain.Identity;
using MysteryShopper.API.Infrastructure;

namespace MysteryShopper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgencyController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public AgencyController(IUnitOfWork uow, IMapper mapper, AppDbContext db, UserManager<ApplicationUser> userManager)
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
                var all = _uow.Agencies.Query().ToList();
                return Ok(_mapper.Map<IEnumerable<AgencyDto>>(all));
            }
            else if (User.IsInRole(Roles.Client))
            {
                if (currentUser?.CompanyId == null)
                    return Forbid();

                var list = _uow.Agencies.Query()
                    .Where(a => a.CompanyId == currentUser.CompanyId.Value)
                    .ToList();

                return Ok(_mapper.Map<IEnumerable<AgencyDto>>(list));
            }

            return Forbid();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var agency = await _uow.Agencies.GetByIdAsync(id);
            if (agency == null) return NotFound();

            if (User.IsInRole(Roles.Admin))
            {
                return Ok(_mapper.Map<AgencyDto>(agency));
            }
            else if (User.IsInRole(Roles.Client))
            {
                if (currentUser?.CompanyId != agency.CompanyId) return Forbid();
                return Ok(_mapper.Map<AgencyDto>(agency));
            }

            return Forbid();
        }


        [HttpPost]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Create(AgencyCreateDto dto)
        {
            var entity = _mapper.Map<Agency>(dto);
            await _uow.Agencies.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<AgencyDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Update(Guid id, AgencyUpdateDto dto)
        {
            var entity = await _uow.Agencies.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _uow.Agencies.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<AgencyDto>(entity));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hard = false)
        {
            var entity = await _uow.Agencies.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (hard)
            {
                _db.Remove(entity);
            }
            else
            {
                await _uow.Agencies.SoftDeleteAsync(id);
            }
            await _uow.SaveChangesAsync();
            return NoContent();
        }
    }

}
