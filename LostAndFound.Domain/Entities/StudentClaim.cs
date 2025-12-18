using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class StudentClaim
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int FoundItemId { get; set; }

    public int? LostReportId { get; set; }

    public int? CaseId { get; set; }

    public string? Status { get; set; }

    public string? EvidenceImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Case? Case { get; set; }

    public virtual StaffFoundItem FoundItem { get; set; } = null!;

    public virtual StudentLostReport? LostReport { get; set; }

    public virtual ICollection<StaffReturnReceipt> StaffReturnReceipts { get; set; } = new List<StaffReturnReceipt>();

    public virtual User Student { get; set; } = null!;
}
