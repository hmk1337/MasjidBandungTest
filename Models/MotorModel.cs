using System.ComponentModel;

namespace MasjidBandung.Models;

public class MotorStatusModel {
    public int Id { get; set; }
    public double Position { get; set; }
    public MachineState State { get; set; } = MachineState.Other;
}

public class MotorCommandRequest {
    [DefaultValue("NamaKoreografi")]
    public string Coreography { get; set; } = string.Empty;

    [DefaultValue(new[] {0, 0, 0, 0,0, 0, 0, 0})]
    public double[]? Position { get; set; }

    [DefaultValue(new[] {"#ffffff", "#ffffff", "#ffffff", "#ffffff"})]
    public string[] Color { get; set; } = Array.Empty<string>();

    public int? Speed { get; set; }
    public int? Time { get; set; }
}
