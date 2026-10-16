
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Organiser> Organisers => Set<Organiser>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<RaceEvent> Events => Set<RaceEvent>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //roles
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(r => r.RoleID)
                .HasName("PK_Roles");

            entity.Property(r => r.RoleName)
                .HasMaxLength(30)
                .IsRequired();

            entity.HasIndex(r => r.RoleName)
                .IsUnique()
                .HasDatabaseName("UQ_Roles_RoleName");
        });

        //users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(u => u.UserID)
                .HasName("PK_Users");

            entity.Property(u => u.FirstName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.LastName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UQ_Users_Email");

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Users_Roles");
        });

        //organisers
        modelBuilder.Entity<Organiser>(entity =>
        {
            entity.ToTable("Organisers");

            entity.HasKey(o => o.OrganiserID)
                .HasName("PK_Organisers");

            entity.Property(o => o.OrganiserID)
                .ValueGeneratedNever();

            entity.HasOne(o => o.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(o => o.OrganiserID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Organisers_Users");
        });

        //participants
        modelBuilder.Entity<Participant>(entity =>
        {
            entity.ToTable("Participants");

            entity.HasKey(p => p.ParticipantID)
                .HasName("PK_Participants");

            entity.Property(p => p.ParticipantID)
                .ValueGeneratedNever();

            entity.HasOne(p => p.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(p => p.ParticipantID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Participants_Users");
        });

        // EVENTS
        modelBuilder.Entity<RaceEvent>(entity =>
        {
            entity.ToTable("Events", table =>
            {
                table.HasCheckConstraint(
                    "CK_Events_Distance", "[Distance] > 0");

                table.HasCheckConstraint(
                    "CK_Events_EventType",
                    "[EventType] IN (N'Run', N'Walk', N'Cycle')");
            });

            entity.HasKey(e => e.EventID)
                .HasName("PK_Events");

            entity.Property(e => e.EventName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.EventDate)
                .HasColumnType("date");

            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.Distance)
                .HasColumnType("decimal(6,2)");

            entity.Property(e => e.EventType)
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(e => e.Organiser)
                .WithMany(o => o.Events)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Events_Organisers");
        });

        // CATEGORIES
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories", table =>
            {
                table.HasCheckConstraint(
                    "CK_Categories_MinimumAge",
                    "[MinimumAge] IS NULL OR [MinimumAge] >= 0");

                table.HasCheckConstraint(
                    "CK_Categories_MaximumAge",
                    "[MaximumAge] IS NULL OR [MaximumAge] >= 0");

                table.HasCheckConstraint(
                    "CK_Categories_AgeRange",
                    "[MinimumAge] IS NULL OR [MaximumAge] IS NULL OR [MinimumAge] <= [MaximumAge]");

                table.HasCheckConstraint(
                    "CK_Categories_Distance",
                    "[Distance] IS NULL OR [Distance] > 0");
            });

            entity.HasKey(c => c.CategoryID)
                .HasName("PK_Categories");

            entity.Property(c => c.CategoryName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(c => c.Distance)
                .HasColumnType("decimal(6,2)");

            entity.HasAlternateKey(c => new
            { c.EventID, c.CategoryID })
                .HasName("UQ_Categories_Event_Category");

            entity.HasOne(c => c.Event)
                .WithMany(e => e.Categories)
                .HasForeignKey(c => c.EventID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Categories_Events");
        });

        //enrollment
        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.ToTable("Enrolments");

            entity.HasKey(e => e.EnrolmentID)
                .HasName("PK_Enrolments");

            entity.Property(e => e.EnrolmentDate)
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasIndex(e => new
            { e.ParticipantID, e.EventID })
                .IsUnique()
                .HasDatabaseName(
                    "UQ_Enrolments_Participant_Event");

            entity.HasOne(e => e.Participant)
                .WithMany(p => p.Enrolments)
                .HasForeignKey(e => e.ParticipantID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Enrolments_Participants");

            entity.HasOne(e => e.Event)
                .WithMany(ev => ev.Enrolments)
                .HasForeignKey(e => e.EventID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Enrolments_Events");

 
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Enrolments)
                .HasForeignKey(e => new
                { e.EventID, e.CategoryID })
                .HasPrincipalKey(c => new
                { c.EventID, c.CategoryID })
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(
                    "FK_Enrolments_EventCategory");
        });

        //results
        modelBuilder.Entity<RaceResult>(entity =>
        {
            entity.ToTable("Results", table =>
            {
                table.HasCheckConstraint(
                    "CK_Results_FinishingPosition",
                    "[FinishingPosition] > 0");
            });

            entity.HasKey(r => r.ResultID)
                .HasName("PK_Results");

            entity.Property(r => r.FinishTime)
                .HasColumnType("time(0)");

            entity.HasIndex(r => r.EnrolmentID)
                .IsUnique()
                .HasDatabaseName("UQ_Results_Enrolment");

            entity.HasOne(r => r.Enrolment)
                .WithOne(e => e.Result)
                .HasForeignKey<RaceResult>(r => r.EnrolmentID)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName("FK_Results_Enrolments");
        });
    }
}
