using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Company
{
    public int Companyid { get; set; }

    public string Companyname { get; set; } = null!;

    public string? Country { get; set; }

    public string? Website { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
