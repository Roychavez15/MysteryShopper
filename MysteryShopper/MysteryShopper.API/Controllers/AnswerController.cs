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
    public class AnswerController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;

        public AnswerController(IUnitOfWork uow, IMapper mapper, AppDbContext db)
        {
            _uow = uow;
            _mapper = mapper;
            _db = db;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _uow.Answers.Query().ToList();
            return Ok(_mapper.Map<IEnumerable<AnswerDto>>(list));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _uow.Answers.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(_mapper.Map<AnswerDto>(item));
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Create(AnswerCreateDto dto)
        {
            var entity = _mapper.Map<Answer>(dto);
            await _uow.Answers.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<AnswerDto>(entity));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN,CLIENTE")]
        public async Task<IActionResult> Update(Guid id, AnswerUpdateDto dto)
        {
            var entity = await _uow.Answers.GetByIdAsync(id);
            if (entity == null) return NotFound();
            _mapper.Map(dto, entity);
            await _uow.Answers.UpdateAsync(entity);
            await _uow.SaveChangesAsync();
            return Ok(_mapper.Map<AnswerDto>(entity));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hard = false)
        {
            var entity = await _uow.Answers.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (hard)
            {
                _db.Remove(entity);
            }
            else
            {
                await _uow.Answers.SoftDeleteAsync(id);
            }
            await _uow.SaveChangesAsync();
            return NoContent();
        }
    }

}
