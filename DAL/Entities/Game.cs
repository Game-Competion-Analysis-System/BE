using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Game
{
    public int Gameid { get; set; }

    public string? Gamename { get; set; }

    public string? Genre { get; set; }

    public int? Companyid { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();

    public virtual ICollection<Server> Servers { get; set; } = new List<Server>();
}
