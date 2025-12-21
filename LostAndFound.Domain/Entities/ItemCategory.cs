using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class ItemCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? IconUrl { get; set; }

    public virtual ICollection<StaffFoundItem> StaffFoundItems { get; set; } = new List<StaffFoundItem>();

    public virtual ICollection<StudentLostReport> StudentLostReports { get; set; } = new List<StudentLostReport>();
}
