using Aviask.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aviask.Data
{
    public class AviaskContext : IdentityDbContext<IdentityUser>
    {
        public AviaskContext(DbContextOptions<AviaskContext> options)
            : base(options)
        {
        }

        public DbSet<Question> Question { get; set; } = default!;
        public DbSet<QuestionAnswers> QuestionAnswers { get; set; } = default!;
        public DbSet<AnswerRecords> AnswerRecords { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Question>().ToTable("Question");
            modelBuilder.Entity<QuestionAnswers>().ToTable("QuestionAnswers");
            modelBuilder.Entity<AnswerRecords>().ToTable("AnswerRecords")
                .Property(e => e.Date)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
