using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Event
{
    public int Eventid { get; set; }

    public int? Gameid { get; set; }

    public string? Eventname { get; set; }

    public string? Eventtype { get; set; }

    public DateTime? Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public virtual Game? Game { get; set; }

    public virtual ICollection<Imageupload> Imageuploads { get; set; } = [];

    public virtual ICollection<Leaderboard> Leaderboards { get; set; } = [];
}
