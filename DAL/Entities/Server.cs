using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Server
{
    public int Serverid { get; set; }

    public int? Gameid { get; set; }

    public string? Servername { get; set; }

    public string? Region { get; set; }

    public string? Status { get; set; }

    public virtual Game? Game { get; set; }

    public virtual ICollection<Guild> Guilds { get; set; } = new List<Guild>();

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
}
