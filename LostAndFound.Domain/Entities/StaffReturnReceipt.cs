using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class StaffReturnReceipt
{
    public int Id { get; set; }

    public int CaseId { get; set; }

    public int ClaimId { get; set; }

    public int StaffId { get; set; }

    public DateTime? ReturnedAt { get; set; }

    public string? ReceiptImageUrl { get; set; }

    public virtual Case Case { get; set; } = null!;

    public virtual StudentClaim Claim { get; set; } = null!;

    public virtual User Staff { get; set; } = null!;
}
