using System;
using System.Collections.Generic;

namespace StudentHelperAPI.Models;

public partial class StudentGroup
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Publication> Publications { get; set; } = new List<Publication>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
