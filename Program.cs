
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnlineLearningPlatform;

// ========================= ENTITIES =========================

public enum CourseLevel { Beginner, Intermediate, Advanced }
public enum EnrollmentStatus { Active, Completed, Dropped }

public class Instructor
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Bio { get; set; }
    public DateTime JoinedAt { get; set; }
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public CourseLevel Level { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int InstructorId { get; set; }
    public int CategoryId { get; set; }
    public Instructor Instructor { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Student
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime DateOfBirth { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Enrollment
{
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public decimal Progress { get; set; }
    public EnrollmentStatus Status { get; set; }
    public decimal? FinalGrade { get; set; }
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

// ========================= CONFIGURATIONS =========================

public class InstructorConfiguration : IEntityTypeConfiguration<Instructor>
{
    public void Configure(EntityTypeBuilder<Instructor> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Email).IsRequired().HasMaxLength(150);
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.Bio).HasMaxLength(500);
        b.Property(x => x.JoinedAt).IsRequired();

        b.HasMany(x => x.Courses).WithOne(x => x.Instructor)
            .HasForeignKey(x => x.InstructorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).IsRequired().HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(500);

        b.HasOne(x => x.ParentCategory).WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Courses).WithOne(x => x.Category)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Title).IsRequired().HasMaxLength(150);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.Price).HasPrecision(10, 2);
        b.Property(x => x.Level).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.IsPublished).IsRequired();
        b.Property(x => x.PublishedAt).IsRequired(false);

        b.HasOne(x => x.Instructor).WithMany(x => x.Courses)
            .HasForeignKey(x => x.InstructorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Category).WithMany(x => x.Courses)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);

        b.ToTable(t => t.HasCheckConstraint(
            "CK_Course_PublishedDate",
            "([IsPublished] = 0 AND [PublishedAt] IS NULL) OR ([IsPublished] = 1 AND [PublishedAt] IS NOT NULL)"));
    }
}

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.FullName).IsRequired().HasMaxLength(100);
        b.Property(x => x.Email).IsRequired().HasMaxLength(150);
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.DateOfBirth).IsRequired();

        b.HasMany(x => x.Enrollments).WithOne(x => x.Student)
            .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> b)
    {
        b.HasKey(x => new { x.StudentId, x.CourseId });
        b.Property(x => x.EnrolledAt).IsRequired();
        b.Property(x => x.Progress).HasPrecision(5, 2).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.FinalGrade).HasPrecision(5, 2);

        b.HasOne(x => x.Student).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Course).WithMany(x => x.Enrollments)
            .HasForeignKey(x => x.CourseId).OnDelete(DeleteBehavior.Cascade);

        b.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Enrollment_Progress", "[Progress] >= 0 AND [Progress] <= 100");
            t.HasCheckConstraint("CK_Enrollment_FinalGrade",
                "[FinalGrade] IS NULL OR ([FinalGrade] >= 0 AND [FinalGrade] <= 100)");
            t.HasCheckConstraint("CK_Enrollment_StatusProgress",
                "([Progress] = 100 AND [Status] = 'Completed') OR ([Progress] < 100 AND [Status] <> 'Completed')");
            t.HasCheckConstraint("CK_Enrollment_GradeStatus",
                "[FinalGrade] IS NULL OR [Status] = 'Completed'");
        });
    }
}

// ========================= DATABASE & SEED DATA =========================

public class AppDbContext : DbContext
{
    // Set this to false only while creating the InitialModel migration.
    public static bool IncludeSeedData = false;

    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(
            @"Server=.;Database=OnlineLearningDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new InstructorConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new CourseConfiguration());
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new EnrollmentConfiguration());

