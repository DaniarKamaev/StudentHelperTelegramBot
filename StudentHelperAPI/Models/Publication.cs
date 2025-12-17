using System;
using System.Collections.Generic;

namespace StudentHelperAPI.Models;

public partial class Publication
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? PublicationType { get; set; }

    public Guid AuthorId { get; set; }

    public Guid? GroupId { get; set; }

    public bool? IsPublished { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual StudentGroup? Group { get; set; }
}
