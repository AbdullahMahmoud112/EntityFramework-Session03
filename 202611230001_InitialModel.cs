using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace OnlineLearningPlatform.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202611230001_InitialModel")]
public partial class InitialModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
CREATE TABLE [Instructors] (
 [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Instructors] PRIMARY KEY,
 [FullName] nvarchar(100) NOT NULL, [Email] nvarchar(150) NOT NULL,
 [Bio] nvarchar(500) NULL, [JoinedAt] datetime2 NOT NULL);
CREATE UNIQUE INDEX [IX_Instructors_Email] ON [Instructors]([Email]);

CREATE TABLE [Categories] (
 [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Categories] PRIMARY KEY,
 [Name] nvarchar(100) NOT NULL, [Description] nvarchar(500) NULL,
 [ParentCategoryId] int NULL,
 CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [Categories]([Id]) ON DELETE NO ACTION);
CREATE INDEX [IX_Categories_ParentCategoryId] ON [Categories]([ParentCategoryId]);

CREATE TABLE [Students] (
 [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Students] PRIMARY KEY,
 [FullName] nvarchar(100) NOT NULL, [Email] nvarchar(150) NOT NULL,
 [DateOfBirth] datetime2 NOT NULL);
CREATE UNIQUE INDEX [IX_Students_Email] ON [Students]([Email]);

CREATE TABLE [Courses] (
 [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Courses] PRIMARY KEY,
 [Title] nvarchar(150) NOT NULL, [Description] nvarchar(1000) NULL,
 [Price] decimal(10,2) NOT NULL, [Level] nvarchar(20) NOT NULL,
 [IsPublished] bit NOT NULL, [PublishedAt] datetime2 NULL,
 [InstructorId] int NOT NULL, [CategoryId] int NOT NULL,
 CONSTRAINT [FK_Courses_Instructors_InstructorId] FOREIGN KEY ([InstructorId]) REFERENCES [Instructors]([Id]) ON DELETE NO ACTION,
 CONSTRAINT [FK_Courses_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories]([Id]) ON DELETE NO ACTION,
 CONSTRAINT [CK_Course_PublishedDate] CHECK (([IsPublished] = 0 AND [PublishedAt] IS NULL) OR ([IsPublished] = 1 AND [PublishedAt] IS NOT NULL)));
CREATE INDEX [IX_Courses_InstructorId] ON [Courses]([InstructorId]);
CREATE INDEX [IX_Courses_CategoryId] ON [Courses]([CategoryId]);

CREATE TABLE [Enrollments] (
 [StudentId] int NOT NULL, [CourseId] int NOT NULL, [EnrolledAt] datetime2 NOT NULL,
 [Progress] decimal(5,2) NOT NULL, [Status] nvarchar(20) NOT NULL, [FinalGrade] decimal(5,2) NULL,
 CONSTRAINT [PK_Enrollments] PRIMARY KEY ([StudentId],[CourseId]),
 CONSTRAINT [FK_Enrollments_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ON DELETE CASCADE,
 CONSTRAINT [FK_Enrollments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses]([Id]) ON DELETE CASCADE,
 CONSTRAINT [CK_Enrollment_Progress] CHECK ([Progress] >= 0 AND [Progress] <= 100),
 CONSTRAINT [CK_Enrollment_FinalGrade] CHECK ([FinalGrade] IS NULL OR ([FinalGrade] >= 0 AND [FinalGrade] <= 100)),
 CONSTRAINT [CK_Enrollment_StatusProgress] CHECK (([Progress] = 100 AND [Status] = 'Completed') OR ([Progress] < 100 AND [Status] <> 'Completed')),
 CONSTRAINT [CK_Enrollment_GradeStatus] CHECK ([FinalGrade] IS NULL OR [Status] = 'Completed'));
CREATE INDEX [IX_Enrollments_CourseId] ON [Enrollments]([CourseId]);
");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"DROP TABLE [Enrollments]; DROP TABLE [Courses]; DROP TABLE [Students]; DROP TABLE [Categories]; DROP TABLE [Instructors];");
    }
}