        if (IncludeSeedData)
            Seed(modelBuilder);
    }

    private static void Seed(ModelBuilder b)
    {
        b.Entity<Instructor>().HasData(new Instructor
        {
            Id = 1,
            FullName = "Ahmed Mohamed",
            Email = "ahmed@example.com",
            Bio = "Software Developer",
            JoinedAt = new DateTime(2025, 1, 1)
        });

        b.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Programming" },
            new Category { Id = 2, Name = "Backend", ParentCategoryId = 1 },
            new Category { Id = 3, Name = "ASP.NET", ParentCategoryId = 2 },
            new Category { Id = 4, Name = "Database" }
        );

        b.Entity<Course>().HasData(
            new Course
            {
                Id = 1,
                Title = "C# Fundamentals",
                Description = "Learn C# basics",
                Price = 500,
                Level = CourseLevel.Beginner,
                IsPublished = true,
                PublishedAt = new DateTime(2025, 2, 1),
                InstructorId = 1,
                CategoryId = 1
            },
            new Course
            {
                Id = 2,
                Title = "ASP.NET Core",
                Description = "Learn ASP.NET Core",
                Price = 800,
                Level = CourseLevel.Intermediate,
                IsPublished = false,
                PublishedAt = null,
                InstructorId = 1,
                CategoryId = 3
            }
        );

        b.Entity<Student>().HasData(
            new Student { Id = 1, FullName = "Mohamed Ali", Email = "mohamed@example.com", DateOfBirth = new DateTime(2003, 5, 10) },
            new Student { Id = 2, FullName = "Omar Hassan", Email = "omar@example.com", DateOfBirth = new DateTime(2002, 8, 15) }
        );

        b.Entity<Enrollment>().HasData(new Enrollment
        {
            StudentId = 1,
            CourseId = 1,
            EnrolledAt = new DateTime(2025, 3, 1),
            Progress = 50,
            Status = EnrollmentStatus.Active,
            FinalGrade = null
        });
    }
}

// ========================= PROGRAM & OPERATIONS =========================

public class Program
{
    public static void Main()
    {
        using (var db = new AppDbContext())
            db.Database.Migrate();

        Console.WriteLine("=== Online Learning Platform ===");

        int instructorId = AddInstructor();
        int courseId = AddCourse(instructorId, 1);
        List<int> studentIds = AddStudents();

        // Read
        FindCourse(1);
        FindCourseByTitle("C# Fundamentals");
        FilterCourses(300, 900);
        ShowSubCategories(1);
        ShowTopLevelCategories();
        CourseRoster(1);
        ChangePrice(1, 550);

        // The new course is published before attempting enrollment.
        PublishCourse(courseId);
        if (studentIds.Count > 0)
        {
            EnrollStudent(studentIds[0], courseId);
            UpdateProgress(studentIds[0], courseId, 100);
            CourseRoster(courseId);
        }

        // Delete
        UnenrollStudent(1, 1);
        TryDeleteCategory(1);
        SafeDeleteCategory(1);

        // This course has an enrollment, so cascade delete can be seen here.
        DeleteCourse(courseId);

        Console.WriteLine("=== Finished ===");
    }

    // CREATE

    static int AddInstructor()
    {
        using var db = new AppDbContext();
        var instructor = new Instructor
        {
            FullName = "Mostafa Ibrahim",
            Email = "mostafa@example.com",
            Bio = "Backend Developer",
            JoinedAt = DateTime.Now
        };

        try
        {
            db.Instructors.Add(instructor);
            db.SaveChanges();
            Console.WriteLine($"Instructor added. Id: {instructor.Id}");
            return instructor.Id;
        }
        catch (DbUpdateException)
        {
            Console.WriteLine("Could not add instructor. The email may already exist.");
            return 0;
        }
    }

    static int AddCourse(int instructorId, int categoryId)
    {
        using var db = new AppDbContext();

        if (!db.Instructors.Any(x => x.Id == instructorId))
        {
            Console.WriteLine("Instructor not found. Course was not added.");
            return 0;
        }

        if (!db.Categories.Any(x => x.Id == categoryId))
        {
            Console.WriteLine("Category not found. Course was not added.");
            return 0;
        }

        var course = new Course
        {
            Title = "Entity Framework Core",
            Description = "Learn EF Core",
            Price = 700,
            Level = CourseLevel.Intermediate,
            IsPublished = false,
            PublishedAt = null,
            InstructorId = instructorId,
            CategoryId = categoryId
        };

        db.Courses.Add(course);
        db.SaveChanges();
        Console.WriteLine($"Course added. Id: {course.Id}");
        return course.Id;
    }

