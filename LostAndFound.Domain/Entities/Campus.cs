using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class Campus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Location { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Case> Cases { get; set; } = new List<Case>();

    public virtual ICollection<StaffFoundItem> StaffFoundItems { get; set; } = new List<StaffFoundItem>();
}
