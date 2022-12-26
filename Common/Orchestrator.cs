using System.Text;
using MasjidBandung.Configurations;
using Microsoft.Extensions.Options;
using Ngb.SerialPortHelper;

namespace MasjidBandung.Common;

public static class MotorChannel {
    public const int X = 0;
    public const int Y = 1;
}

public sealed class Orchestrator : IDisposable {
    private readonly ChannelMap _motors;
    private readonly Dictionary<string, GCodeInfo> _gcodeMap = new();
    private readonly ISerialPortProvider _serialPortProvider;
    private readonly ILedService _led;
    private readonly MotorConfiguration _options;
    private readonly ILogger<Orchestrator> _logger;
    public int Count => _options.MotorCount;
    private int _duration;

    public IEnumerable<MotorInfo> Motors {
        get {
            foreach (var item in _motors) yield return item;
        }
    }

    public Orchestrator(IOptions<MotorConfiguration> options, ISerialPortProvider serialPortProvider, ILedService led, ILogger<Orchestrator> logger) {
        _serialPortProvider = serialPortProvider;
        _options = options.Value;
        _led = led;
        _logger = logger;
        _duration = options.Value.MinimumDuration;

        // jumlah motor diatur permanen, tidak dapat diubah saat runtime
        _motors = new ChannelMap(options.Value.MotorCount);
    }

    /// <summary>
    /// Menambahkan motor ke sistem. Tidak disarankan dipanggil saat web service sudah berjalan.
    /// </summary>
    /// <param name="index">Nomor motor</param>
    /// <param name="gcodeId">Nama grup gcode</param>
    /// <param name="channel">Channel motor, disaranken menggunakan <see cref="MotorChannel"/></param>
    /// <returns></returns>
    public Orchestrator AddMotor(int index, string gcodeId, int channel) {
        if (index < 0) throw new IndexOutOfRangeException("Invalid index value");
        if (index >= Count) throw new IndexOutOfRangeException("Jumlah motor tidak dapat lebih dari yang ditentukan");
        if (channel != MotorChannel.X && channel != MotorChannel.Y) throw new Exception("Invalid channel");

        var motor = new MotorInfo(index, gcodeId, channel);
        _motors[index] = motor;

        // daftarkan objek gcode jika belum terdaftar
        if (!_gcodeMap.ContainsKey(gcodeId)) {
            var port = _serialPortProvider.GetPort(gcodeId);
            var gcodeInfo = new GCodeInfo(new GCode(port));
            gcodeInfo.Motors[channel] = motor;
            _gcodeMap.Add(gcodeId, gcodeInfo);
        } else {
            _gcodeMap[gcodeId].Motors[channel] = motor;
        }


        return this;
    }

    public Orchestrator SetTravelDistance(int travelDistance) {
        _options.TravelDistance = travelDistance;
        _logger.LogDebug("Set travel distance to {Distance}mm", travelDistance);
        return this;
    }

    public Orchestrator SetDuration(int duration) {
        _duration = Math.Max(duration, _options.MinimumDuration);
        _logger.LogDebug("Set duration {Duration} seconds", duration);
        return this;
    }

    public MotorInfo GetMotor(int index) => _motors[index];

    public void GetState() {
        // mengambil state dari setiap gcode untuk disimpan di setiap objek motor,
        // untuk mempermudah GET /status
        foreach (var gcode in _gcodeMap) {
            var item = gcode.Value;
            var state = item.GCode.State;
            foreach (var motor in item.Motors) {
                motor.State = state;
            }
        }
    }

    /// <summary>
    /// Homing dan reset untuk setiap motor
    /// </summary>
    public void Reset() {
        _logger.LogInformation("Reset");
        // var task = new List<Task>();

        // kirim perintah homing untuk setiap gcode
        foreach (var item in _gcodeMap.Select(x => x.Value.GCode)) {
            item.Reset();
            // task.Add(item.ResetAsync());
        }

        // reset posisi untuk setiap motor agar tidak mengganggu perhitungan kecepatan
        foreach (var item in _motors) {
            item.PositionPercent = 0;
            item.TargetPositionMillimeter = 0;
            item.LastPositionMillimeter = 0;
        }

        // seharusnya tidak terlalu dibutuhkan, hanya untuk memastikan semua perintah reset tidak ada masalah
        // Task.WhenAll(task).Wait();
    }

