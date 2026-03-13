using Microsoft.EntityFrameworkCore.Migrations;

namespace DocumentService.Infrastructure.Persistence.Migrations;

// RLS Migration — enables Row Level Security on PostgreSQL tables
//
// What this does:
// 1. Enables RLS on documents table
// 2. Enables RLS on document_versions table
// 3. Creates policy: only show rows where tenant_id matches session var
// 4. FORCE RLS applies to table owner too — no bypass
//
// After this migration:
// SET LOCAL app.current_tenant_id = 'tenant-guid'
// SELECT * FROM documents ? only returns that tenant's rows
public partial class AddRowLevelSecurity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Enable RLS on documents table
        migrationBuilder.Sql(@"
            ALTER TABLE documents ENABLE ROW LEVEL SECURITY;
            ALTER TABLE documents FORCE ROW LEVEL SECURITY;
        ");

        // Create policy for documents
        // current_setting returns empty string if not set
        // We handle that by allowing empty string to return nothing
        migrationBuilder.Sql(@"
            CREATE POLICY tenant_isolation_documents
            ON documents
            USING (
                tenant_id::text = current_setting(
                    'app.current_tenant_id', true)
            );
        ");

        // Enable RLS on document_versions table
        // Versions are accessed through document — but RLS on both
        // provides defence in depth
        migrationBuilder.Sql(@"
            ALTER TABLE document_versions ENABLE ROW LEVEL SECURITY;
            ALTER TABLE document_versions FORCE ROW LEVEL SECURITY;
        ");

        // Create policy for document_versions
        // Joins to documents table to check tenant ownership
        migrationBuilder.Sql(@"
            CREATE POLICY tenant_isolation_document_versions
            ON document_versions
            USING (
                document_id IN (
                    SELECT id FROM documents
                    WHERE tenant_id::text = current_setting(
                        'app.current_tenant_id', true)
                )
            );
        ");

        // Grant usage on setting to saasuser
        migrationBuilder.Sql(@"
            GRANT ALL ON documents TO saasuser;
            GRANT ALL ON document_versions TO saasuser;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Remove RLS policies
        migrationBuilder.Sql(@"
            DROP POLICY IF EXISTS tenant_isolation_documents
            ON documents;
        ");

        migrationBuilder.Sql(@"
            DROP POLICY IF EXISTS tenant_isolation_document_versions
            ON document_versions;
        ");

        // Disable RLS
        migrationBuilder.Sql(@"
            ALTER TABLE documents DISABLE ROW LEVEL SECURITY;
            ALTER TABLE document_versions DISABLE ROW LEVEL SECURITY;
        ");
    }
}
