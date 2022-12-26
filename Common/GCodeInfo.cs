using System.Text;

namespace MasjidBandung.Common;

public sealed class GCodeInfo : IDisposable {
    public GCodeInfo(GCode gCode) { GCode = gCode; }

    /// <summary>
    /// Referensi objek gcode
    /// </summary>
    public GCode GCode { get; }

    /// <summary>
    /// Buffer gcode sebelum dikirim ke arduino secara bersamaan
    /// </summary>
    public string CommandBuffer { get; set; } = string.Empty;

    public void SetBuffer(string command) {
        CommandBuffer = command;
    }

    /// <summary>
    /// Mengirim buffer ke arduino
    /// </summary>
    public void SendBuffer() {
        GCode.Send(CommandBuffer);
    }

    /// <summary>
    /// Referensi ke motor, mengacu ke objek yang sama pada ChannelMap
    /// </summary>
    /// <remarks>
    /// Karena saat ini satu arduino hanya bisa untuk 2 motor,
    /// maka langsung saja batasi jumlah motor tersebut.
    /// </remarks>
    public MotorInfo[] Motors { get; } = new MotorInfo[2];

    /// <summary>
    /// Posisi menjadi perintah gcode
    /// </summary>
    public void Append(StringBuilder stringBuilder) {
        var x = Motors[MotorChannel.X].TargetPositionMillimeter;
        var y = Motors[MotorChannel.Y].TargetPositionMillimeter;
        stringBuilder.Append('X').Append(x.ToString("f3"));
        stringBuilder.Append('Y').Append(y.ToString("f3"));
    }

    /// <summary>
    /// Menghitung kecepatan pada gcode berdasarkan jarak tempuh dan durasi
    /// </summary>
    /// <remarks>
    /// Jarak tempuh masing-masing dihitung dari selisih LastPosition dan TargetPosition.
    /// Kecepatan dari kedua motor digabungkan dengan pythagoras 
    /// </remarks>
    /// <param name="duration"></param>
    /// <returns></returns>
    public int GetSpeed(int duration) {
        // duration dalam detik
        // jika durasi tidak diset, maka diatur default 10 
        if (duration <= 0) duration = 10;

        for (int i = 0; i < 2; i++) {
            var motor = Motors[i];

            // menghitung delta jarak motor
            // arah gerak tidak diperhitungkan, yang penting jarak tempuh pergerakan motor
            var delta = Math.Abs(motor.TargetPositionMillimeter - motor.LastPositionMillimeter);

            // kecepatan dalam mm/min,
            // batasi juga jika terlalu cepat atau tidak valid
            var speed = (int) (60.0 * delta / duration);
            motor.Speed = speed switch {
                > 15000 => 15000,
                < 0 => 0,
                _ => speed
            };
        }

        return RootSquare(Motors[0].Speed, Motors[1].Speed);
    }

    /// <summary>
    /// Pada dasarnya rumus pythagoras
    /// </summary>
    private static int RootSquare(double x, double y) => (int) Math.Sqrt(x * x + y * y);

    public void Dispose() {
        GCode.Dispose();
    }
}
