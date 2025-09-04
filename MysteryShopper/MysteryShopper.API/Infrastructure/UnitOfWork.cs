using MysteryShopper.API.Domain;

namespace MysteryShopper.API.Infrastructure
{
    public interface IUnitOfWork
    {
        IGenericRepository<Company> Companies { get; }
        IGenericRepository<Agency> Agencies { get; }
        IGenericRepository<Employee> Employees { get; }
        IGenericRepository<SurveyTemplate> SurveyTemplates { get; }
        IGenericRepository<Question> Questions { get; }
        IGenericRepository<SurveyAssignment> Assignments { get; }
        IGenericRepository<SurveyResponse> Responses { get; }
        IGenericRepository<Answer> Answers { get; }
        IGenericRepository<MediaFile> Media { get; }

        Task<int> SaveChangesAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        public UnitOfWork(AppDbContext db)
        { _db = db; }

        public IGenericRepository<Company> Companies => new GenericRepository<Company>(_db);
        public IGenericRepository<Agency> Agencies => new GenericRepository<Agency>(_db);
        public IGenericRepository<Employee> Employees => new GenericRepository<Employee>(_db);
        public IGenericRepository<SurveyTemplate> SurveyTemplates => new GenericRepository<SurveyTemplate>(_db);
        public IGenericRepository<Question> Questions => new GenericRepository<Question>(_db);
        public IGenericRepository<SurveyAssignment> Assignments => new GenericRepository<SurveyAssignment>(_db);
        public IGenericRepository<SurveyResponse> Responses => new GenericRepository<SurveyResponse>(_db);
        public IGenericRepository<Answer> Answers => new GenericRepository<Answer>(_db);
        public IGenericRepository<MediaFile> Media => new GenericRepository<MediaFile>(_db);

        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();
    }

}
