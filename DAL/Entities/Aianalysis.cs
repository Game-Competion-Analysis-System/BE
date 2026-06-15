using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Aianalysis
{
    public int Analysisid { get; set; }

    public int? Uploadid { get; set; }

    public string? Aimodelversion { get; set; }

    public double? Confidencescore { get; set; }

    public DateTime? Processedtime { get; set; }

    public virtual ICollection<Aiextractedfield> Aiextractedfields { get; set; } = [];

    public virtual ICollection<Leaderboard> Leaderboards { get; set; } = [];

    public virtual Imageupload? Upload { get; set; }
}
