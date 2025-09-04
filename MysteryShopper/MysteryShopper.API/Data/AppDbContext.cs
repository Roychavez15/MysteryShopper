using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MysteryShopper.API.Domain;
using MysteryShopper.API.Domain.Identity;
using MysteryShopper.API.Services;
using System.Linq.Expressions;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentUserService? _currentUser;
    //public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    //{
    //}
    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUser = null) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<SurveyTemplate> SurveyTemplates => Set<SurveyTemplate>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<SurveyAssignment> SurveyAssignments => Set<SurveyAssignment>();
    public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<MediaFile> MediaFiles => Set<MediaFile>();

    //protected override void OnModelCreating(ModelBuilder b)
    //{
    //    base.OnModelCreating(b);

    //    // Apply soft-delete global filter for all entities deriving from BaseEntity
    //    foreach (var entityType in b.Model.GetEntityTypes()
    //        .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
    //    {
    //        var method = typeof(AppDbContext).GetMethod(nameof(GetIsDeletedFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
    //            .MakeGenericMethod(entityType.ClrType);
    //        var filter = method.Invoke(null, new object[] { });
    //        entityType.SetQueryFilter((LambdaExpression)filter!);
    //    }

    //    b.Entity<Company>().HasIndex(x => x.Name).IsUnique();
    //    b.Entity<Agency>().HasOne(a => a.Company).WithMany(c => c.Agencies).HasForeignKey(a => a.CompanyId);
    //    b.Entity<Employee>().HasOne(e => e.Agency).WithMany(a => a.Employees).HasForeignKey(e => e.AgencyId);

    //    b.Entity<Question>()
    //        .HasOne(q => q.SurveyTemplate)
    //        .WithMany(s => s.Questions)
    //        .HasForeignKey(q => q.SurveyTemplateId);

    //    b.Entity<SurveyAssignment>()
    //        .HasOne(a => a.SurveyTemplate)
    //        .WithMany()
    //        .HasForeignKey(a => a.SurveyTemplateId);

    //    b.Entity<SurveyAssignment>()
    //        .HasOne(a => a.Agency)
    //        .WithMany()
    //        .HasForeignKey(a => a.AgencyId);

    //    b.Entity<SurveyResponse>()
    //        .HasOne(r => r.Assignment)
    //        .WithMany()
    //        .HasForeignKey(r => r.AssignmentId);

    //    b.Entity<Answer>()
    //        .HasOne(a => a.Response)
    //        .WithMany(r => r.Answers)
    //        .HasForeignKey(a => a.ResponseId);

    //    b.Entity<MediaFile>()
    //        .HasOne(m => m.Answer)
    //        .WithMany(a => a.Media)
    //        .HasForeignKey(m => m.AnswerId)
    //        .OnDelete(DeleteBehavior.Cascade);

    //    b.Entity<MediaFile>()
    //        .HasOne(m => m.Response)
    //        .WithMany(r => r.Media)
    //        .HasForeignKey(m => m.ResponseId)
    //        .OnDelete(DeleteBehavior.Cascade);
    //}
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Soft delete global filter
        foreach (var entityType in b.Model.GetEntityTypes()
            .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            var method = typeof(AppDbContext).GetMethod(nameof(GetIsDeletedFilter),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            var filter = method.Invoke(null, new object[] { });
            entityType.SetQueryFilter((LambdaExpression)filter!);
        }

        // Company
        b.Entity<Company>()
            .HasIndex(x => x.Name)
            .IsUnique();

        // Agency -> Company
        b.Entity<Agency>()
            .HasOne(a => a.Company)
            .WithMany(c => c.Agencies)
            .HasForeignKey(a => a.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Employee -> Agency
        b.Entity<Employee>()
            .HasOne(e => e.Agency)
            .WithMany(a => a.Employees)
            .HasForeignKey(e => e.AgencyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Question -> SurveyTemplate
        b.Entity<Question>()
            .HasOne(q => q.SurveyTemplate)
            .WithMany(s => s.Questions)
            .HasForeignKey(q => q.SurveyTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        // SurveyAssignment -> SurveyTemplate
        b.Entity<SurveyAssignment>()
            .HasOne(a => a.SurveyTemplate)
            .WithMany()
            .HasForeignKey(a => a.SurveyTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        // SurveyAssignment -> Agency
        b.Entity<SurveyAssignment>()
            .HasOne(a => a.Agency)
            .WithMany()
            .HasForeignKey(a => a.AgencyId)
            .OnDelete(DeleteBehavior.Restrict);

        // SurveyResponse -> SurveyAssignment
        b.Entity<SurveyResponse>()
            .HasOne(r => r.Assignment)
            .WithMany()
            .HasForeignKey(r => r.AssignmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Answer -> SurveyResponse
        b.Entity<Answer>()
            .HasOne(a => a.Response)
            .WithMany(r => r.Answers)
            .HasForeignKey(a => a.ResponseId)
            .OnDelete(DeleteBehavior.Restrict);

        // MediaFile -> Answer
        b.Entity<MediaFile>()
            .HasOne(m => m.Answer)
            .WithMany(a => a.Media)
            .HasForeignKey(m => m.AnswerId)
            .OnDelete(DeleteBehavior.Cascade);

        // MediaFile -> Response
        b.Entity<MediaFile>()
            .HasOne(m => m.Response)
            .WithMany(r => r.Media)
            .HasForeignKey(m => m.ResponseId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static LambdaExpression GetIsDeletedFilter<T>() where T : BaseEntity
    {
        Expression<Func<T, bool>> filter = e => !e.IsDeleted;
        return filter;
    }

    public override int SaveChanges()
    {
        ApplyAuditing();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditing();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditing()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var now = DateTime.UtcNow;
        var userId = _currentUser?.UserId;

        foreach (var e in entries)
        {
            if (e.State == EntityState.Added)
            {
                e.Entity.CreatedAt = now;
                e.Entity.CreatedBy = userId;
            }
            if (e.State == EntityState.Modified)
            {
                e.Entity.UpdatedAt = now;
                e.Entity.UpdatedBy = userId;
            }
            if (e.State == EntityState.Deleted)
            {
                // convert to soft-delete
                e.State = EntityState.Modified;
                e.Entity.IsDeleted = true;
                e.Entity.DeletedAt = now;
                e.Entity.DeletedBy = userId;
            }
        }
    }
}