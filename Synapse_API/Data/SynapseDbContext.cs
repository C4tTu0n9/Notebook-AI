using Microsoft.EntityFrameworkCore;
using Synapse_API.Data.SeedingData;
using Synapse_API.Models.Entities;
using Synapse_API.Models.Enums;

namespace Synapse_API.Data
{
    public class SynapseDbContext  : DbContext
    {
        public SynapseDbContext(DbContextOptions<SynapseDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<LearningPath> LearningPaths { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<PathTopic> PathTopics { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventReminder> EventReminders { get; set; }
        public DbSet<TimeEstimation> TimeEstimations { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<UserQuizAttempt> UserQuizAttempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<LearningActivity> LearningActivities { get; set; }
        public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<RevenueReport> RevenueReports { get; set; }
        public DbSet<UserStatistic> UserStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasDefaultValue(UserRole.Student)
                .IsRequired();

            // UserProfiles
            modelBuilder.Entity<UserProfile>()
                .HasOne(up => up.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(up => up.UserID);

            // PasswordResets
            modelBuilder.Entity<Token>()
                .HasOne(pr => pr.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(pr => pr.UserID);

            // LearningPaths
            modelBuilder.Entity<LearningPath>()
                .HasOne(lp => lp.User)
                .WithMany(u => u.LearningPaths)
                .HasForeignKey(lp => lp.UserID);

            // Courses
            modelBuilder.Entity<Course>()
                .Property(c => c.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.User)
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Topics
            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Course)
                .WithMany(c => c.Topics)
                .HasForeignKey(t => t.CourseID);

            // PathTopics
            modelBuilder.Entity<PathTopic>()
                .HasOne(pt => pt.LearningPath)
                .WithMany(lp => lp.PathTopics)
                .HasForeignKey(pt => pt.PathID);

            modelBuilder.Entity<PathTopic>()
                .HasOne(pt => pt.Topic)
                .WithMany(t => t.PathTopics)
                .HasForeignKey(pt => pt.TopicID);

            modelBuilder.Entity<PathTopic>()
                .Property(pt => pt.Status)
                .HasDefaultValue(PathTopicStatus.NotStarted);

            // Events
            modelBuilder.Entity<Event>()
                .HasOne(e => e.User)
                .WithMany(u => u.Events)
                .HasForeignKey(e => e.UserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CourseID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.ParentEvent)
                .WithMany(e => e.ChildEvents)
                .HasForeignKey(e => e.ParentEventID)
                .OnDelete(DeleteBehavior.Restrict);

            // EventReminders
            modelBuilder.Entity<EventReminder>()
                .HasOne(er => er.Event)
                .WithMany(e => e.EventReminders)
                .HasForeignKey(er => er.EventID);

            // TimeEstimations
            modelBuilder.Entity<TimeEstimation>()
                .HasOne(te => te.User)
                .WithMany(u => u.TimeEstimations)
                .HasForeignKey(te => te.UserID);

            // Quizzes
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Topic)
                .WithMany(t => t.Quizzes)
                .HasForeignKey(q => q.TopicID);

            // Questions
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizID);

            // Options
            modelBuilder.Entity<Option>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionID)
                .OnDelete(DeleteBehavior.Cascade);

            // UserQuizAttempts
            modelBuilder.Entity<UserQuizAttempt>()
                .HasOne(uqa => uqa.User)
                .WithMany(u => u.UserQuizAttempts)
                .HasForeignKey(uqa => uqa.UserID)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<UserQuizAttempt>()
                .HasOne(uqa => uqa.Quiz)
                .WithMany(q => q.UserQuizAttempts)
                .HasForeignKey(uqa => uqa.QuizID)
                .OnDelete(DeleteBehavior.Restrict);


            // UserAnswers
            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.UserQuizAttempt)
                .WithMany(uqa => uqa.UserAnswers)
                .HasForeignKey(ua => ua.AttemptID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany(q => q.UserAnswers)
                .HasForeignKey(ua => ua.QuestionID)
                .OnDelete(DeleteBehavior.Restrict);

            // LearningActivities
            modelBuilder.Entity<LearningActivity>()
                .HasOne(la => la.User)
                .WithMany(u => u.LearningActivities)
                .HasForeignKey(la => la.UserID);

            // PerformanceMetrics
            modelBuilder.Entity<PerformanceMetric>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.PerformanceMetrics)
                .HasForeignKey(pm => pm.UserID);

            modelBuilder.Entity<PerformanceMetric>()
                .HasOne(pm => pm.Topic)
                .WithMany(t => t.PerformanceMetrics)
                .HasForeignKey(pm => pm.TopicID);

            // Goals
            modelBuilder.Entity<Goal>()
                .HasOne(g => g.User)
                .WithMany(u => u.Goals)
                .HasForeignKey(g => g.UserID);

            // Subscriptions
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserID);

            modelBuilder.Entity<Subscription>()
                .Property(s => s.PlanType)
                .HasDefaultValue(PlanType.Free);

            // Payments
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserID);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasDefaultValue(PaymentStatus.Pending);

            // RevenueReports và UserStatistics không cần cấu hình quan hệ

            modelBuilder.Entity<User>().HasData(SeedingUser.GetSeedUsers());

        }

    }
}
