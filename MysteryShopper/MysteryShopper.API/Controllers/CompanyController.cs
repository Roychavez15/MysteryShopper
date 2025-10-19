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
    public class CompanyController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        public CompanyController(IUnitOfWork uow, IMapper mapper, AppDbContext db, UserManager<ApplicationUser> userManager)
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
                var all = _uow.Companies.Query().ToList();
                return Ok(_mapper.Map<IEnumerable<CompanyDto>>(all));
            }
            else if (User.IsInRole(Roles.Client))
            {
                if (currentUser?.CompanyId == null)
                    return Forbid();

                var company = await _uow.Companies.GetByIdAsync(currentUser.CompanyId.Value);
                if (company == null) return NotFound();

                return Ok(_mapper.Map<CompanyDto>(company));
            }

            return Forbid();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (User.IsInRole(Roles.Admin))
            {
                var item = await _uow.Companies.GetByIdAsync(id);
                if (item == null) return NotFound();
                return Ok(_mapper.Map<CompanyDto>(item));
            }
            else if (User.IsInRole(Roles.Client))
            {
                if (currentUser?.CompanyId != id) return Forbid();

                var item = await _uow.Companies.GetByIdAsync(id);
                if (item == null) return NotFound();

                return Ok(_mapper.Map<CompanyDto>(item));
            }

            return Forbid();
        }
    

        [HttpPost]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Create(CompanyCreateDto dto)
        {
            var entity = _mapper.Map<Company>(dto);
            await _uow.Companies.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<CompanyDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Update(Guid id, CompanyUpdateDto dto)
        {
            var entity = await _uow.Companies.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _uow.Companies.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<CompanyDto>(entity));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hard = false)
        {
            var entity = await _uow.Companies.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (hard)
            {
                _db.Remove(entity);
            }
            else
            {
                await _uow.Companies.SoftDeleteAsync(id);
            }
            await _uow.SaveChangesAsync();
            return NoContent();
        }
    }

}
