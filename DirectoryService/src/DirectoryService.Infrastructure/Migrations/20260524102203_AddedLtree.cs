using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedLtree : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TABLE departments
                                 ALTER COLUMN path TYPE ltree
                                 USING path::ltree;
                                 """);
            
            migrationBuilder.Sql("CREATE INDEX idx_departments_path ON departments USING GIST (path);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS idx_departments_path;");
            
            migrationBuilder.Sql("""
                                 ALTER TABLE departments
                                 ALTER COLUMN path TYPE text
                                 USING path::text;
                                 """);
        }
    }
}
