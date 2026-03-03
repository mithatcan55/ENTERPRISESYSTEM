using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations.LogDb
{
    /// <inheritdoc />
    public partial class EnforceImmutableLogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION logs.prevent_log_mutation()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    RAISE EXCEPTION 'Log tables are immutable. Operation % is not allowed on %.%', TG_OP, TG_TABLE_SCHEMA, TG_TABLE_NAME;
                END;
                $$;
                """);

            migrationBuilder.Sql(
                """
                CREATE TRIGGER trg_immutable_database_query_logs
                BEFORE UPDATE OR DELETE ON logs.database_query_logs
                FOR EACH ROW EXECUTE FUNCTION logs.prevent_log_mutation();

                CREATE TRIGGER trg_immutable_entity_change_logs
                BEFORE UPDATE OR DELETE ON logs.entity_change_logs
                FOR EACH ROW EXECUTE FUNCTION logs.prevent_log_mutation();

                CREATE TRIGGER trg_immutable_http_request_logs
                BEFORE UPDATE OR DELETE ON logs.http_request_logs
                FOR EACH ROW EXECUTE FUNCTION logs.prevent_log_mutation();

                CREATE TRIGGER trg_immutable_page_visit_logs
                BEFORE UPDATE OR DELETE ON logs.page_visit_logs
                FOR EACH ROW EXECUTE FUNCTION logs.prevent_log_mutation();

                CREATE TRIGGER trg_immutable_performance_logs
                BEFORE UPDATE OR DELETE ON logs.performance_logs
                FOR EACH ROW EXECUTE FUNCTION logs.prevent_log_mutation();

                CREATE TRIGGER trg_immutable_security_event_logs
                BEFORE UPDATE OR DELETE ON logs.security_event_logs
                FOR EACH ROW EXECUTE FUNCTION logs.prevent_log_mutation();

                CREATE TRIGGER trg_immutable_system_logs
                BEFORE UPDATE OR DELETE ON logs.system_logs
                FOR EACH ROW EXECUTE FUNCTION logs.prevent_log_mutation();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_immutable_database_query_logs ON logs.database_query_logs;
                DROP TRIGGER IF EXISTS trg_immutable_entity_change_logs ON logs.entity_change_logs;
                DROP TRIGGER IF EXISTS trg_immutable_http_request_logs ON logs.http_request_logs;
                DROP TRIGGER IF EXISTS trg_immutable_page_visit_logs ON logs.page_visit_logs;
                DROP TRIGGER IF EXISTS trg_immutable_performance_logs ON logs.performance_logs;
                DROP TRIGGER IF EXISTS trg_immutable_security_event_logs ON logs.security_event_logs;
                DROP TRIGGER IF EXISTS trg_immutable_system_logs ON logs.system_logs;
                """);

            migrationBuilder.Sql("DROP FUNCTION IF EXISTS logs.prevent_log_mutation();");
        }
    }
}
