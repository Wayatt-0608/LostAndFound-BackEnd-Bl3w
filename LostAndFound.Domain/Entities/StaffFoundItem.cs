using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class StaffFoundItem
{
    public int Id { get; set; }

    public int CreatedBy { get; set; }

    public int? CategoryId { get; set; }

    public int CampusId { get; set; }

    public string? Description { get; set; }

    public DateTime? FoundDate { get; set; }

    public string? FoundLocation { get; set; }

    public string? Status { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Campus Campus { get; set; } = null!;

    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();

    public virtual ItemCategory? Category { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;

    public virtual ICollection<StudentClaim> StudentClaims { get; set; } = new List<StudentClaim>();
}
