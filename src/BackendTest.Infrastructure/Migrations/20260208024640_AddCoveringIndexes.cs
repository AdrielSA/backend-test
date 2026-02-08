using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackendTest.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCoveringIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_Movies_Title_Cover
                ON Movies (Title)
                INCLUDE (Director, Genre, Description, ReleaseYear, DurationMinutes, IsActive, CreatedAt, UpdatedAt, Id);
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_Movies_Director_Cover
                ON Movies (Director)
                INCLUDE (Title, Genre, Description, ReleaseYear, DurationMinutes, IsActive, CreatedAt, UpdatedAt, Id);
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX IX_Movies_Genre_Cover
                ON Movies (Genre)
                INCLUDE (Title, Director, Description, ReleaseYear, DurationMinutes, IsActive, CreatedAt, UpdatedAt, Id);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Movies_Title_Cover ON Movies;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Movies_Director_Cover ON Movies;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Movies_Genre_Cover ON Movies;");
        }
    }
}
