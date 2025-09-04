using MysteryShopper.API.Domain;

namespace MysteryShopper.API.Infrastructure.Files
{
    public interface IFileStorage
    {
        Task<MediaFile> SaveAsync(IFormFile file, MediaKind kind, Guid? responseId, Guid? answerId);
    }

    public class LocalFileStorage : IFileStorage
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _cfg;
        private readonly AppDbContext _db;

        public LocalFileStorage(IWebHostEnvironment env, IConfiguration cfg, AppDbContext db)
        { _env = env; _cfg = cfg; _db = db; }

        public async Task<MediaFile> SaveAsync(IFormFile file, MediaKind kind, Guid? responseId, Guid? answerId)
        {
            var root = _cfg.GetValue<string>("FileStorage:Root") ?? "wwwroot";
            var uploadsFolder = _cfg.GetValue<string>("FileStorage:UploadsFolder") ?? "uploads";
            var datePath = DateTime.UtcNow.ToString("yyyy/MM");
            var relativeDir = $"/{uploadsFolder}/{datePath}";
            var absDir = Path.Combine(_env.ContentRootPath, root, uploadsFolder, DateTime.UtcNow.ToString("yyyy"), DateTime.UtcNow.ToString("MM"));
            Directory.CreateDirectory(absDir);

            var safeName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
            var absPath = Path.Combine(absDir, safeName);
            using (var stream = new FileStream(absPath, FileMode.Create))
            { await file.CopyToAsync(stream); }

            var media = new MediaFile
            {
                FileName = file.FileName,
                RelativePath = $"{relativeDir}/{safeName}",
                Kind = kind,
                SizeBytes = file.Length,
                ResponseId = responseId,
                AnswerId = answerId
            };
            _db.MediaFiles.Add(media);
            await _db.SaveChangesAsync();
            return media;
        }
    }
}
