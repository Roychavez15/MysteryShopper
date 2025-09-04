using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MysteryShopper.API.Contracts;
using MysteryShopper.API.Domain.Identity;
using MysteryShopper.API.Domain;
using MysteryShopper.API.Infrastructure.Files;
using MysteryShopper.API.Infrastructure;
using System.Security.Claims;

namespace MysteryShopper.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Roles.Evaluator)]
    public class ResponsesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IUnitOfWork _uow;
        private readonly IFileStorage _files;

        public ResponsesController(AppDbContext db, IUnitOfWork uow, IFileStorage files)
        { _db = db; _uow = uow; _files = files; }

        [HttpPost("start")] // starts a response from an assignment
        public async Task<ActionResult<SurveyResponse>> Start(StartResponseDto dto)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var asg = await _db.SurveyAssignments.FirstOrDefaultAsync(a => a.Id == dto.AssignmentId);
            if (asg is null) return NotFound("assignment");
            if (asg.EvaluatorUserId != uid) return Forbid();
            if (await _db.SurveyResponses.AnyAsync(r => r.AssignmentId == asg.Id))
                return BadRequest("Response already exists");

            var r = new SurveyResponse { AssignmentId = dto.AssignmentId };
            await _uow.Responses.AddAsync(r);
            await _uow.SaveChangesAsync();
            return Ok(r);
        }

        [HttpPost("{responseId}/answer")] // upsert answer per question
        public async Task<ActionResult> UpsertAnswer(Guid responseId, AnswerUpsertDto dto)
        {
            var resp = await _db.SurveyResponses.Include(r => r.Answers).FirstOrDefaultAsync(r => r.Id == responseId);
            if (resp is null) return NotFound();

            var ans = resp.Answers.FirstOrDefault(a => a.QuestionId == dto.QuestionId);
            if (ans is null)
            {
                ans = new Answer { ResponseId = responseId, QuestionId = dto.QuestionId };
                resp.Answers.Add(ans);
            }
            ans.TextValue = dto.TextValue;
            ans.NumberValue = dto.NumberValue;
            ans.BoolValue = dto.BoolValue;
            ans.SelectedOptionsJson = dto.SelectedOptionsJson;
            ans.Comment = dto.Comment;

            await _uow.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("{responseId}/answer/{questionId}/upload")] // per-question media
        [RequestSizeLimit(200_000_000)]
        public async Task<ActionResult<MediaFile>> UploadForAnswer(Guid responseId, Guid questionId, IFormFile file, MediaKind kind)
        {
            var ans = await _db.Answers.FirstOrDefaultAsync(a => a.ResponseId == responseId && a.QuestionId == questionId);
            if (ans is null) return NotFound("answer");
            var media = await _files.SaveAsync(file, kind, responseId, ans.Id);
            return Ok(media);
        }

        [HttpPost("{responseId}/upload")] // global media for whole survey
        [RequestSizeLimit(200_000_000)]
        public async Task<ActionResult<MediaFile>> UploadForResponse(Guid responseId, IFormFile file, MediaKind kind)
        {
            var resp = await _db.SurveyResponses.FindAsync(responseId);
            if (resp is null) return NotFound("response");
            var media = await _files.SaveAsync(file, kind, responseId, null);
            return Ok(media);
        }

        [HttpPost("{responseId}/submit")] // compute score, finalize
        public async Task<ActionResult<SurveyResponse>> Submit(Guid responseId, SubmitResponseDto dto)
        {
            var resp = await _db.SurveyResponses
                .Include(r => r.Answers)
                .Include(r => r.Assignment)
                .ThenInclude(a => a.SurveyTemplate)
                .ThenInclude(t => t.Questions)
                .FirstOrDefaultAsync(r => r.Id == responseId);
            if (resp is null) return NotFound();

            decimal score = 0, totalWeight = 0;
            foreach (var q in resp.Assignment.SurveyTemplate.Questions)
            {
                totalWeight += q.Weight;
                var ans = resp.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                if (ans is null) continue;

                decimal partial = q.Type switch
                {
                    QuestionType.YesNo => (ans.BoolValue == true ? 1m : 0m),
                    QuestionType.Rating1to5 => (ans.NumberValue is null ? 0m : Math.Clamp((decimal)ans.NumberValue, 1, 5) / 5m),
                    QuestionType.Number => (ans.NumberValue ?? 0m),
                    _ => 0m
                };
                score += partial * q.Weight;
            }
            resp.Score = totalWeight == 0 ? 0 : Math.Round(score / totalWeight * 100, 2); // 0..100
            resp.OverallComment = dto.OverallComment;
            resp.SubmittedAt = DateTime.UtcNow;
            resp.Assignment.Completed = true;

            await _uow.SaveChangesAsync();
            return Ok(resp);
        }
    }

}
