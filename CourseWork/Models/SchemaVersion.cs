using System;
using System.Collections.Generic;

namespace CourseWork.Models;

public partial class SchemaVersion
{
    public int Id { get; set; }

    public string ScriptName { get; set; } = null!;

    public DateTime AppliedAt { get; set; }

    public string ScriptHash { get; set; } = null!;
}