    static List<int> AddStudents()
    {
        using var db = new AppDbContext();

        var students = new List<Student>
        {
            new Student { FullName = "Ali Ahmed", Email = "ali@example.com", DateOfBirth = new DateTime(2003, 4, 10) },
            new Student { FullName = "Hassan Mohamed", Email = "hassan@example.com", DateOfBirth = new DateTime(2002, 6, 15) },
            new Student { FullName = "Khaled Samir", Email = "khaled@example.com", DateOfBirth = new DateTime(2004, 2, 20) }
        };

        try
        {
            db.Students.AddRange(students);
            db.SaveChanges();

            foreach (var student in students)
                Console.WriteLine($"Student added: {student.FullName}, Id: {student.Id}");

            return students.Select(x => x.Id).ToList();
        }
        catch (DbUpdateException)
        {
            Console.WriteLine("Could not add students. Check whether one of the emails already exists.");
            return new List<int>();
        }
    }

    static void EnrollStudent(int studentId, int courseId)
    {
        using var db = new AppDbContext();

        if (!db.Students.Any(x => x.Id == studentId))
        {
            Console.WriteLine("Enrollment refused: student not found.");
            return;
        }

        var course = db.Courses.FirstOrDefault(x => x.Id == courseId);
        if (course == null)
        {
            Console.WriteLine("Enrollment refused: course not found.");
            return;
        }

        if (!course.IsPublished)
        {
            Console.WriteLine("Enrollment refused: course is not published.");
            return;
        }

        if (db.Enrollments.Any(x => x.StudentId == studentId && x.CourseId == courseId))
        {
            Console.WriteLine("Enrollment refused: student is already enrolled.");
            return;
        }

        db.Enrollments.Add(new Enrollment
        {
            StudentId = studentId,
            CourseId = courseId,
            EnrolledAt = DateTime.Now,
            Progress = 0,
            Status = EnrollmentStatus.Active,
            FinalGrade = null
        });

        db.SaveChanges();
        Console.WriteLine("Student enrolled successfully.");
    }

    // READ

    static void FindCourse(int id)
    {
        using var db = new AppDbContext();
        var course = db.Courses.Find(id);

        if (course == null)
            Console.WriteLine("Course not found.");
        else
            Console.WriteLine($"Found by Id: {course.Title}");
    }

    static void FindCourseByTitle(string title)
    {
        using var db = new AppDbContext();
        var course = db.Courses.FirstOrDefault(x => x.Title == title);

        if (course == null)
            Console.WriteLine("Course not found.");
        else
            Console.WriteLine($"Found by title: {course.Title}");
    }

    static void FilterCourses(decimal minPrice, decimal maxPrice)
    {
        using var db = new AppDbContext();

        var courses = db.Courses
            .Where(x => x.Price >= minPrice && x.Price <= maxPrice)
            .OrderByDescending(x => x.Price)
            .Select(x => new { x.Title, x.Level, x.Price })
            .ToList();

        Console.WriteLine("Courses in price range:");
        if (courses.Count == 0)
            Console.WriteLine("No courses found.");

        foreach (var course in courses)
            Console.WriteLine($"{course.Title} | {course.Level} | {course.Price}");
    }

    static void ShowSubCategories(int categoryId)
    {
        using var db = new AppDbContext();
        var category = db.Categories.Find(categoryId);

        if (category == null)
        {
            Console.WriteLine("Category not found.");
            return;
        }

        var subCategories = db.Categories
            .Where(x => x.ParentCategoryId == categoryId)
            .ToList();

        Console.WriteLine($"Sub-categories of {category.Name}:");
        foreach (var item in subCategories)
            Console.WriteLine(item.Name);

        if (subCategories.Count == 0)
            Console.WriteLine("No sub-categories found.");
    }

    static void ShowTopLevelCategories()
    {
        using var db = new AppDbContext();
        var categories = db.Categories.Where(x => x.ParentCategoryId == null).ToList();

        Console.WriteLine("Top-level categories:");
        foreach (var category in categories)
            Console.WriteLine(category.Name);
    }

    static void CourseRoster(int courseId)
    {
        using var db = new AppDbContext();

        if (!db.Courses.Any(x => x.Id == courseId))
        {
            Console.WriteLine("Course not found.");
            return;
        }

        var studentIds = db.Enrollments
            .Where(x => x.CourseId == courseId)
            .Select(x => x.StudentId)
            .ToList();

        var students = db.Students
            .Where(x => studentIds.Contains(x.Id))
            .ToList();

        Console.WriteLine($"Enrolled students: {students.Count}");
        foreach (var student in students)
            Console.WriteLine(student.FullName);
    }

