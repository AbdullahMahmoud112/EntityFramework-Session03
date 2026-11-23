using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace OnlineLearningPlatform.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202611230002_SeedData")]
public partial class SeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
SET IDENTITY_INSERT [Instructors] ON;
INSERT INTO [Instructors] ([Id],[FullName],[Email],[Bio],[JoinedAt]) VALUES (1,N'Ahmed Mohamed',N'ahmed@example.com',N'Software Developer','2025-01-01T00:00:00');
SET IDENTITY_INSERT [Instructors] OFF;

SET IDENTITY_INSERT [Categories] ON;
INSERT INTO [Categories] ([Id],[Name],[Description],[ParentCategoryId]) VALUES
(1,N'Programming',NULL,NULL),(2,N'Backend',NULL,1),(3,N'ASP.NET',NULL,2),(4,N'Database',NULL,NULL);
SET IDENTITY_INSERT [Categories] OFF;

SET IDENTITY_INSERT [Courses] ON;
INSERT INTO [Courses] ([Id],[Title],[Description],[Price],[Level],[IsPublished],[PublishedAt],[InstructorId],[CategoryId]) VALUES
(1,N'C# Fundamentals',N'Learn C# basics',500.00,N'Beginner',1,'2025-02-01T00:00:00',1,1),
(2,N'ASP.NET Core',N'Learn ASP.NET Core',800.00,N'Intermediate',0,NULL,1,3);
SET IDENTITY_INSERT [Courses] OFF;

SET IDENTITY_INSERT [Students] ON;
INSERT INTO [Students] ([Id],[FullName],[Email],[DateOfBirth]) VALUES
(1,N'Mohamed Ali',N'mohamed@example.com','2003-05-10T00:00:00'),
(2,N'Omar Hassan',N'omar@example.com','2002-08-15T00:00:00');
SET IDENTITY_INSERT [Students] OFF;

INSERT INTO [Enrollments] ([StudentId],[CourseId],[EnrolledAt],[Progress],[Status],[FinalGrade])
VALUES (1,1,'2025-03-01T00:00:00',50.00,N'Active',NULL);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DELETE FROM [Enrollments]; DELETE FROM [Courses]; DELETE FROM [Students]; DELETE FROM [Categories]; DELETE FROM [Instructors];");
    }
}
