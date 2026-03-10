using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class User
{
    public int Userid { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? Passwordhash { get; set; }

    public string? Role { get; set; }

    public virtual ICollection<Imageupload> Imageuploads { get; set; } = new List<Imageupload>();
}