    /// <summary>
    /// Mengatur posisi motor pada posisi tertentu
    /// </summary>
    /// <param name="index">Nomor motor</param>
    /// <param name="position">Posisi persen</param>
    public void SetPosition(int index, double position) {
        _logger.LogDebug("Motor {Index} set to {Position}", index, position);

        // jika posisi kurang dari 0, maka motor akan menggunakan posisi sebelumnya (tidak bergerak)
        if (position < 0) return;

        // batasi jika posisi lebih dari 100%
        if (position > 100) position = 100;

        var motor = _motors[index];
        motor.PositionPercent = position;

        // dari persen, perlu dikalikan dengann TravelMultiplier untuk mendapat posis dalam milimeter
        var target = position * _options.TravelMultiplier;
        if (target > _options.TravelDistance) target = _options.TravelDistance;
        motor.TargetPositionMillimeter = target;
    }

    /// <summary>
    /// Mengatur warna pada motor
    /// </summary>
    /// <param name="index">Nomor motor</param>
    /// <param name="color">Warna dalam hex atau WebColor</param>
    public void SetColor(int index, string color) {
        _motors[index].Color = color;
    }

    /// <summary>
    /// Menyiapkan perintah dan disimpan dalam buffer sebelum dikirim ke Arduino
    /// </summary>
    public void Prepare() {
        foreach (var item in _gcodeMap) {
            item.Value.GCode.Clear();
        }

        foreach ((string key, var gcode) in _gcodeMap) {
            // GCode command is compiled using StringBuilder instead of string concatenation
            // to reduce garbage of unused strings
            var commandBuilder = new StringBuilder("G1");
            gcode.Append(commandBuilder);

            var speed = gcode.GetSpeed(_duration);
            if (speed > 0) {
                commandBuilder.Append('F').Append(speed);
            }

            foreach (var motor in gcode.Motors) {
                _led.SetColor(motor.Index, motor.Color ?? string.Empty);
            }

            // simpan perntah ke buffer
            var gcodeCommand = commandBuilder.ToString();
            _logger.LogTrace("GCode {Id}: {GCode}", key, gcodeCommand);
            gcode.SetBuffer(gcodeCommand);
        }

        _logger.LogDebug("Command buffer ready");
    }

    /// <summary>
    /// Menjalankan perintah yang sudah disimpan
    /// </summary>
    public void Execute() {
        // kirim perintah yang sudah tersimpan di buffer
        foreach (var gCode in _gcodeMap) {
            gCode.Value.SendBuffer();
        }

        // atur warna LED
        _led.SetColor();

        // update posisi terakhir motor untuk perhitungan kecepatan berikutnya
        foreach (var motor in _motors) {
            motor.LastPositionMillimeter = motor.PositionPercent;
        }

        _logger.LogInformation("Execute movement");
    }

    public void Dispose() {
        foreach (var item in _gcodeMap) {
            item.Value.Dispose();
        }
    }

    public void SetMaxSpeed(int maxSpeed) {
        if (maxSpeed <= 0) return;
        foreach (var item in _gcodeMap) {
            var gcode = item.Value.GCode;
            gcode.Send($"$110={maxSpeed}");
            gcode.Send($"$111={maxSpeed}");
        }
    }

    public void SetStepPerMm(int stepUnit) {
        if (stepUnit <= 0) return;
        foreach (var item in _gcodeMap) {
            var gcode = item.Value.GCode;
            gcode.Send($"$100={stepUnit}");
            gcode.Send($"$101={stepUnit}");
        }
    }

    public void SetMaxAcc(int maxAcc) {
        if (maxAcc <= 0) return;
        foreach (var gcode in _gcodeMap.Select(x => x.Value.GCode)) {
            gcode.Send($"$120={maxAcc}");
            gcode.Send($"$121={maxAcc}");
        }
    }

    public void Stop() {
        _logger.LogInformation("Stop");
        foreach (var gcode in _gcodeMap.Select(x => x.Value.GCode)) {
            gcode.Stop();
        }
    }
}
