using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class Case
{
    public int Id { get; set; }

    public int FoundItemId { get; set; }

    public int CampusId { get; set; }

    public string? Status { get; set; }

    public int? TotalClaims { get; set; }

    public int? SuccessfulClaimId { get; set; }

    public DateTime? OpenedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public virtual Campus Campus { get; set; } = null!;

    public virtual StaffFoundItem FoundItem { get; set; } = null!;

    public virtual ICollection<SecurityVerificationRequest> SecurityVerificationRequests { get; set; } = new List<SecurityVerificationRequest>();

    public virtual ICollection<StaffReturnReceipt> StaffReturnReceipts { get; set; } = new List<StaffReturnReceipt>();

    public virtual ICollection<StudentClaim> StudentClaims { get; set; } = new List<StudentClaim>();
}
