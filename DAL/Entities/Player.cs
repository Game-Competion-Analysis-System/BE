using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Player
{
    public int Playerid { get; set; }

    public string? Playername { get; set; }

    public int? Gameid { get; set; }

    public int? Serverid { get; set; }

    public int? Guildid { get; set; }

    public virtual Game? Game { get; set; }

    public virtual Guild? Guild { get; set; }

    public virtual ICollection<Leaderboardentry> Leaderboardentries { get; set; } = [];

    public virtual Server? Server { get; set; }
}
