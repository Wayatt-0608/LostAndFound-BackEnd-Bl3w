using System;
using System.Collections.Generic;

namespace LostAndFound.Domain.Entities;

public partial class EmailOtp
{
    public int OtpId { get; set; }

    public string Email { get; set; } = null!;

    public string OtpCode { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool? IsUsed { get; set; }

    public string? Purpose { get; set; }
}
