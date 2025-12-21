using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class StudentLostReport
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int? CategoryId { get; set; }

    public string? Description { get; set; }

    public DateTime? LostDate { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ItemCategory? Category { get; set; }

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<StudentClaim> StudentClaims { get; set; } = new List<StudentClaim>();
}
