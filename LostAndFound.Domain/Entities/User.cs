using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class User
{
    public int Id { get; set; }

    public string? FullName { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public int CampusId { get; set; }

    public string? AvatarUrl { get; set; }

    public virtual Campus Campus { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<SecurityVerificationDecision> SecurityVerificationDecisions { get; set; } = new List<SecurityVerificationDecision>();

    public virtual ICollection<SecurityVerificationRequest> SecurityVerificationRequests { get; set; } = new List<SecurityVerificationRequest>();

    public virtual ICollection<StaffFoundItem> StaffFoundItems { get; set; } = new List<StaffFoundItem>();

    public virtual ICollection<StaffReturnReceipt> StaffReturnReceipts { get; set; } = new List<StaffReturnReceipt>();

    public virtual ICollection<StudentClaim> StudentClaims { get; set; } = new List<StudentClaim>();

    public virtual ICollection<StudentLostReport> StudentLostReports { get; set; } = new List<StudentLostReport>();
}
