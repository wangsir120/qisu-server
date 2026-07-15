using Microsoft.EntityFrameworkCore;

namespace qisu_server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Models.User> Users { get; set; }
        public DbSet<Models.Host> Hosts { get; set; }
        public DbSet<Models.Admin> Admins { get; set; }
        public DbSet<Models.HostApplication> HostApplications { get; set; }
        public DbSet<Models.Message> Messages { get; set; }
        public DbSet<Models.Announcement> Announcements { get; set; }
        public DbSet<Models.Banner> Banners { get; set; }
        public DbSet<Models.OperationLog> OperationLogs { get; set; }
        public DbSet<Models.UserAnnouncementRead> UserAnnouncementReads { get; set; }
        public DbSet<Models.SystemConfig> SystemConfigs { get; set; }
        public DbSet<Models.ChatMessage> ChatMessages { get; set; }
        public DbSet<Models.ChatConversation> ChatConversations { get; set; }
        public DbSet<Models.SystemNotification> SystemNotifications { get; set; }
        public DbSet<Models.Favorite> Favorites { get; set; }
        public DbSet<Models.Order> Orders { get; set; }
        public DbSet<Models.Property> Properties { get; set; }
        public DbSet<Models.Review> Reviews { get; set; }
        public DbSet<Models.ReviewImage> ReviewImages { get; set; }
        public DbSet<Models.ReviewReply> ReviewReplies { get; set; }
        public DbSet<Models.HotDestination> HotDestinations { get; set; }
        public DbSet<Models.Theme> Themes { get; set; }
        public DbSet<Models.PropertyImage> PropertyImages { get; set; }
        public DbSet<Models.TravelStory> TravelStories { get; set; }
        public DbSet<Models.PropertyTheme> PropertyThemes { get; set; }
        public DbSet<Models.Address> Addresses { get; set; }
        public DbSet<Models.Room> Rooms { get; set; }
        public DbSet<Models.Bill> Bills { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.User>(entity =>
            {
                entity.ToTable("users", tb => tb.HasTrigger("trg_users_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Avatar).HasMaxLength(500);
                entity.Property(e => e.Nickname).HasMaxLength(50);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.IdCard).HasMaxLength(18).HasColumnName("id_card");
                entity.Property(e => e.IsVerified).HasColumnName("is_verified");
                entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Models.Admin>(entity =>
            {
                entity.ToTable("admins", tb => tb.HasTrigger("trg_admins_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.Avatar).HasMaxLength(500);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Role).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
                entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            });

            modelBuilder.Entity<Models.Host>(entity =>
            {
                entity.ToTable("hosts", tb => tb.HasTrigger("trg_hosts_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.Avatar).HasMaxLength(500);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.ResponseTime).HasMaxLength(50);
                entity.Property(e => e.Rating).HasPrecision(3, 2);
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.IsSuperhost).HasColumnName("is_superhost");
                entity.Property(e => e.ResponseRate).HasPrecision(5, 2).HasColumnName("response_rate");
                entity.Property(e => e.ResponseTime).HasColumnName("response_time");
                entity.Property(e => e.TotalListings).HasColumnName("total_listings");
                entity.Property(e => e.TotalReviews).HasColumnName("total_reviews");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Models.HostApplication>(entity =>
            {
                entity.ToTable("host_applications", tb => tb.HasTrigger("trg_host_applications_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.Name).HasMaxLength(50);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.IdCard).HasMaxLength(20);
                entity.Property(e => e.Province).HasMaxLength(50);
                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.District).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.PropertyType).HasMaxLength(50);
                entity.Property(e => e.PropertyTitle).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.IdCard).HasColumnName("id_card");
                entity.Property(e => e.Province).HasColumnName("province");
                entity.Property(e => e.City).HasColumnName("city");
                entity.Property(e => e.District).HasColumnName("district");
                entity.Property(e => e.Address).HasColumnName("address");
                entity.Property(e => e.PropertyType).HasColumnName("property_type");
                entity.Property(e => e.RoomCount).HasColumnName("room_count");
                entity.Property(e => e.BedCount).HasColumnName("bed_count");
                entity.Property(e => e.GuestCount).HasColumnName("guest_count");
                entity.Property(e => e.PropertyTitle).HasColumnName("property_title");
                entity.Property(e => e.PropertyDesc).HasColumnName("property_desc");
                entity.Property(e => e.Amenities).HasColumnName("amenities");
                entity.Property(e => e.Images).HasColumnName("images");
                entity.Property(e => e.AuditRemark).HasColumnName("audit_remark");
                entity.Property(e => e.AuditorId).HasColumnName("auditor_id");
                entity.Property(e => e.AuditedAt).HasColumnName("audited_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Models.Message>(entity =>
            {
                entity.ToTable("messages");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.IsRead);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Type).HasMaxLength(20);
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.IsRead).HasColumnName("is_read");
                entity.Property(e => e.RelatedId).HasColumnName("related_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<Models.Announcement>(entity =>
            {
                entity.ToTable("announcements");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsTop);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.IsTop).HasColumnName("is_top");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
                entity.Property(e => e.ViewCount).HasColumnName("view_count");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Models.Banner>(entity =>
            {
                entity.ToTable("banners", tb => tb.HasTrigger("trg_banners_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Position);
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Subtitle).HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired().HasColumnName("image_url");
                entity.Property(e => e.LinkUrl).HasMaxLength(500);
                entity.Property(e => e.LinkType).HasMaxLength(20);
                entity.Property(e => e.Gradient).HasMaxLength(200);
                entity.Property(e => e.Position).HasMaxLength(20);
                entity.Property(e => e.SortOrder).HasColumnName("sort_order");
                entity.Property(e => e.StartTime).HasColumnName("start_time");
                entity.Property(e => e.EndTime).HasColumnName("end_time");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Models.OperationLog>(entity =>
            {
                entity.ToTable("operation_logs");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.OperatorId);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.Type).HasMaxLength(20);
                entity.Property(e => e.OperatorId).HasColumnName("operator_id");
                entity.Property(e => e.OperatorName).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IpAddress).HasMaxLength(50);
                entity.Property(e => e.Browser).HasMaxLength(100);
                entity.Property(e => e.Os).HasMaxLength(100);
                entity.Property(e => e.RequestUrl).HasMaxLength(500);
                entity.Property(e => e.RequestMethod).HasMaxLength(10);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<Models.UserAnnouncementRead>(entity =>
            {
                entity.ToTable("user_announcement_reads");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.AnnouncementId);
                entity.HasIndex(e => new { e.UserId, e.AnnouncementId }).IsUnique();
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.AnnouncementId).HasColumnName("announcement_id");
                entity.Property(e => e.ReadAt).HasColumnName("read_at");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<Models.SystemConfig>(entity =>
            {
                entity.ToTable("system_configs", tb => tb.HasTrigger("trg_system_configs_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ConfigKey).IsUnique();
                entity.Property(e => e.ConfigKey).HasMaxLength(100).IsRequired().HasColumnName("config_key");
                entity.Property(e => e.ConfigValue).HasColumnName("config_value");
                entity.Property(e => e.Description).HasMaxLength(500).HasColumnName("description");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            });

            modelBuilder.Entity<Models.ChatMessage>(entity =>
            {
                entity.ToTable("cs_messages");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ConversationId);
                entity.HasIndex(e => e.SenderId);
                entity.HasIndex(e => e.ReceiverId);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.ConversationId).HasMaxLength(50);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.MessageType).HasMaxLength(20).HasDefaultValue("text");
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<Models.ChatConversation>(entity =>
            {
                entity.ToTable("cs_conversations");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ConversationId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.AdminId);
                entity.Property(e => e.ConversationId).HasMaxLength(50).IsRequired();
                entity.Property(e => e.LastMessage).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            });

            modelBuilder.Entity<Models.SystemNotification>(entity =>
            {
                entity.ToTable("system_notifications");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TargetUserId);
                entity.HasIndex(e => e.IsRead);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Content).HasMaxLength(500);
                entity.Property(e => e.Type).HasMaxLength(20).HasDefaultValue("info");
                entity.Property(e => e.TargetUserId).HasColumnName("target_user_id");
                entity.Property(e => e.TargetRole).HasMaxLength(20).HasColumnName("target_role");
                entity.Property(e => e.IsRead).HasColumnName("is_read");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            });

            modelBuilder.Entity<Models.Favorite>(entity =>
            {
                entity.ToTable("favorites", tb => tb.HasTrigger("trg_favorites_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => new { e.UserId, e.PropertyId }).IsUnique();
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.PropertyId).HasColumnName("property_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Property).WithMany().HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Models.Property>(entity =>
            {
                entity.ToTable("properties", tb => tb.HasTrigger("trg_properties_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.HostId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PricePerNight);
                entity.HasIndex(e => e.Rating);

                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(4000);
                entity.Property(e => e.PropertyType).HasMaxLength(20).HasColumnName("property_type");
                entity.Property(e => e.Area).HasPrecision(10, 2);
                entity.Property(e => e.BedType).HasMaxLength(200).HasColumnName("bed_type");
                entity.Property(e => e.Bedrooms).HasColumnName("bedrooms").HasDefaultValue(1);
                entity.Property(e => e.Beds).HasColumnName("beds").HasDefaultValue(1);
                entity.Property(e => e.Bathrooms).HasColumnName("bathrooms").HasDefaultValue(1);
                entity.Property(e => e.MaxGuests).HasColumnName("max_guests").HasDefaultValue(2);
                entity.Property(e => e.PricePerNight).HasPrecision(10, 2).HasColumnName("price_per_night");
                entity.Property(e => e.CleaningFee).HasPrecision(10, 2).HasColumnName("cleaning_fee");
                entity.Property(e => e.ServiceFeeRate).HasPrecision(5, 2).HasColumnName("service_fee_rate");
                entity.Property(e => e.Rating).HasPrecision(3, 2);
                entity.Property(e => e.ReviewCount).HasColumnName("review_count");
                entity.Property(e => e.ViewCount).HasColumnName("view_count");
                entity.Property(e => e.FavoriteCount).HasColumnName("favorite_count");
                entity.Property(e => e.IsInstantBook).HasColumnName("is_instant_book");
                entity.Property(e => e.IsNew).HasColumnName("is_new");
                entity.Property(e => e.Status).HasDefaultValue((byte)1);
                entity.Property(e => e.RoomCount).HasColumnName("room_count").HasDefaultValue(1);
                entity.Property(e => e.Facilities).HasColumnName("facilities");
                entity.Property(e => e.AddressId).HasColumnName("address_id");
                entity.Property(e => e.HostId).HasColumnName("host_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Host).WithMany().HasForeignKey(e => e.HostId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.PropertyAddress).WithMany().HasForeignKey(e => e.AddressId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Models.Review>(entity =>
            {
                entity.ToTable("reviews", tb => tb.HasTrigger("trg_reviews_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.HostId);
                entity.HasIndex(e => e.Rating);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.OrderId).HasColumnName("order_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.PropertyId).HasColumnName("property_id");
                entity.Property(e => e.HostId).HasColumnName("host_id");
                entity.Property(e => e.Rating).IsRequired();
                entity.Property(e => e.CleanlinessRating).HasColumnName("cleanliness_rating");
                entity.Property(e => e.CommunicationRating).HasColumnName("communication_rating");
                entity.Property(e => e.CheckinRating).HasColumnName("checkin_rating");
                entity.Property(e => e.AccuracyRating).HasColumnName("accuracy_rating");
                entity.Property(e => e.LocationRating).HasColumnName("location_rating");
                entity.Property(e => e.ValueRating).HasColumnName("value_rating");
                entity.Property(e => e.Content).HasMaxLength(2000);
                entity.Property(e => e.IsAnonymous).HasColumnName("is_anonymous");
                entity.Property(e => e.HostReply).HasColumnName("host_reply");
                entity.Property(e => e.HostReplyTime).HasColumnName("host_reply_time");
                entity.Property(e => e.Status).HasDefaultValue((byte)1);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Order).WithMany().HasForeignKey(e => e.OrderId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Property).WithMany().HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Host).WithMany().HasForeignKey(e => e.HostId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Models.ReviewImage>(entity =>
            {
                entity.ToTable("review_images");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ReviewId);
                entity.Property(e => e.ReviewId).HasColumnName("review_id");
                entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired().HasColumnName("image_url");
                entity.Property(e => e.SortOrder).HasColumnName("sort_order");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Review).WithMany(r => r.Images).HasForeignKey(e => e.ReviewId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Models.ReviewReply>(entity =>
            {
                entity.ToTable("review_replies");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ReviewId);
                entity.HasIndex(e => e.HostId);
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.ReviewId).HasColumnName("review_id");
                entity.Property(e => e.HostId).HasColumnName("host_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Review).WithMany(r => r.Replies).HasForeignKey(e => e.ReviewId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Host).WithMany().HasForeignKey(e => e.HostId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Models.Order>(entity =>
            {
                entity.ToTable("orders", tb => tb.HasTrigger("trg_orders_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.OrderNo).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.HostId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CheckInDate);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.RoomId);

                entity.Property(e => e.OrderNo).HasMaxLength(50).IsRequired().HasColumnName("order_no");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.PropertyId).HasColumnName("property_id");
                entity.Property(e => e.RoomId).HasColumnName("room_id");
                entity.Property(e => e.HostId).HasColumnName("host_id");
                entity.Property(e => e.CheckInDate).HasColumnName("check_in_date");
                entity.Property(e => e.CheckOutDate).HasColumnName("check_out_date");
                entity.Property(e => e.Nights).IsRequired();
                entity.Property(e => e.GuestCount).HasDefaultValue(1).HasColumnName("guest_count");
                entity.Property(e => e.GuestName).HasMaxLength(50).HasColumnName("guest_name");
                entity.Property(e => e.GuestPhone).HasMaxLength(20).HasColumnName("guest_phone");
                entity.Property(e => e.GuestIdCard).HasMaxLength(18).HasColumnName("guest_id_card");
                entity.Property(e => e.PricePerNight).HasPrecision(10, 2).HasColumnName("price_per_night");
                entity.Property(e => e.Subtotal).HasPrecision(10, 2);
                entity.Property(e => e.CleaningFee).HasPrecision(10, 2).HasColumnName("cleaning_fee");
                entity.Property(e => e.ServiceFee).HasPrecision(10, 2).HasColumnName("service_fee");
                entity.Property(e => e.TotalPrice).HasPrecision(10, 2).HasColumnName("total_price");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("pending");
                entity.Property(e => e.PaymentMethod).HasMaxLength(20).HasColumnName("payment_method");
                entity.Property(e => e.PaymentTime).HasColumnName("payment_time");
                entity.Property(e => e.PayDeadline).HasColumnName("pay_deadline");
                entity.Property(e => e.CancelReason).HasMaxLength(500).HasColumnName("cancel_reason");
                entity.Property(e => e.CancelTime).HasColumnName("cancel_time");
                entity.Property(e => e.RefundAmount).HasPrecision(10, 2).HasColumnName("refund_amount");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Property).WithMany().HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Room).WithMany().HasForeignKey(e => e.RoomId).OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Host).WithMany().HasForeignKey(e => e.HostId).OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<Models.HotDestination>(entity =>
            {
                entity.ToTable("hot_destinations");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SortOrder);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.HotScore);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Image).HasMaxLength(500);
                entity.Property(e => e.PropertyCount).HasColumnName("property_count");
                entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
                entity.Property(e => e.Status).HasDefaultValue(true);
                entity.Property(e => e.SearchCount).HasColumnName("search_count").HasDefaultValue(0);
                entity.Property(e => e.BookingCount).HasColumnName("booking_count").HasDefaultValue(0);
                entity.Property(e => e.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
                entity.Property(e => e.HotScore).HasColumnName("hot_score").HasPrecision(10, 2).HasDefaultValue(0m);
                entity.Property(e => e.LastUpdatedBy).HasColumnName("last_updated_by").HasMaxLength(50);
                entity.Ignore(e => e.Description);
                entity.Ignore(e => e.Region);
                entity.Ignore(e => e.Rating);
                entity.Ignore(e => e.BestTime);
                entity.Ignore(e => e.TrafficGuide);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Models.Theme>(entity =>
            {
                entity.ToTable("themes", tb => tb.HasTrigger("trg_themes_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SortOrder);
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500).HasColumnName("image_url");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.PropertyCount).HasColumnName("property_count").HasDefaultValue(0);
                entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
                entity.Property(e => e.Status).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Models.PropertyImage>(entity =>
            {
                entity.ToTable("property_images");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.IsCover);
                entity.Property(e => e.PropertyId).HasColumnName("property_id");
                entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired().HasColumnName("image_url");
                entity.Property(e => e.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
                entity.Property(e => e.IsCover).HasColumnName("is_cover").HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Property).WithMany(p => p.Images).HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Models.TravelStory>(entity =>
            {
                entity.ToTable("travel_stories", tb => tb.HasTrigger("trg_travel_stories_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.StoryType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.ImageUrl).HasMaxLength(500).HasColumnName("image_url");
                entity.Property(e => e.StoryType).HasMaxLength(30).HasColumnName("story_type").HasDefaultValue("travel_story");
                entity.Property(e => e.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
                entity.Property(e => e.LikeCount).HasColumnName("like_count").HasDefaultValue(0);
                entity.Property(e => e.Status).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Models.PropertyTheme>(entity =>
            {
                entity.ToTable("property_themes");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.ThemeId);
                entity.HasIndex(new[] { "PropertyId", "ThemeId" }).IsUnique();

                entity.Property(e => e.PropertyId).HasColumnName("property_id");
                entity.Property(e => e.ThemeId).HasColumnName("theme_id");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Property)
                    .WithMany(p => p.PropertyThemes)
                    .HasForeignKey(e => e.PropertyId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Theme)
                    .WithMany(t => t.PropertyThemes)
                    .HasForeignKey(e => e.ThemeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Models.Address>(entity =>
            {
                entity.ToTable("addresses", tb => tb.HasTrigger("trg_addresses_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.HostId);
                entity.HasIndex(e => e.City);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Province).HasMaxLength(50);
                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.District).HasMaxLength(50);
                entity.Property(e => e.Detail).HasMaxLength(500).IsRequired().HasColumnName("address");
                entity.Property(e => e.FullAddress).HasMaxLength(500);
                entity.Property(e => e.Latitude).HasPrecision(10, 7);
                entity.Property(e => e.Longitude).HasPrecision(10, 7);
                entity.Property(e => e.PoiId).HasMaxLength(50);
                entity.Property(e => e.PoiName).HasMaxLength(200);
                entity.Property(e => e.IsDefault).HasDefaultValue(false);
                entity.Property(e => e.Remark).HasMaxLength(500);
                entity.Property(e => e.Status).HasDefaultValue((byte)1);
                entity.Property(e => e.HostId).HasColumnName("host_id");
                entity.Property(e => e.IsDefault).HasColumnName("is_default");
                entity.Property(e => e.FullAddress).HasColumnName("full_address");
                entity.Property(e => e.PoiId).HasColumnName("poi_id");
                entity.Property(e => e.PoiName).HasColumnName("poi_name");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Host).WithMany().HasForeignKey(e => e.HostId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Models.Room>(entity =>
            {
                entity.ToTable("rooms", tb => tb.HasTrigger("trg_rooms_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PropertyId);
                entity.HasIndex(e => e.Status);
                entity.Property(e => e.PropertyId).HasColumnName("property_id");
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.RoomType).HasMaxLength(50).HasColumnName("room_type");
                entity.Property(e => e.Area).HasPrecision(10, 2);
                entity.Property(e => e.BedType).HasMaxLength(200).HasColumnName("bed_type");
                entity.Property(e => e.Beds).HasDefaultValue(1);
                entity.Property(e => e.MaxGuests).HasColumnName("max_guests").HasDefaultValue(2);
                entity.Property(e => e.PricePerNight).HasPrecision(10, 2).HasColumnName("price_per_night");
                entity.Property(e => e.Floor).HasDefaultValue(1);
                entity.Property(e => e.Status).HasDefaultValue((byte)1);
                entity.Property(e => e.Facilities).HasColumnName("facilities");
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Property).WithMany().HasForeignKey(e => e.PropertyId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Models.Bill>(entity =>
            {
                entity.ToTable("bills", tb => tb.HasTrigger("trg_bills_updated_at"));
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.HostId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.HostId).HasColumnName("host_id");
                entity.Property(e => e.Type).HasMaxLength(20).HasDefaultValue("income");
                entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Amount).HasPrecision(10, 2);
                entity.Property(e => e.OrderNo).HasMaxLength(50).HasColumnName("order_no");
                entity.Property(e => e.GuestName).HasMaxLength(50).HasColumnName("guest_name");
                entity.Property(e => e.PayMethod).HasMaxLength(20).HasColumnName("pay_method");
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("completed");
                entity.Property(e => e.Remark).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Host).WithMany().HasForeignKey(e => e.HostId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
