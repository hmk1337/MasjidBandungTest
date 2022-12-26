using System.Text;

namespace MasjidBandung.Common;

public sealed class MotorInfo : IEquatable<MotorInfo> {
    public MotorInfo(int index, string gCodeId, int channel) {
        Index = index;
        GCodeId = gCodeId;
        Channel = channel;
    }

    public int Index { get; }
    public string GCodeId { get; }
    public int Channel { get; }
    public int Speed { get; set; }
    public double PositionPercent { get; set; }
    public double LastPositionMillimeter { get; set; }
    public double TargetPositionMillimeter { get; set; }
    public string? Color { get; set; }
    public MachineState State { get; set; }
    

    public bool Equals(MotorInfo? other) => other != null && other.Index == Index && other.Channel == Channel;
    public override bool Equals(object? obj) => obj is MotorInfo motorInfo && Equals(motorInfo);
    public override int GetHashCode() => HashCode.Combine(Index, GCodeId, Channel);
}
