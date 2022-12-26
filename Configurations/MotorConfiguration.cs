namespace MasjidBandung.Configurations;

public sealed class MotorConfiguration {
    /// <summary>
    /// Jumlah motor tersedia
    /// </summary>
    public int MotorCount { get; set; }

    /// <summary>
    /// Durasi minimum pergerakan motor dalam detik
    /// </summary>
    public int MinimumDuration { get; set; } = 1;

    /// <summary>
    /// Jarak tempuh operasional motor
    /// </summary>
    public int TravelDistance { get; set; } = 450;

    /// <summary>
    /// Faktor pengali dari persen menjadi mm
    /// </summary>
    public double TravelMultiplier => TravelDistance / 100.0;
}
