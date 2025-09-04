using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MysteryShopper.API.Contracts.DTOs;
using MysteryShopper.API.Domain;
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

        public CompanyController(IUnitOfWork uow, IMapper mapper, AppDbContext db)
        {
            _uow = uow;
            _mapper = mapper;
            _db = db;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _uow.Companies.Query().ToList();
            return Ok(_mapper.Map<IEnumerable<CompanyDto>>(list));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _uow.Companies.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<CompanyDto>(item));
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
