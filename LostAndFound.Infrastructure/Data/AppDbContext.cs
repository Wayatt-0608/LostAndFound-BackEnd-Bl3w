using System;
using System.Collections.Generic;
using LostAndFound.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LostAndFound.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Campus> Campuses { get; set; }

    public virtual DbSet<Case> Cases { get; set; }

    public virtual DbSet<EmailOtp> EmailOtps { get; set; }

    public virtual DbSet<ItemCategory> ItemCategories { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<SecurityVerificationDecision> SecurityVerificationDecisions { get; set; }

    public virtual DbSet<SecurityVerificationRequest> SecurityVerificationRequests { get; set; }

    public virtual DbSet<StaffFoundItem> StaffFoundItems { get; set; }

    public virtual DbSet<StaffReturnReceipt> StaffReturnReceipts { get; set; }

    public virtual DbSet<StudentClaim> StudentClaims { get; set; }

    public virtual DbSet<StudentLostReport> StudentLostReports { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__campus__3213E83F4A2F0F63");

            entity.ToTable("campus");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.Location)
                .HasMaxLength(200)
                .HasColumnName("location");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Case>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__cases__3213E83FBBCB67CD");

            entity.ToTable("cases");

            entity.HasIndex(e => e.CampusId, "IX_cases_campus_id");

            entity.HasIndex(e => e.Status, "IX_cases_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CampusId).HasColumnName("campus_id");
            entity.Property(e => e.ClosedAt)
                .HasColumnType("datetime")
                .HasColumnName("closed_at");
            entity.Property(e => e.FoundItemId).HasColumnName("found_item_id");
            entity.Property(e => e.OpenedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("opened_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.SuccessfulClaimId).HasColumnName("successful_claim_id");
            entity.Property(e => e.TotalClaims)
                .HasDefaultValue(0)
                .HasColumnName("total_claims");

            entity.HasOne(d => d.Campus).WithMany(p => p.Cases)
                .HasForeignKey(d => d.CampusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cases__campus_id__5441852A");

            entity.HasOne(d => d.FoundItem).WithMany(p => p.Cases)
                .HasForeignKey(d => d.FoundItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cases__found_ite__534D60F1");
        });

        modelBuilder.Entity<EmailOtp>(entity =>
        {
            entity.HasKey(e => e.OtpId).HasName("PK__EmailOTP__AEE35435957D9517");

            entity.ToTable("EmailOTP");

            entity.HasIndex(e => new { e.Email, e.Purpose, e.IsUsed, e.ExpiresAt }, "IX_EmailOTP_Email");

            entity.Property(e => e.OtpId).HasColumnName("otp_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.IsUsed)
                .HasDefaultValue(false)
                .HasColumnName("is_used");
            entity.Property(e => e.OtpCode)
                .HasMaxLength(50)
                .HasColumnName("otp_code");
            entity.Property(e => e.Purpose)
                .HasMaxLength(50)
                .HasColumnName("purpose");
        });

        modelBuilder.Entity<ItemCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__item_cat__3213E83F97640B0E");

            entity.ToTable("item_categories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IconUrl)
                .HasMaxLength(500)
                .HasColumnName("icon_url");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__notifica__3213E83F96F20579");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.Type, "IX_notifications_type");

            entity.HasIndex(e => e.UserId, "IX_notifications_user_id");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_notifications_user_id_created_at").IsDescending(false, true);

            entity.HasIndex(e => new { e.UserId, e.IsRead }, "IX_notifications_user_id_is_read");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .HasColumnName("message");
            entity.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id");
            entity.Property(e => e.RelatedEntityType)
                .HasMaxLength(50)
                .HasColumnName("related_entity_type");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__notificat__user___73BA3083");
        });

        modelBuilder.Entity<SecurityVerificationDecision>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__security__3213E83FD2A1D5EA");

            entity.ToTable("security_verification_decisions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Decision)
                .HasMaxLength(20)
                .HasColumnName("decision");
            entity.Property(e => e.EvidenceImageUrl)
                .HasMaxLength(500)
                .HasColumnName("evidence_image_url");
            entity.Property(e => e.Note)
                .HasMaxLength(300)
                .HasColumnName("note");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.SecurityOfficerId).HasColumnName("security_officer_id");

            entity.HasOne(d => d.Request).WithMany(p => p.SecurityVerificationDecisions)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__security___reque__656C112C");

            entity.HasOne(d => d.SecurityOfficer).WithMany(p => p.SecurityVerificationDecisions)
                .HasForeignKey(d => d.SecurityOfficerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__security___secur__66603565");
        });

        modelBuilder.Entity<SecurityVerificationRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__security__3213E83FA7643C2C");

            entity.ToTable("security_verification_requests");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaseId).HasColumnName("case_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.RequestedBy).HasColumnName("requested_by");

            entity.HasOne(d => d.Case).WithMany(p => p.SecurityVerificationRequests)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__security___case___5FB337D6");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.SecurityVerificationRequests)
                .HasForeignKey(d => d.RequestedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__security___reque__60A75C0F");
        });

        modelBuilder.Entity<StaffFoundItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__staff_fo__3213E83FB0360721");

            entity.ToTable("staff_found_items");

            entity.HasIndex(e => e.CampusId, "IX_staff_found_items_campus_id");

            entity.HasIndex(e => e.Status, "IX_staff_found_items_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CampusId).HasColumnName("campus_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.FoundDate)
                .HasColumnType("datetime")
                .HasColumnName("found_date");
            entity.Property(e => e.FoundLocation)
                .HasMaxLength(200)
                .HasColumnName("found_location");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("STORED")
                .HasColumnName("status");

            entity.HasOne(d => d.Campus).WithMany(p => p.StaffFoundItems)
                .HasForeignKey(d => d.CampusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__staff_fou__campu__4D94879B");

            entity.HasOne(d => d.Category).WithMany(p => p.StaffFoundItems)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__staff_fou__categ__4CA06362");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.StaffFoundItems)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__staff_fou__creat__4BAC3F29");
        });

        modelBuilder.Entity<StaffReturnReceipt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__staff_re__3213E83FF1BEA724");

            entity.ToTable("staff_return_receipts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaseId).HasColumnName("case_id");
            entity.Property(e => e.ClaimId).HasColumnName("claim_id");
            entity.Property(e => e.ReceiptImageUrl)
                .HasMaxLength(500)
                .HasColumnName("receipt_image_url");
            entity.Property(e => e.ReturnedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("returned_at");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Case).WithMany(p => p.StaffReturnReceipts)
                .HasForeignKey(d => d.CaseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__staff_ret__case___6A30C649");

            entity.HasOne(d => d.Claim).WithMany(p => p.StaffReturnReceipts)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__staff_ret__claim__6B24EA82");

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffReturnReceipts)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__staff_ret__staff__6C190EBB");
        });

        modelBuilder.Entity<StudentClaim>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__student___3213E83F67211FD3");

            entity.ToTable("student_claims");

            entity.HasIndex(e => e.CaseId, "IX_student_claims_case_id");

            entity.HasIndex(e => e.Status, "IX_student_claims_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CaseId).HasColumnName("case_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EvidenceImageUrl)
                .HasMaxLength(500)
                .HasColumnName("evidence_image_url");
            entity.Property(e => e.FoundItemId).HasColumnName("found_item_id");
            entity.Property(e => e.LostReportId).HasColumnName("lost_report_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Case).WithMany(p => p.StudentClaims)
                .HasForeignKey(d => d.CaseId)
                .HasConstraintName("FK__student_c__case___5BE2A6F2");

            entity.HasOne(d => d.FoundItem).WithMany(p => p.StudentClaims)
                .HasForeignKey(d => d.FoundItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__student_c__found__59FA5E80");

            entity.HasOne(d => d.LostReport).WithMany(p => p.StudentClaims)
                .HasForeignKey(d => d.LostReportId)
                .HasConstraintName("FK__student_c__lost___5AEE82B9");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentClaims)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__student_c__stude__59063A47");
        });

        modelBuilder.Entity<StudentLostReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__student___3213E83FC0A5B494");

            entity.ToTable("student_lost_reports");

            entity.HasIndex(e => e.CategoryId, "IX_student_lost_reports_category_id");

            entity.HasIndex(e => e.StudentId, "IX_student_lost_reports_student_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.LostDate)
                .HasColumnType("datetime")
                .HasColumnName("lost_date");
            entity.Property(e => e.LostLocation)
                .HasMaxLength(200)
                .HasColumnName("lost_location");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Category).WithMany(p => p.StudentLostReports)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__student_l__categ__45F365D3");

            entity.HasOne(d => d.Student).WithMany(p => p.StudentLostReports)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__student_l__stude__44FF419A");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FA477641E");

            entity.ToTable("users");

            entity.HasIndex(e => e.Role, "IX_users_role");

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164BC9A0B46").IsUnique();

            entity.HasIndex(e => e.StudentCode, "UX_users_student_code")
                .IsUnique()
                .HasFilter("([student_code] IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasColumnName("role");
            entity.Property(e => e.StudentCode)
                .HasMaxLength(20)
                .HasColumnName("student_code");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
