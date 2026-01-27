using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Player
{
    public int Playerid { get; set; }

    public string Playername { get; set; } = null!;

    public string? Email { get; set; }

    public string? Passwordhash { get; set; }

    public string? Server { get; set; }

    public int? Gameid { get; set; }

    public string? Role { get; set; }

    public DateTime? Createdat { get; set; }

    public string? Status { get; set; }

    public virtual Game? Game { get; set; }

    public virtual ICollection<Imageupload> Imageuploads { get; set; } = new List<Imageupload>();

    public virtual ICollection<Leaderboardentry> Leaderboardentries { get; set; } = new List<Leaderboardentry>();
}
