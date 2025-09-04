namespace MysteryShopper.API.Domain
{
    using System.ComponentModel.DataAnnotations;

    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Auditing
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class Company : BaseEntity
    {
        [MaxLength(200)] public string Name { get; set; } = default!; // e.g., KFC, McDonalds
        public string? Notes { get; set; }
        public ICollection<Agency> Agencies { get; set; } = new List<Agency>();
    }

    public class Agency : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = default!;

        [MaxLength(200)] public string Name { get; set; } = default!; // e.g., KFC Amazonas
        [MaxLength(300)] public string? Address { get; set; }
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

    public class Employee : BaseEntity
    {
        public Guid AgencyId { get; set; }
        public Agency Agency { get; set; } = default!;
        [MaxLength(140)] public string FullName { get; set; } = default!;
        public string? Position { get; set; }
    }

    public class SurveyTemplate : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = default!;
        [MaxLength(200)] public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }

    public enum QuestionType
    {
        Text = 0,
        SingleChoice = 1,
        MultipleChoice = 2,
        Number = 3,
        YesNo = 4,
        Rating1to5 = 5
    }

    public class Question : BaseEntity
    {
        public Guid SurveyTemplateId { get; set; }
        public SurveyTemplate SurveyTemplate { get; set; } = default!;

        [MaxLength(500)] public string Text { get; set; } = default!;
        public QuestionType Type { get; set; }
        public decimal Weight { get; set; } = 1; // peso de la pregunta

        // For options (Single/Multiple)
        public string? OptionsJson { get; set; } // e.g., ["Muy bueno","Bueno","Regular","Malo"]

        public bool AllowComment { get; set; } = true; // comentario por pregunta
        public bool AllowMedia { get; set; } = true;   // adjuntos por pregunta
    }

    public class SurveyAssignment : BaseEntity
    {
        // A whom & where we assign a survey
        public Guid SurveyTemplateId { get; set; }
        public SurveyTemplate SurveyTemplate { get; set; } = default!;

        public Guid AgencyId { get; set; }
        public Agency Agency { get; set; } = default!;

        public Guid? EmployeeId { get; set; } // optional, evaluar a un trabajador
        public Employee? Employee { get; set; }

        public string EvaluatorUserId { get; set; } = default!; // ApplicationUser.Id (role Evaluador)
        public DateTime DueDate { get; set; }
        public bool Completed { get; set; }
    }

    public class SurveyResponse : BaseEntity
    {
        public Guid AssignmentId { get; set; }
        public SurveyAssignment Assignment { get; set; } = default!;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }

        public string? OverallComment { get; set; }
        public decimal Score { get; set; } // calculated from answers * weights

        public ICollection<Answer> Answers { get; set; } = new List<Answer>();
        public ICollection<MediaFile> Media { get; set; } = new List<MediaFile>(); // global media for whole survey
    }

    public class Answer : BaseEntity
    {
        public Guid ResponseId { get; set; }
        public SurveyResponse Response { get; set; } = default!;

        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = default!;

        public string? TextValue { get; set; }
        public decimal? NumberValue { get; set; }
        public bool? BoolValue { get; set; }
        public string? SelectedOptionsJson { get; set; } // for single/multi choice

        public string? Comment { get; set; }
        public ICollection<MediaFile> Media { get; set; } = new List<MediaFile>();
    }

    public enum MediaKind { Image = 0, Video = 1, Audio = 2, Other = 3 }

    public class MediaFile : BaseEntity
    {
        public string FileName { get; set; } = default!;
        public string RelativePath { get; set; } = default!; // e.g., /uploads/2025/09/...
        public MediaKind Kind { get; set; }
        public long SizeBytes { get; set; }

        public Guid? ResponseId { get; set; } // si es global de la encuesta
        public SurveyResponse? Response { get; set; }

        public Guid? AnswerId { get; set; }   // si es adjunto de una pregunta
        public Answer? Answer { get; set; }
    }
}
