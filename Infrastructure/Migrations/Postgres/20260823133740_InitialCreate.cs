using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bank",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    primary_color = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    institution_url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    image_url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "bank_connection",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    external_connection_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    institution_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    institution_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_sync_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_connection", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "data_retention_purge_runs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    last_run_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_retention_purge_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "financial_goals",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    target_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    current_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    target_date = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    previous_attempt_goal_id = table.Column<int>(type: "integer", nullable: true),
                    last_contribution_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_goals", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gamification_evaluation_runs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ran_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    users_processed = table.Column<int>(type: "integer", nullable: false),
                    events_triggered = table.Column<int>(type: "integer", nullable: false),
                    badges_unlocked = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gamification_evaluation_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_evaluation_runs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    last_run_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_evaluation_runs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status_transaction",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    description = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_status_transaction", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "transaction_group",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    installments = table.Column<short>(type: "smallint", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_group", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "type_transaction",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    description = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_type_transaction", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "type_user",
                columns: table => new
                {
                    code = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    description = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_type_user", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "bank_account",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bank_connection_id = table.Column<int>(type: "integer", nullable: false),
                    external_account_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    current_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bank_account", x => x.id);
                    table.ForeignKey(
                        name: "FK_bank_account_bank_connection_bank_connection_id",
                        column: x => x.bank_connection_id,
                        principalTable: "bank_connection",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    username = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    cpf = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cod_user_type = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_Access_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_type_user_cod_user_type",
                        column: x => x.cod_user_type,
                        principalTable: "type_user",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    cod_type_transaction = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deactivated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_card_invoice_category = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_type_transaction_cod_type_transaction",
                        column: x => x.cod_type_transaction,
                        principalTable: "type_transaction",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_category_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "consistency_counters",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    counter_key = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    current_streak = table.Column<int>(type: "integer", nullable: false),
                    best_streak = table.Column<int>(type: "integer", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consistency_counters", x => x.id);
                    table.ForeignKey(
                        name: "FK_consistency_counters_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "credit_card",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    bank_id = table.Column<int>(type: "integer", nullable: false),
                    due_day = table.Column<short>(type: "smallint", nullable: false),
                    closing_day = table.Column<short>(type: "smallint", nullable: false),
                    credit_limit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deactivated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_card", x => x.id);
                    table.ForeignKey(
                        name: "FK_credit_card_bank_bank_id",
                        column: x => x.bank_id,
                        principalTable: "bank",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_credit_card_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mission_state_snapshots",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    related_entity_id = table.Column<int>(type: "integer", nullable: true),
                    is_condition_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_evaluated_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mission_state_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_mission_state_snapshots_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_preferences",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    email_on_critical = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    email_on_important = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    email_on_informative = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_preferences", x => x.id);
                    table.ForeignKey(
                        name: "FK_notification_preferences_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    criticality = table.Column<int>(type: "integer", nullable: false),
                    message_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: true),
                    related_entity_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    related_entity_id = table.Column<int>(type: "integer", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_resolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_method",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deactivated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_method", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_method_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "point_ledger_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    significance = table.Column<int>(type: "integer", nullable: false),
                    points_delta = table.Column<int>(type: "integer", nullable: false),
                    tier_at_event = table.Column<int>(type: "integer", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_point_ledger_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_point_ledger_entries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_badges",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<int>(type: "integer", nullable: false),
                    unlocked_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_badges", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_badges_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_profile_photos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    photo = table.Column<byte[]>(type: "bytea", nullable: false),
                    content_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_profile_photos", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_profile_photos_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_rank_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    tier = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_rank_profiles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_rank_profiles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "verification_codes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    code_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    purpose = table.Column<int>(type: "integer", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    used = table.Column<bool>(type: "boolean", nullable: false),
                    attempts_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verification_codes", x => x.id);
                    table.ForeignKey(
                        name: "FK_verification_codes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "category_budget_goals",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    monthly_limit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    deactivated_at = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_budget_goals", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_budget_goals_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "category_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    deactivated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_item", x => x.id);
                    table.ForeignKey(
                        name: "FK_category_item_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "planned_commitment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    direction = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    periodicity = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: true),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deactivated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planned_commitment", x => x.id);
                    table.ForeignKey(
                        name: "FK_planned_commitment_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planned_commitment_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "card_purchase",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    credit_card_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    purchase_date = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    installments_count = table.Column<short>(type: "smallint", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    category_item_id = table.Column<int>(type: "integer", nullable: true),
                    origin = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    deactivated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_purchase", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_purchase_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_purchase_category_item_category_item_id",
                        column: x => x.category_item_id,
                        principalTable: "category_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_purchase_credit_card_credit_card_id",
                        column: x => x.credit_card_id,
                        principalTable: "credit_card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_purchase_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    cod_type_transaction = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: true),
                    category_item_id = table.Column<int>(type: "integer", nullable: true),
                    Installments = table.Column<short>(type: "smallint", nullable: true),
                    id_payment_method = table.Column<int>(type: "integer", nullable: false),
                    transaction_group_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_category_category_id",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_category_item_category_item_id",
                        column: x => x.category_item_id,
                        principalTable: "category_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_payment_method_id_payment_method",
                        column: x => x.id_payment_method,
                        principalTable: "payment_method",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_status_transaction_status",
                        column: x => x.status,
                        principalTable: "status_transaction",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_transaction_group_transaction_group_id",
                        column: x => x.transaction_group_id,
                        principalTable: "transaction_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_type_transaction_cod_type_transaction",
                        column: x => x.cod_type_transaction,
                        principalTable: "type_transaction",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "card_invoice",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    credit_card_id = table.Column<int>(type: "integer", nullable: false),
                    reference_year = table.Column<short>(type: "smallint", nullable: false),
                    reference_month = table.Column<short>(type: "smallint", nullable: false),
                    closing_date = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    transaction_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_invoice", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_invoice_credit_card_credit_card_id",
                        column: x => x.credit_card_id,
                        principalTable: "credit_card",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_invoice_transaction_transaction_id",
                        column: x => x.transaction_id,
                        principalTable: "transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_invoice_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "card_installment",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    card_purchase_id = table.Column<int>(type: "integer", nullable: false),
                    card_invoice_id = table.Column<int>(type: "integer", nullable: false),
                    installment_number = table.Column<short>(type: "smallint", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    status = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_installment", x => x.id);
                    table.ForeignKey(
                        name: "FK_card_installment_card_invoice_card_invoice_id",
                        column: x => x.card_invoice_id,
                        principalTable: "card_invoice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_installment_card_purchase_card_purchase_id",
                        column: x => x.card_purchase_id,
                        principalTable: "card_purchase",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_card_installment_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "bank",
                columns: new[] { "id", "active", "image_url", "institution_url", "name", "primary_color", "type" },
                values: new object[,]
                {
                    { 1, true, "", "https://cdn.pluggy.ai/assets/connector-icons/804.png", "99Pay", "#ef294b", "PERSONAL_BANK" },
                    { 2, true, "", "https://www.agorainvest.com.br/images/OpenFinance/agora.svg", "Ágora Investimentos", "#296fa7", "INVESTMENT" },
                    { 3, true, "", "https://cdn.pluggy.ai/assets/connector-icons/883.svg", "ASA", "#2b56c0", "PERSONAL_BANK" },
                    { 4, true, "", "https://cdn.pluggy.ai/assets/connector-icons/230.svg", "Avenue", "#000000", "INVESTMENT" },
                    { 5, true, "", "https://www.bancobmg.com.br/data/files/8C/A2/7F/0A/FBA318104A94D208970BE9C2/bmg_open_finance.svg", "Banco Bmg", "#fa6300", "PERSONAL_BANK" },
                    { 6, true, "", "https://cdn.pluggy.ai/assets/connector-icons/682.svg", "Banco BRB", "#003D7C", "PERSONAL_BANK" },
                    { 7, true, "", "https://static.digio.com.br/media/logo_81e95ae2ab.svg", "Banco Digio", "#ff6776", "PERSONAL_BANK" },
                    { 8, true, "", "https://www.bb.com.br/docs/pub/inst/img/LogoBB.svg", "Banco do Brasil", "#1194F6", "PERSONAL_BANK" },
                    { 9, true, "", "https://cdn.pluggy.ai/assets/connector-icons/671.svg", "Banco do Nordeste", "#ef294b", "PERSONAL_BANK" },
                    { 10, true, "", "https://cdn.pluggy.ai/assets/connector-icons/742.svg", "Banco Mercantil", "#ef294b", "PERSONAL_BANK" },
                    { 11, true, "", "https://cdn.pluggy.ai/assets/connector-icons/657.svg", "Banco PAN", "#02afff", "PERSONAL_BANK" },
                    { 12, true, "", "https://www.sofisa.com.br/openbanking/logo_sofisa.svg", "Banco Sofisa", "#004e46", "PERSONAL_BANK" },
                    { 13, true, "", "https://banrisul.com.br/bob/data/Simbolo-Banrisul.svg", "Banrisul", "#0B45E4", "PERSONAL_BANK" },
                    { 14, true, "", "https://banco.bradesco/open-finance/logo/icones_vetorial-pf.svg", "Bradesco", "#e5173f", "PERSONAL_BANK" },
                    { 15, true, "", "https://cdn.pluggy.ai/assets/connector-icons/203.svg", "Bradesco Cartões", "#e5173f", "PERSONAL_BANK" },
                    { 16, true, "", "https://banking-public-prd.s3.sa-east-1.amazonaws.com/open-finance/logo/btgbanking/btgbanking.svg", "BTG Pactual", "#66768F", "INVESTMENT" },
                    { 17, true, "", "https://www.bv.com.br/site/resources/open-finance/logo-bv.svg", "BV", "#223AD2", "PERSONAL_BANK" },
                    { 18, true, "", "https://cdn.pluggy.ai/assets/connector-icons/726.svg", "C6 Bank", "#FFE45C", "PERSONAL_BANK" },
                    { 19, true, "", "https://consentimento.openbanking.caixa.gov.br/assets/images/logomarca_caixa.svg", "Caixa Econômica Federal", "#296fa7", "PERSONAL_BANK" },
                    { 20, true, "", "https://cdn.pluggy.ai/assets/connector-icons/250.svg", "Cora", "#f51b81", "BUSINESS_BANK" },
                    { 21, true, "", "https://cdn.pluggy.ai/assets/connector-icons/250.svg", "Conta Simples", "#2DCC68", "BUSINESS_BANK" },
                    { 22, true, "", "https://cdn.pluggy.ai/assets/connector-icons/750.svg", "Crefisa", "#23b9e2", "PERSONAL_BANK" },
                    { 23, true, "", "https://cdn.pluggy.ai/assets/connector-icons/810.svg", "Dock", "#ef294b", "PERSONAL_BANK" },
                    { 24, true, "", "https://cdn.pluggy.ai/assets/connector-icons/239.svg", "Efí Bank", "#fb6910", "BUSINESS_BANK" },
                    { 25, true, "", "https://cdn.pluggy.ai/assets/connector-icons/271.svg", "EQI Investimentos", "#DB671F", "INVESTMENT" },
                    { 26, true, "", "https://cdn.pluggy.ai/assets/connector-icons/215.svg", "Inter", "#fb6910", "PERSONAL_BANK" },
                    { 27, true, "", "https://www.itau.com.br/assets/dam/publisher/07_itau_empresas/13_open_banking/logos_regulatorio_bacen/opb_log_reg_bac_itau_img_01.svg", "Itaú", "#EC7000", "PERSONAL_BANK" },
                    { 28, true, "", "https://http2.mlstatic.com/frontend-assets/opb-logos/logo.svg", "Mercado Pago", "#009ee3", "PERSONAL_BANK" },
                    { 29, true, "", "https://nuapp.nubank.com.br/open-banking/logo.svg", "Nubank", "#8a0fbe", "PERSONAL_BANK" },
                    { 30, true, "", "https://cdn.pluggy.ai/assets/connector-icons/692.svg", "PagBank", "#ef294b", "PERSONAL_BANK" },
                    { 31, true, "", "https://picpay.s3.sa-east-1.amazonaws.com/openbanking/picpay-logo-icon-pf.svg", "PicPay", "#238662", "PERSONAL_BANK" },
                    { 32, true, "", "https://cdn.pluggy.ai/assets/connector-icons/205.svg", "Rico Investimentos", "#ff5200", "INVESTMENT" },
                    { 33, true, "", "https://storage.googleapis.com/inic-data/safra-pf.svg", "Safra", "#00003C", "PERSONAL_BANK" },
                    { 34, true, "", "https://cms.santander.com.br/sites/WPS/imagem/img-santander-chama/21-08-06_200409_P_santander_chama.svg", "Santander", "#cc0000", "PERSONAL_BANK" },
                    { 35, true, "", "https://sicoob-openbanking.s3.sa-east-1.amazonaws.com/logo-sicoob.svg", "Sicoob", "#00AE9D", "PERSONAL_BANK" },
                    { 36, true, "", "https://www.sicredi.com.br/openbanking/app/assets/images/shared/logo/logo_sicredi_512.svg", "Sicredi", "#3FA110", "PERSONAL_BANK" },
                    { 37, true, "", "https://cdn.pluggy.ai/assets/connector-icons/787.svg", "Stone", "#00A868", "PERSONAL_BANK" },
                    { 38, true, "", "https://cdn.pluggy.ai/assets/connector-icons/796.svg", "Toro Investimentos", "#ef294b", "INVESTMENT" },
                    { 39, true, "", "https://www.unicred.com.br/logo.svg", "Unicred", "#1e5c49", "PERSONAL_BANK" },
                    { 40, true, "", "https://cdn.pluggy.ai/assets/connector-icons/291.svg", "Wise", "#9fe870", "PERSONAL_BANK" },
                    { 41, true, "", "https://cdn.pluggy.ai/assets/connector-icons/202.svg", "XP Investimentos", "#111111", "INVESTMENT" }
                });

            migrationBuilder.InsertData(
                table: "status_transaction",
                columns: new[] { "code", "description" },
                values: new object[,]
                {
                    { "C", "Canceled" },
                    { "E", "Expired" },
                    { "O", "Open" },
                    { "P", "Paid" }
                });

            migrationBuilder.InsertData(
                table: "type_transaction",
                columns: new[] { "code", "description" },
                values: new object[,]
                {
                    { "E", "Expense" },
                    { "I", "Income" },
                    { "R", "Redemption" },
                    { "V", "Investment" }
                });

            migrationBuilder.InsertData(
                table: "type_user",
                columns: new[] { "code", "description" },
                values: new object[,]
                {
                    { "A", "Admin" },
                    { "B", "Basic" },
                    { "C", "Corporate" },
                    { "P", "Premium" },
                    { "V", "VIP" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_bank_account_bank_connection_id",
                table: "bank_account",
                column: "bank_connection_id");

            migrationBuilder.CreateIndex(
                name: "IX_bank_connection_active",
                table: "bank_connection",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_bank_connection_user_id",
                table: "bank_connection",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_installment_card_invoice_id",
                table: "card_installment",
                column: "card_invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_installment_card_purchase_id",
                table: "card_installment",
                column: "card_purchase_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_installment_user_id",
                table: "card_installment",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_invoice_credit_card_id",
                table: "card_invoice",
                column: "credit_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_invoice_transaction_id",
                table: "card_invoice",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_invoice_user_id_credit_card_id_reference_year_referenc~",
                table: "card_invoice",
                columns: new[] { "user_id", "credit_card_id", "reference_year", "reference_month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_card_purchase_category_id",
                table: "card_purchase",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_purchase_category_item_id",
                table: "card_purchase",
                column: "category_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_purchase_credit_card_id",
                table: "card_purchase",
                column: "credit_card_id");

            migrationBuilder.CreateIndex(
                name: "IX_card_purchase_user_id_credit_card_id",
                table: "card_purchase",
                columns: new[] { "user_id", "credit_card_id" });

            migrationBuilder.CreateIndex(
                name: "IX_category_cod_type_transaction",
                table: "category",
                column: "cod_type_transaction");

            migrationBuilder.CreateIndex(
                name: "IX_category_user_id_description",
                table: "category",
                columns: new[] { "user_id", "description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_category_budget_goals_category_id",
                table: "category_budget_goals",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_category_budget_goals_user_id_category_id",
                table: "category_budget_goals",
                columns: new[] { "user_id", "category_id" });

            migrationBuilder.CreateIndex(
                name: "IX_category_item_category_id",
                table: "category_item",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_consistency_counters_user_id_counter_key",
                table: "consistency_counters",
                columns: new[] { "user_id", "counter_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_credit_card_bank_id",
                table: "credit_card",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "IX_credit_card_user_id_description",
                table: "credit_card",
                columns: new[] { "user_id", "description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_financial_goals_user_id_status",
                table: "financial_goals",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_mission_state_snapshots_user_id_type_related_entity_id",
                table: "mission_state_snapshots",
                columns: new[] { "user_id", "type", "related_entity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_preferences_user_id",
                table: "notification_preferences",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_is_archived_archived_at",
                table: "notifications",
                columns: new[] { "user_id", "is_archived", "archived_at" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_is_archived_is_read",
                table: "notifications",
                columns: new[] { "user_id", "is_archived", "is_read" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_user_id_is_resolved_criticality",
                table: "notifications",
                columns: new[] { "user_id", "is_resolved", "criticality" });

            migrationBuilder.CreateIndex(
                name: "IX_payment_method_user_id_description",
                table: "payment_method",
                columns: new[] { "user_id", "description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planned_commitment_category_id",
                table: "planned_commitment",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_planned_commitment_user_id_active",
                table: "planned_commitment",
                columns: new[] { "user_id", "active" });

            migrationBuilder.CreateIndex(
                name: "IX_point_ledger_entries_user_id_occurred_at",
                table: "point_ledger_entries",
                columns: new[] { "user_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_category_id",
                table: "transaction",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_category_item_id",
                table: "transaction",
                column: "category_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_cod_type_transaction",
                table: "transaction",
                column: "cod_type_transaction");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_id_payment_method",
                table: "transaction",
                column: "id_payment_method");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_status",
                table: "transaction",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_transaction_date",
                table: "transaction",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_transaction_group_id",
                table: "transaction",
                column: "transaction_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_user_id",
                table: "transaction",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_group_active",
                table: "transaction_group",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_group_user_id",
                table: "transaction_group",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_badges_user_id_code",
                table: "user_badges",
                columns: new[] { "user_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_profile_photos_user_id",
                table: "user_profile_photos",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_rank_profiles_user_id",
                table: "user_rank_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_cod_user_type",
                table: "users",
                column: "cod_user_type");

            migrationBuilder.CreateIndex(
                name: "IX_users_cpf",
                table: "users",
                column: "cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_verification_codes_user_id_purpose_created_at",
                table: "verification_codes",
                columns: new[] { "user_id", "purpose", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank_account");

            migrationBuilder.DropTable(
                name: "card_installment");

            migrationBuilder.DropTable(
                name: "category_budget_goals");

            migrationBuilder.DropTable(
                name: "consistency_counters");

            migrationBuilder.DropTable(
                name: "data_retention_purge_runs");

            migrationBuilder.DropTable(
                name: "financial_goals");

            migrationBuilder.DropTable(
                name: "gamification_evaluation_runs");

            migrationBuilder.DropTable(
                name: "mission_state_snapshots");

            migrationBuilder.DropTable(
                name: "notification_evaluation_runs");

            migrationBuilder.DropTable(
                name: "notification_preferences");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "planned_commitment");

            migrationBuilder.DropTable(
                name: "point_ledger_entries");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "user_badges");

            migrationBuilder.DropTable(
                name: "user_profile_photos");

            migrationBuilder.DropTable(
                name: "user_rank_profiles");

            migrationBuilder.DropTable(
                name: "verification_codes");

            migrationBuilder.DropTable(
                name: "bank_connection");

            migrationBuilder.DropTable(
                name: "card_invoice");

            migrationBuilder.DropTable(
                name: "card_purchase");

            migrationBuilder.DropTable(
                name: "transaction");

            migrationBuilder.DropTable(
                name: "credit_card");

            migrationBuilder.DropTable(
                name: "category_item");

            migrationBuilder.DropTable(
                name: "payment_method");

            migrationBuilder.DropTable(
                name: "status_transaction");

            migrationBuilder.DropTable(
                name: "transaction_group");

            migrationBuilder.DropTable(
                name: "bank");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "type_transaction");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "type_user");
        }
    }
}
