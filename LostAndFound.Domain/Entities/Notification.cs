using System;

namespace LostAndFound.Domain.Entities;

public partial class Notification
{
    public int Id { get; set; }
    
    public int UserId { get; set; } // Người nhận notification
    
    public string Title { get; set; } = null!;
    
    public string Message { get; set; } = null!;
    
    public string Type { get; set; } = null!; // CLAIM_MATCHED, CLAIM_APPROVED, CLAIM_REJECTED, etc.
    
    public int? RelatedEntityId { get; set; } // ID của claim/lost report/found item
    
    public string? RelatedEntityType { get; set; } // "CLAIM", "LOST_REPORT", "FOUND_ITEM", "CASE"
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public virtual User User { get; set; } = null!;
}

