using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Leaderboardentry
{
    public int Entryid { get; set; }

    public int? Leaderboardid { get; set; }

    public int? Playerid { get; set; }

    public int? Rank { get; set; }

    public double? Value { get; set; }

    public virtual Leaderboard? Leaderboard { get; set; }

    public virtual Player? Player { get; set; }
}
