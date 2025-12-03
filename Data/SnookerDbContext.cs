using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SnookerGameManagementSystem.Models;

namespace SnookerGameManagementSystem.Data
{
    public class SnookerDbContext : DbContext
    {
        public SnookerDbContext(DbContextOptions<SnookerDbContext> options) : base(options)
        {
        }

        // DbSets - Initialize to prevent null reference warnings
        public DbSet<AppUser> AppUsers { get; set; } = null!;
        public DbSet<GameType> GameTypes { get; set; } = null!;
        public DbSet<GameRule> GameRules { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<Frame> Frames { get; set; } = null!;
        public DbSet<FrameParticipant> FrameParticipants { get; set; } = null!;
        public DbSet<LedgerCharge> LedgerCharges { get; set; } = null!;
        public DbSet<LedgerPayment> LedgerPayments { get; set; } = null!;
        public DbSet<PaymentAllocation> PaymentAllocations { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Create enum converters
            var overtimeModeConverter = new EnumToStringConverter<OvertimeMode>();
            var payerModeConverter = new EnumToStringConverter<PayerMode>();
            var sessionStatusConverter = new EnumToStringConverter<SessionStatus>();
            var payStatusConverter = new EnumToStringConverter<PayStatus>();
            var teamConverter = new EnumToStringConverter<Team>();
            
            // AppUser
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("app_user");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // GameType
            modelBuilder.Entity<GameType>(entity =>
            {
                entity.ToTable("game_type");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // GameRule
            modelBuilder.Entity<GameRule>(entity =>
            {
                entity.ToTable("game_rule");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.GameTypeId)
                    .HasColumnName("game_type_id");
                entity.Property(e => e.BaseRatePk).HasColumnName("base_rate_pk").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.TimeLimitMinutes).HasColumnName("time_limit_minutes");
                entity.Property(e => e.OvertimeMode)
                    .HasColumnName("overtime_mode")
                    .HasConversion(overtimeModeConverter)
                    .IsRequired();
                entity.Property(e => e.OvertimeRatePkMin).HasColumnName("overtime_rate_pk_min").HasColumnType("decimal(10,2)");
                entity.Property(e => e.OvertimeLumpSumPk).HasColumnName("overtime_lump_sum_pk").HasColumnType("decimal(10,2)");
                entity.Property(e => e.DefaultPayerMode)
                    .HasColumnName("default_payer_mode")
                    .HasConversion(payerModeConverter)
                    .IsRequired();
                
                // Ignore computed properties
                entity.Ignore(e => e.Description);
                entity.Ignore(e => e.BaseRate);
                entity.Ignore(e => e.OvertimeRate);

                entity.HasOne(e => e.GameType)
                    .WithMany(g => g.GameRules)
                    .HasForeignKey(e => e.GameTypeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customer");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(120).IsRequired();
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(30);
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                
                // Ignore computed property
                entity.Ignore(e => e.Balance);
            });

            // Session
            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("session");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
                entity.Property(e => e.GameTypeId)
                    .HasColumnName("game_type_id");
                entity.Property(e => e.StartedAt).HasColumnName("started_at").IsRequired();
                entity.Property(e => e.EndedAt).HasColumnName("ended_at");
                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasConversion(sessionStatusConverter)
                    .IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(e => e.GameType)
                    .WithMany(g => g.Sessions)
                    .HasForeignKey(e => e.GameTypeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Frame
            modelBuilder.Entity<Frame>(entity =>
            {
                entity.ToTable("frame");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.SessionId)
                    .HasColumnName("session_id");
                entity.Property(e => e.StartedAt).HasColumnName("started_at").IsRequired();
                entity.Property(e => e.EndedAt).HasColumnName("ended_at");
                entity.Property(e => e.WinnerCustomerId)
                    .HasColumnName("winner_customer_id");
                entity.Property(e => e.LoserCustomerId)
                    .HasColumnName("loser_customer_id");
                entity.Property(e => e.BaseRatePk).HasColumnName("base_rate_pk").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.OvertimeMinutes).HasColumnName("overtime_minutes");
                entity.Property(e => e.OvertimeAmountPk).HasColumnName("overtime_amount_pk").HasColumnType("decimal(10,2)");
                entity.Property(e => e.LumpSumFinePk).HasColumnName("lump_sum_fine_pk").HasColumnType("decimal(10,2)");
                entity.Property(e => e.DiscountPk).HasColumnName("discount_pk").HasColumnType("decimal(10,2)");
                entity.Property(e => e.TotalAmountPk).HasColumnName("total_amount_pk").HasColumnType("decimal(10,2)");
                entity.Property(e => e.PayerMode)
                    .HasColumnName("payer_mode")
                    .HasConversion(payerModeConverter)
                    .IsRequired();
                entity.Property(e => e.PayStatus)
                    .HasColumnName("pay_status")
                    .HasConversion(payStatusConverter);

                entity.HasOne(e => e.Session)
                    .WithMany(s => s.Frames)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.WinnerCustomer)
                    .WithMany()
                    .HasForeignKey(e => e.WinnerCustomerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.LoserCustomer)
                    .WithMany()
                    .HasForeignKey(e => e.LoserCustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // FrameParticipant
            modelBuilder.Entity<FrameParticipant>(entity =>
            {
                entity.ToTable("frame_participant");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.FrameId)
                    .HasColumnName("frame_id");
                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id");
                entity.Property(e => e.Team)
                    .HasColumnName("team")
                    .HasConversion(teamConverter);
                entity.Property(e => e.IsWinner).HasColumnName("is_winner");
                entity.Property(e => e.SharePk).HasColumnName("share_pk").HasColumnType("decimal(10,2)");

                entity.HasOne(e => e.Frame)
                    .WithMany(f => f.Participants)
                    .HasForeignKey(e => e.FrameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.FrameParticipants)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LedgerCharge
            modelBuilder.Entity<LedgerCharge>(entity =>
            {
                entity.ToTable("ledger_charge");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id");
                entity.Property(e => e.FrameId)
                    .HasColumnName("frame_id");
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(200).IsRequired();
                entity.Property(e => e.AmountPk).HasColumnName("amount_pk").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                
                // Ignore computed properties
                entity.Ignore(e => e.Amount);
                entity.Ignore(e => e.ChargedAt);

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.LedgerCharges)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Frame)
                    .WithMany(f => f.LedgerCharges)
                    .HasForeignKey(e => e.FrameId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // LedgerPayment
            modelBuilder.Entity<LedgerPayment>(entity =>
            {
                entity.ToTable("ledger_payment");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.CustomerId)
                    .HasColumnName("customer_id");
                entity.Property(e => e.AmountPk).HasColumnName("amount_pk").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.Method).HasColumnName("method").HasMaxLength(50);
                entity.Property(e => e.ReceivedAt).HasColumnName("received_at");

                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.LedgerPayments)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PaymentAllocation
            modelBuilder.Entity<PaymentAllocation>(entity =>
            {
                entity.ToTable("payment_allocation");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                    .HasColumnName("id");
                entity.Property(e => e.PaymentId)
                    .HasColumnName("payment_id");
                entity.Property(e => e.ChargeId)
                    .HasColumnName("charge_id");
                entity.Property(e => e.AllocatedAmountPk).HasColumnName("allocated_amount_pk").HasColumnType("decimal(10,2)").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                // Ignore computed property
                entity.Ignore(e => e.AllocatedAmount);

                entity.HasOne(e => e.Payment)
                    .WithMany(p => p.PaymentAllocations)
                    .HasForeignKey(e => e.PaymentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Charge)
                    .WithMany(c => c.PaymentAllocations)
                    .HasForeignKey(e => e.ChargeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
