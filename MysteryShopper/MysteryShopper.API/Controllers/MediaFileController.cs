using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MysteryShopper.API.Contracts.DTOs;
using MysteryShopper.API.Infrastructure;

namespace MysteryShopper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaFileController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly AppDbContext _db;

        public MediaFileController(IUnitOfWork uow, IMapper mapper, AppDbContext db)
        {
            _uow = uow;
            _mapper = mapper;
            _db = db;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var list = _uow.Media.Query().ToList();
            return Ok(_mapper.Map<IEnumerable<MediaFileDto>>(list));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool hard = false)
        {
            var entity = await _uow.Media.GetByIdAsync(id);
            if (entity == null) return NotFound();
            if (hard)
            {
                _db.Remove(entity);
            }
            else
            {
                await _uow.Media.SoftDeleteAsync(id);
            }
            await _uow.SaveChangesAsync();
            return NoContent();
        }
    }

}