    // UPDATE

    static void ChangePrice(int courseId, decimal newPrice)
    {
        using var db = new AppDbContext();

        if (newPrice < 0)
        {
            Console.WriteLine("Price cannot be negative.");
            return;
        }

        var course = db.Courses.Find(courseId);
        if (course == null)
        {
            Console.WriteLine("Course not found.");
            return;
        }

        course.Price = newPrice;
        db.SaveChanges();
        Console.WriteLine("Course price updated.");
    }

    static void UpdateProgress(int studentId, int courseId, decimal newProgress)
    {
        using var db = new AppDbContext();

        if (newProgress < 0 || newProgress > 100)
        {
            Console.WriteLine("Progress must be between 0 and 100.");
            return;
        }

        var enrollment = db.Enrollments.Find(studentId, courseId);
        if (enrollment == null)
        {
            Console.WriteLine("Enrollment not found.");
            return;
        }

        enrollment.Progress = newProgress;
        if (newProgress == 100)
            enrollment.Status = EnrollmentStatus.Completed;
        else if (enrollment.Status == EnrollmentStatus.Completed)
        {
            enrollment.Status = EnrollmentStatus.Active;
            enrollment.FinalGrade = null;
        }

        db.SaveChanges();
        Console.WriteLine("Progress updated.");
    }

    static void PublishCourse(int courseId)
    {
        using var db = new AppDbContext();
        var course = db.Courses.Find(courseId);

        if (course == null)
        {
            Console.WriteLine("Course not found.");
            return;
        }

        course.IsPublished = true;
        course.PublishedAt ??= DateTime.Now;
        db.SaveChanges();
        Console.WriteLine("Course published.");
    }

    // DELETE

    static void UnenrollStudent(int studentId, int courseId)
    {
        using var db = new AppDbContext();
        var enrollment = db.Enrollments.Find(studentId, courseId);

        if (enrollment == null)
        {
            Console.WriteLine("Enrollment not found.");
            return;
        }

        db.Enrollments.Remove(enrollment);
        db.SaveChanges();
        Console.WriteLine("Student unenrolled.");
    }

    static void TryDeleteCategory(int categoryId)
    {
        using var db = new AppDbContext();
        var category = db.Categories.Find(categoryId);

        if (category == null)
        {
            Console.WriteLine("Category not found.");
            return;
        }

        db.Categories.Remove(category);

        try
        {
            db.SaveChanges();
            Console.WriteLine("Category deleted.");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine("Category could not be deleted: " + ex.GetBaseException().Message);
            db.Entry(category).State = EntityState.Unchanged;
        }
    }

    static void SafeDeleteCategory(int categoryId)
    {
        using var db = new AppDbContext();
        var category = db.Categories.Find(categoryId);

        if (category == null)
        {
            Console.WriteLine("Category not found.");
            return;
        }

        if (db.Categories.Any(x => x.ParentCategoryId == categoryId))
        {
            Console.WriteLine("Cannot delete category because it has sub-categories.");
            return;
        }

        if (db.Courses.Any(x => x.CategoryId == categoryId))
        {
            Console.WriteLine("Cannot delete category because it has courses.");
            return;
        }

        db.Categories.Remove(category);
        db.SaveChanges();
        Console.WriteLine("Category deleted successfully.");
    }

    static void DeleteCourse(int courseId)
    {
        using var db = new AppDbContext();
        var course = db.Courses.Find(courseId);

        if (course == null)
        {
            Console.WriteLine("Course not found.");
            return;
        }

        int before = db.Enrollments.Count(x => x.CourseId == courseId);
        Console.WriteLine($"Enrollments before deletion: {before}");

        db.Courses.Remove(course);
        db.SaveChanges();

        int after = db.Enrollments.Count(x => x.CourseId == courseId);
        Console.WriteLine($"Enrollments after deletion: {after}");
        Console.WriteLine("Course deleted.");

        // Enrollments are deleted automatically because the Course relationship
        // is configured with DeleteBehavior.Cascade.
    }
}