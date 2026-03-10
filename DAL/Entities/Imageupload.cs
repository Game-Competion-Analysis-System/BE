using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Imageupload
{
    public int Uploadid { get; set; }

    public int? Userid { get; set; }

    public int? Eventid { get; set; }

    public string? Imageurl { get; set; }

    public DateTime? Uploadtime { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<Aianalysis> Aianalyses { get; set; } = new List<Aianalysis>();

    public virtual Event? Event { get; set; }

    public virtual User? User { get; set; }
}
