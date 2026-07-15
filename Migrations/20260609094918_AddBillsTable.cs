using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace qisu_server.Migrations
{
    /// <inheritdoc />
    public partial class AddBillsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "announcements",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    is_top = table.Column<bool>(type: "bit", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    view_count = table.Column<int>(type: "int", nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "banners",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    subtitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    link_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    link_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    gradient = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    position = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<bool>(type: "bit", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banners", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "host_applications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    id_card = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    city = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    district = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    property_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    room_count = table.Column<int>(type: "int", nullable: true),
                    bed_count = table.Column<int>(type: "int", nullable: true),
                    guest_count = table.Column<int>(type: "int", nullable: true),
                    property_title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    property_desc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    amenities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    audit_remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    auditor_id = table.Column<long>(type: "bigint", nullable: true),
                    audited_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_host_applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hosts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_superhost = table.Column<bool>(type: "bit", nullable: false),
                    response_rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    response_time = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Verified = table.Column<bool>(type: "bit", nullable: false),
                    total_listings = table.Column<int>(type: "int", nullable: false),
                    total_reviews = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hot_destinations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Image = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    property_count = table.Column<int>(type: "int", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    search_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    booking_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    view_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    hot_score = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    last_updated_by = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hot_destinations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    related_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operation_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    operator_id = table.Column<long>(type: "bigint", nullable: true),
                    operator_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    browser = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    os = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    request_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    request_method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    request_params = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    response_code = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    error_message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    duration = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_configs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    config_key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    config_value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_configs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "system_notifications",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "info"),
                    target_user_id = table.Column<long>(type: "bigint", nullable: true),
                    target_role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "themes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    property_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_themes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Avatar = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Nickname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    id_card = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    is_verified = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "addresses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    host_id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Province = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    District = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    full_address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    poi_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    poi_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    is_default = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_addresses_hosts_host_id",
                        column: x => x.host_id,
                        principalTable: "hosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bills",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    host_id = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "income"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    order_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    guest_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    pay_method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "completed"),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bills_hosts_host_id",
                        column: x => x.host_id,
                        principalTable: "hosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cs_conversations",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    conversation_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    admin_id = table.Column<long>(type: "bigint", nullable: true),
                    last_message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    last_message_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    unread_count = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "active"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cs_conversations", x => x.id);
                    table.ForeignKey(
                        name: "FK_cs_conversations_users_admin_id",
                        column: x => x.admin_id,
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_cs_conversations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "cs_messages",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    conversation_id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    sender_id = table.Column<long>(type: "bigint", nullable: false),
                    receiver_id = table.Column<long>(type: "bigint", nullable: true),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    message_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "text"),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cs_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_cs_messages_users_receiver_id",
                        column: x => x.receiver_id,
                        principalTable: "users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_cs_messages_users_sender_id",
                        column: x => x.sender_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "travel_stories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    story_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "travel_story"),
                    view_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    like_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_travel_stories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_travel_stories_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_announcement_reads",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    announcement_id = table.Column<long>(type: "bigint", nullable: false),
                    read_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_announcement_reads", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_announcement_reads_announcements_announcement_id",
                        column: x => x.announcement_id,
                        principalTable: "announcements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_announcement_reads_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "properties",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    host_id = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    property_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Area = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    bed_type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    bedrooms = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    beds = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    bathrooms = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    max_guests = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    price_per_night = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    cleaning_fee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    service_fee_rate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Rating = table.Column<decimal>(type: "decimal(3,2)", precision: 3, scale: 2, nullable: false),
                    review_count = table.Column<int>(type: "int", nullable: false),
                    view_count = table.Column<int>(type: "int", nullable: false),
                    favorite_count = table.Column<int>(type: "int", nullable: false),
                    is_instant_book = table.Column<bool>(type: "bit", nullable: false),
                    is_new = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    room_count = table.Column<int>(type: "int", nullable: true, defaultValue: 1),
                    facilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address_id = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_properties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_properties_addresses_address_id",
                        column: x => x.address_id,
                        principalTable: "addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_properties_hosts_host_id",
                        column: x => x.host_id,
                        principalTable: "hosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    property_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_favorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_favorites_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_favorites_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "property_images",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    property_id = table.Column<long>(type: "bigint", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_cover = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_property_images_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "property_themes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    property_id = table.Column<long>(type: "bigint", nullable: false),
                    theme_id = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_property_themes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_property_themes_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_property_themes_themes_theme_id",
                        column: x => x.theme_id,
                        principalTable: "themes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    property_id = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    room_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Area = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    bed_type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Beds = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    max_guests = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    price_per_night = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Status = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)1),
                    facilities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rooms_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_no = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    property_id = table.Column<long>(type: "bigint", nullable: false),
                    room_id = table.Column<long>(type: "bigint", nullable: true),
                    host_id = table.Column<long>(type: "bigint", nullable: false),
                    check_in_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    check_out_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nights = table.Column<int>(type: "int", nullable: false),
                    guest_count = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    guest_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    guest_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    guest_id_card = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    price_per_night = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    cleaning_fee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    service_fee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "pending"),
                    payment_method = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    payment_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    pay_deadline = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancel_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    cancel_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    refund_amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_orders_hosts_host_id",
                        column: x => x.host_id,
                        principalTable: "hosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_orders_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_orders_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    property_id = table.Column<long>(type: "bigint", nullable: false),
                    host_id = table.Column<long>(type: "bigint", nullable: false),
                    Rating = table.Column<byte>(type: "tinyint", nullable: false),
                    cleanliness_rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    communication_rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    checkin_rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    accuracy_rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    location_rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    value_rating = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    is_anonymous = table.Column<bool>(type: "bit", nullable: false),
                    host_reply = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    host_reply_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reviews_hosts_host_id",
                        column: x => x.host_id,
                        principalTable: "hosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_reviews_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_reviews_properties_property_id",
                        column: x => x.property_id,
                        principalTable: "properties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_reviews_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "review_images",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    review_id = table.Column<long>(type: "bigint", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_review_images_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_replies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    review_id = table.Column<long>(type: "bigint", nullable: false),
                    host_id = table.Column<long>(type: "bigint", nullable: true),
                    user_id = table.Column<long>(type: "bigint", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_replies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_review_replies_hosts_host_id",
                        column: x => x.host_id,
                        principalTable: "hosts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_review_replies_reviews_review_id",
                        column: x => x.review_id,
                        principalTable: "reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_review_replies_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_addresses_City",
                table: "addresses",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_addresses_host_id",
                table: "addresses",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "IX_admins_Username",
                table: "admins",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_announcements_is_top",
                table: "announcements",
                column: "is_top");

            migrationBuilder.CreateIndex(
                name: "IX_announcements_status",
                table: "announcements",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_banners_position",
                table: "banners",
                column: "position");

            migrationBuilder.CreateIndex(
                name: "IX_banners_status",
                table: "banners",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_bills_Category",
                table: "bills",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_bills_created_at",
                table: "bills",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_bills_host_id",
                table: "bills",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "IX_bills_Status",
                table: "bills",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_bills_Type",
                table: "bills",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_cs_conversations_admin_id",
                table: "cs_conversations",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "IX_cs_conversations_conversation_id",
                table: "cs_conversations",
                column: "conversation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cs_conversations_user_id",
                table: "cs_conversations",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_cs_messages_conversation_id",
                table: "cs_messages",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "IX_cs_messages_created_at",
                table: "cs_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_cs_messages_receiver_id",
                table: "cs_messages",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_cs_messages_sender_id",
                table: "cs_messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_property_id",
                table: "favorites",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_user_id",
                table: "favorites",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_favorites_user_id_property_id",
                table: "favorites",
                columns: new[] { "user_id", "property_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_host_applications_Status",
                table: "host_applications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_host_applications_user_id",
                table: "host_applications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_hosts_user_id",
                table: "hosts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_hot_destinations_hot_score",
                table: "hot_destinations",
                column: "hot_score");

            migrationBuilder.CreateIndex(
                name: "IX_hot_destinations_sort_order",
                table: "hot_destinations",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "IX_hot_destinations_Status",
                table: "hot_destinations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_messages_is_read",
                table: "messages",
                column: "is_read");

            migrationBuilder.CreateIndex(
                name: "IX_messages_user_id",
                table: "messages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_operation_logs_created_at",
                table: "operation_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_operation_logs_operator_id",
                table: "operation_logs",
                column: "operator_id");

            migrationBuilder.CreateIndex(
                name: "IX_operation_logs_type",
                table: "operation_logs",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_orders_check_in_date",
                table: "orders",
                column: "check_in_date");

            migrationBuilder.CreateIndex(
                name: "IX_orders_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_orders_host_id",
                table: "orders",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_order_no",
                table: "orders",
                column: "order_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_property_id",
                table: "orders",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_room_id",
                table: "orders",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_orders_Status",
                table: "orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_orders_user_id",
                table: "orders",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_properties_address_id",
                table: "properties",
                column: "address_id");

            migrationBuilder.CreateIndex(
                name: "IX_properties_host_id",
                table: "properties",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "IX_properties_price_per_night",
                table: "properties",
                column: "price_per_night");

            migrationBuilder.CreateIndex(
                name: "IX_properties_Rating",
                table: "properties",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_properties_Status",
                table: "properties",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_property_images_is_cover",
                table: "property_images",
                column: "is_cover");

            migrationBuilder.CreateIndex(
                name: "IX_property_images_property_id",
                table: "property_images",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_property_themes_property_id",
                table: "property_themes",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_property_themes_property_id_theme_id",
                table: "property_themes",
                columns: new[] { "property_id", "theme_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_property_themes_theme_id",
                table: "property_themes",
                column: "theme_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_images_review_id",
                table: "review_images",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_replies_host_id",
                table: "review_replies",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_replies_review_id",
                table: "review_replies",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_replies_user_id",
                table: "review_replies",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_created_at",
                table: "reviews",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_host_id",
                table: "reviews",
                column: "host_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_order_id",
                table: "reviews",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_property_id",
                table: "reviews",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_Rating",
                table: "reviews",
                column: "Rating");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user_id",
                table: "reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_property_id",
                table: "rooms",
                column: "property_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_Status",
                table: "rooms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_system_configs_config_key",
                table: "system_configs",
                column: "config_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_system_notifications_created_at",
                table: "system_notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_system_notifications_is_read",
                table: "system_notifications",
                column: "is_read");

            migrationBuilder.CreateIndex(
                name: "IX_system_notifications_target_user_id",
                table: "system_notifications",
                column: "target_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_themes_sort_order",
                table: "themes",
                column: "sort_order");

            migrationBuilder.CreateIndex(
                name: "IX_themes_Status",
                table: "themes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_travel_stories_created_at",
                table: "travel_stories",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_travel_stories_Status",
                table: "travel_stories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_travel_stories_story_type",
                table: "travel_stories",
                column: "story_type");

            migrationBuilder.CreateIndex(
                name: "IX_travel_stories_user_id",
                table: "travel_stories",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_announcement_reads_announcement_id",
                table: "user_announcement_reads",
                column: "announcement_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_announcement_reads_user_id",
                table: "user_announcement_reads",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_announcement_reads_user_id_announcement_id",
                table: "user_announcement_reads",
                columns: new[] { "user_id", "announcement_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_Phone",
                table: "users",
                column: "Phone",
                unique: true,
                filter: "[Phone] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "banners");

            migrationBuilder.DropTable(
                name: "bills");

            migrationBuilder.DropTable(
                name: "cs_conversations");

            migrationBuilder.DropTable(
                name: "cs_messages");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "host_applications");

            migrationBuilder.DropTable(
                name: "hot_destinations");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "operation_logs");

            migrationBuilder.DropTable(
                name: "property_images");

            migrationBuilder.DropTable(
                name: "property_themes");

            migrationBuilder.DropTable(
                name: "review_images");

            migrationBuilder.DropTable(
                name: "review_replies");

            migrationBuilder.DropTable(
                name: "system_configs");

            migrationBuilder.DropTable(
                name: "system_notifications");

            migrationBuilder.DropTable(
                name: "travel_stories");

            migrationBuilder.DropTable(
                name: "user_announcement_reads");

            migrationBuilder.DropTable(
                name: "themes");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "announcements");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "properties");

            migrationBuilder.DropTable(
                name: "addresses");

            migrationBuilder.DropTable(
                name: "hosts");
        }
    }
}
