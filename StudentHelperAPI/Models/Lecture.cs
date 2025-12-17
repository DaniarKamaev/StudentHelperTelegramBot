using System;
using System.Collections.Generic;

namespace StudentHelperAPI.Models;

public partial class Lecture
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string ExternalUrl { get; set; } = null!;

    public string? Subject { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User CreatedByNavigation { get; set; } = null!;
}
