using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Guild
{
    public int Guildid { get; set; }

    public int? Serverid { get; set; }

    public string? Guildname { get; set; }

    public int? Leaderplayerid { get; set; }

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();

    public virtual Server? Server { get; set; }
}
