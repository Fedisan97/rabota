using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using rabota.Models;

namespace rabota.Context;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Film> Films { get; set; }

    public virtual DbSet<Hall> Halls { get; set; }

    public virtual DbSet<Price> Prices { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Viewer> Viewers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost; Database=postgres; Username=postgres; Password=Vaser314");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Film>(entity =>
        {
            entity.HasKey(e => e.FilmId).HasName("films_pkey");

            entity.ToTable("films");

            entity.Property(e => e.FilmId).HasColumnName("film_id");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Genre)
                .HasMaxLength(100)
                .HasColumnName("genre");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(e => e.HallId).HasName("halls_pkey");

            entity.ToTable("halls");

            entity.Property(e => e.HallId).HasColumnName("hall_id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Price>(entity =>
        {
            entity.HasKey(e => e.PriceId).HasName("prices_pkey");

            entity.ToTable("prices");

            entity.Property(e => e.PriceId).HasColumnName("price_id");
            entity.Property(e => e.PriceValue)
                .HasPrecision(10, 2)
                .HasColumnName("price_value");
            entity.Property(e => e.SeatType)
                .HasMaxLength(50)
                .HasColumnName("seat_type");
            entity.Property(e => e.SessionId).HasColumnName("session_id");

            entity.HasOne(d => d.Session).WithMany(p => p.Prices)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prices_session_id_fkey");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("promotions_pkey");

            entity.ToTable("promotions");

            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2)
                .HasColumnName("discount_percent");
            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_date");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.StartDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_date");

            entity.HasMany(d => d.Films).WithMany(p => p.Promotions)
                .UsingEntity<Dictionary<string, object>>(
                    "PromotionFilm",
                    r => r.HasOne<Film>().WithMany()
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("promotion_films_film_id_fkey"),
                    l => l.HasOne<Promotion>().WithMany()
                        .HasForeignKey("PromotionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("promotion_films_promotion_id_fkey"),
                    j =>
                    {
                        j.HasKey("PromotionId", "FilmId").HasName("promotion_films_pkey");
                        j.ToTable("promotion_films");
                        j.HasIndex(new[] { "PromotionId", "FilmId" }, "idx_promotion_films");
                        j.IndexerProperty<int>("PromotionId").HasColumnName("promotion_id");
                        j.IndexerProperty<int>("FilmId").HasColumnName("film_id");
                    });

            entity.HasMany(d => d.Sessions).WithMany(p => p.Promotions)
                .UsingEntity<Dictionary<string, object>>(
                    "PromotionSession",
                    r => r.HasOne<Session>().WithMany()
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("promotion_sessions_session_id_fkey"),
                    l => l.HasOne<Promotion>().WithMany()
                        .HasForeignKey("PromotionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("promotion_sessions_promotion_id_fkey"),
                    j =>
                    {
                        j.HasKey("PromotionId", "SessionId").HasName("promotion_sessions_pkey");
                        j.ToTable("promotion_sessions");
                        j.HasIndex(new[] { "PromotionId", "SessionId" }, "idx_promotion_sessions");
                        j.IndexerProperty<int>("PromotionId").HasColumnName("promotion_id");
                        j.IndexerProperty<int>("SessionId").HasColumnName("session_id");
                    });
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("sales_pkey");

            entity.ToTable("sales");

            entity.Property(e => e.SaleId).HasColumnName("sale_id");
            entity.Property(e => e.SaleTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("sale_time");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Sales)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_ticket_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Sales)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sales_user_id_fkey");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("sessions_pkey");

            entity.ToTable("sessions");

            entity.HasIndex(e => e.EndTime, "idx_sessions_end_time");

            entity.HasIndex(e => e.StartTime, "idx_sessions_start_time");

            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.EndTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_time");
            entity.Property(e => e.FilmId).HasColumnName("film_id");
            entity.Property(e => e.HallId).HasColumnName("hall_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_time");

            entity.HasOne(d => d.Film).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.FilmId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sessions_film_id_fkey");

            entity.HasOne(d => d.Hall).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.HallId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sessions_hall_id_fkey");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("tickets_pkey");

            entity.ToTable("tickets");

            entity.HasIndex(e => new { e.SessionId, e.SeatRow, e.SeatNumber }, "idx_tickets_seat");

            entity.HasIndex(e => e.SessionId, "idx_tickets_session_id");

            entity.HasIndex(e => e.QrCode, "tickets_qr_code_key").IsUnique();

            entity.HasIndex(e => new { e.SessionId, e.SeatRow, e.SeatNumber }, "unique_seat_per_session").IsUnique();

            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.IsSold)
                .HasDefaultValue(false)
                .HasColumnName("is_sold");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.QrCode)
                .HasMaxLength(255)
                .HasColumnName("qr_code");
            entity.Property(e => e.SeatNumber).HasColumnName("seat_number");
            entity.Property(e => e.SeatRow).HasColumnName("seat_row");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.ViewerId).HasColumnName("viewer_id");

            entity.HasOne(d => d.Session).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tickets_session_id_fkey");

            entity.HasOne(d => d.Viewer).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ViewerId)
                .HasConstraintName("tickets_viewer_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<Viewer>(entity =>
        {
            entity.HasKey(e => e.ViewerId).HasName("viewers_pkey");

            entity.ToTable("viewers");

            entity.Property(e => e.ViewerId).HasColumnName("viewer_id");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
