namespace MasjidBandung.Models;

public static class MotorSettings {
    public static int MinimumDuration => 1;
    public static int TravelDistance {get;set;} = 450;
    public static double TravelMultiplier => TravelDistance / 100.0;
}
