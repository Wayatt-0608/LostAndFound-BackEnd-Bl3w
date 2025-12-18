using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class SecurityVerificationRequest
{
    public int Id { get; set; }

    public int CaseId { get; set; }

    public int RequestedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Case Case { get; set; } = null!;

    public virtual User RequestedByNavigation { get; set; } = null!;

    public virtual ICollection<SecurityVerificationDecision> SecurityVerificationDecisions { get; set; } = new List<SecurityVerificationDecision>();
}
