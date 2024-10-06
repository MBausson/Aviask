using AviaskApi.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AviaskApi.Models;

public class AviaskApiContext : IdentityDbContext<AviaskUser, AviaskUserRole, Guid>
{
    public AviaskApiContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Attachment> Attachments { get; set; } = default!;
    public virtual DbSet<Question> Questions { get; set; } = default!;
    public DbSet<QuestionAnswers> QuestionAnswers { get; set; } = default!;
    public DbSet<AviaskUser> AviaskUsers { get; set; } = default!;
    public DbSet<AnswerRecord> AnswerRecords { get; set; } = default!;
    public DbSet<QuestionReport> QuestionReports { get; set; } = default!;
    public DbSet<RecoveryToken> RecoveryTokens { get; set; } = default!;
    public DbSet<MockExam> MockExams { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //  Attachment
        modelBuilder.Entity<Attachment>().ToTable(nameof(Attachments));

        //  Question
        modelBuilder.Entity<Question>()
            .ToTable(nameof(Questions))
            .HasOne<Attachment>()
            .WithMany()
            .HasForeignKey(q => q.IllustrationId);

        //  Prevents from updating the publisher
        modelBuilder.Entity<Question>().Property(e => e.PublisherId).ValueGeneratedOnUpdate();

        //  QuestionAnswer
        modelBuilder.Entity<QuestionAnswers>().ToTable(nameof(QuestionAnswers));

        //  AviaskUser
        modelBuilder.Entity<AviaskUser>().ToTable("AspNetUsers")
            .HasKey(u => u.Id);

        modelBuilder.Entity<AviaskUser>()
            .HasMany(u => u.Publications)
            .WithOne(u => u.Publisher)
            .HasForeignKey(u => u.PublisherId)
            .IsRequired(false);

        //  QuestionReport
        modelBuilder.Entity<QuestionReport>().ToTable(nameof(QuestionReports));

        //  RecoveryToken
        modelBuilder.Entity<RecoveryToken>().ToTable(nameof(RecoveryTokens));

        //  MockExam
        modelBuilder.Entity<MockExam>().ToTable(nameof(MockExams))
            .HasMany<AnswerRecord>(e => e.AnswerRecords)
            .WithOne(e => e.MockExam)
            .HasForeignKey(e => e.MockExamId)
            .IsRequired(false);
    }
}