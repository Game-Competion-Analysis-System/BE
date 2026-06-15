using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Leaderboard
{
    public int Leaderboardid { get; set; }

    public int? Eventid { get; set; }

    public string? Title { get; set; }

    public string? Metrictype { get; set; }

    public int? Createdfromanalysisid { get; set; }

    public virtual Aianalysis? Createdfromanalysis { get; set; }

    public virtual Event? Event { get; set; }

    public virtual ICollection<Leaderboardentry> Leaderboardentries { get; set; } = [];
}
