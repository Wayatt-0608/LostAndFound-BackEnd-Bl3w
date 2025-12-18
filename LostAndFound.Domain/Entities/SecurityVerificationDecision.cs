using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class SecurityVerificationDecision
{
    public int Id { get; set; }

    public int RequestId { get; set; }

    public int SecurityOfficerId { get; set; }

    public string? Decision { get; set; }

    public string? Note { get; set; }

    public string? EvidenceImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual SecurityVerificationRequest Request { get; set; } = null!;

    public virtual User SecurityOfficer { get; set; } = null!;
}
