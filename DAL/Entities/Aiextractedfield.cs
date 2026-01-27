using System;
using System.Collections.Generic;

namespace DAL.Entities;

public partial class Aiextractedfield
{
    public int Fieldid { get; set; }

    public int? Analysisid { get; set; }

    public string? Rawtext { get; set; }

    public string? Fieldtype { get; set; }

    public double? Confidence { get; set; }

    public virtual Aianalysis? Analysis { get; set; }
}
