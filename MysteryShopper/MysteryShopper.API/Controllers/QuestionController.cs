using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using MysteryShopper.API.Contracts.DTOs;
using MysteryShopper.API.Domain;
using MysteryShopper.API.Infrastructure;

namespace MysteryShopper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;

        public QuestionController(IUnitOfWork uow, IMapper mapper, AppDbContext db)
        {
            _uow = uow;
            _mapper = mapper;
            _db = db;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _uow.Questions.Query().ToList();
            return Ok(_mapper.Map<IEnumerable<QuestionDto>>(list));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _uow.Questions.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<QuestionDto>(item));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Create(QuestionCreateDto dto)
        {
            var entity = _mapper.Map<Question>(dto);
            await _uow.Questions.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<QuestionDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Update(Guid id, QuestionUpdateDto dto)
        {
            var entity = await _uow.Questions.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _uow.Questions.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<QuestionDto>(entity));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hard = false)
        {
            var entity = await _uow.Questions.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (hard)
            {
                _db.Remove(entity);
            }
            else
            {
                await _uow.Questions.SoftDeleteAsync(id);
            }
            await _uow.SaveChangesAsync();
            return NoContent();
        }
    }
}
