// using System.Diagnostics.CodeAnalysis;

// using System.IO.Ports;
// using MasjidBandung.Models;
//
// namespace MasjidBandung.Services;

// public class MotorCommand {
//     // private const int MinimumDuration = 1; // s
//     // private const int TravelDistance = 400; // mm
//     // private const double TravelMultiplier = TravelDistance / 100.0;
//     public int Speed { get; }
//     public static readonly double[] LastPositions = {0, 0, 0, 0, 0, 0, 0, 0};
//     public readonly int[] Speeds = {0, 0, 0, 0,};
//     public int Duration { get; set; }
//
//     // Rentang nilai 0-100
//     public double[] NewPositions { get; } = {0, 0, 0, 0, 0, 0, 0, 0};
//
//     private static int Min(int a, int b) => a < b ? a : b;
//
//     public static void Reset() {
//         for (int i = 0; i < LastPositions.Length; i++) {
//             LastPositions[i] = 0;
//         }
//     }
//
//     private static int RootSquare(double x, double y) {
//         return (int) Math.Sqrt(x * x + y * y);
//     }
//
//     public static MotorCommand WithDuration(double[] positions, int duration) {
//         var cmd = new MotorCommand {
//             Duration = duration
//         };
//         var length = Min(positions.Length, LastPositions.Length);
//         // var deltaPosition = new int[length];
//         var speeds = new[] {0, 0, 0, 0, 0, 0, 0, 0}; //new int[4]};
//         for (int i = 0; i < length; i++) {
//             double target;
//             // hitung perubahan posisi
//             if (positions[i] < 0) {
//                 // jika negatif, pakai posisi sebelumnya (diam)
//                 target = LastPositions[i];
//             } else {
//                 target = positions[i] * MotorSettings.TravelMultiplier;
//             }
//
//             // seharusnya target akan selali >=0, jadi tidak perlu cek jika target < 0
//             if (target > MotorSettings.TravelDistance) target = MotorSettings.TravelDistance;
//
//             var delta = Math.Abs(target - LastPositions[i]);
//             // hitung kecepatan (mm/min) dari delta (mm) dan durasi (s)
//             if (duration < MotorSettings.MinimumDuration) duration = MotorSettings.MinimumDuration;
//             var speed = (int) (60.0 * delta / duration);
//             speeds[i] = speed switch {
//                 > 15000 => 15000,
//                 < 0 => 0,
//                 _ => speed
//             };
//
//             // simpan posisi terbaru
//             cmd.NewPositions[i] = target; //kenapa variabel berbeda bisa disatukan
//             // Positions[i] = positions[i];
//         }
//
//         cmd.Speeds[0] = RootSquare(speeds[0], speeds[1]);
//         cmd.Speeds[1] = RootSquare(speeds[2], speeds[3]);
//         cmd.Speeds[2] = RootSquare(speeds[4], speeds[5]);
//         cmd.Speeds[3] = RootSquare(speeds[6], speeds[7]);
//
//         // foreach (double position in positions) {
//         //     cmd.NewPositions.Add(position * TravelMultiplier);
//         // }
//
//         return cmd;
//     }
//
//     public MotorCommand(params double[] positions) : this(0, positions) { }
//
//     public MotorCommand(int speed, params double[] positions) {
//         Speed = speed;
//         Duration = 0;
//         for (int i = 0; i < Speeds.Length; i++) {
//             Speeds[i] = speed;
//         }
//
//         var length = Min(positions.Length, NewPositions.Length);
//         for (int i = 0; i < length; i++) {
//             var pos = positions[i] * MotorSettings.TravelMultiplier;
//             if (pos < 0) pos = -1;
//             if (pos > MotorSettings.TravelDistance) pos = MotorSettings.TravelMultiplier;
//             NewPositions[i] = pos;
//         }
//         // foreach (var item in positions) {
//         //     NewPositions.Add(item);
//         // }
//     }
// }

// public interface IMotorService {
//     void SetStepPerMm(int stepUnit);
//     void SetTravelDistance(int travelDistance);
//     void SetMaxSpeed(int maxSpeed);
//     void SetMaxAcc(int maxAcc);
//     int Count { get; }
//     void NewCommand(MotorCommand command);
//     int GetCommandCount();
//     void Move(MotorCommand command);
//     bool Move(int index);
//     bool Next();
//     bool First();
//     void Clear();
//     void Stop();
//     Task<Dictionary<int, MachineState>> Reset();
//     Task Unlock();
//     List<double> GetPosition();
//     List<MachineState> GetState();
//     List<Dictionary<int, string>> GetSettings();
//     List<string?> CheckId();
// }
//
// // [SuppressMessage("ReSharper", "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
// public class MotorService : IMotorService, IDisposable {
//     /*
//      * Contoh kasus
//      *  Motor 1, di serial 0 ch X
//      *  Motor 2, di serial 0 ch Y
//      *  Motor 3, di serial 1 ch X
//      *  Motor 4, di serial 1 ch Y
//      * channel X dan Y sudah pasti, yang mungkin tertukar adalah serial 0 dan 1
//      */
//     // private static int _travelDistance = 400; // mm
//
//     // private static double TravelMultiplier = TravelDistance / 100.0;
//     public int Count { get; }
//     private int _cmdCounter;
//     private readonly List<GCode> _gcode = new();
//     private readonly Dictionary<int, int> _gcodeMapping = new();
//     private readonly Dictionary<int, char> _channelMapping;
//
//     private readonly List<MotorCommand> _commandList = new();
//
//     public void SetTravelDistance(int distance) {
//         // _travelDistance = distance;
//         MotorSettings.TravelDistance = distance;
//         // TravelMultiplier = distance / 100.0;
//     }
//
//     public void SetMaxSpeed(int maxSpeed) {
//         if (maxSpeed <= 0) return;
//         foreach (var gcode in _gcode) {
//             gcode.Send($"$110={maxSpeed}");
//             gcode.Send($"$111={maxSpeed}");
//         }
//     }
//
//     public void SetStepPerMm(int stepUnit) {
//         if (stepUnit <= 0) return;
//         foreach (var gcode in _gcode) {
//             gcode.Send($"$100={stepUnit}");
//             gcode.Send($"$101={stepUnit}");
//         }
//     }
//
//     public void SetMaxAcc(int maxAcc) {
//         if (maxAcc <= 0) return;
//         foreach (var gcode in _gcode) {
//             gcode.Send($"$120={maxAcc}");
//             gcode.Send($"$121={maxAcc}");
//         }
//     }
//     // private int[] _position = {0, 0, 0, 0};
//     // private ILedService _led = new PyLedService();
//
//     public MotorService(Dictionary<int, char> channelMapping, IEnumerable<SerialPort> ports) {
//         Console.WriteLine("Initialize MotorService");
//         _channelMapping = channelMapping;
//         Count = channelMapping.Count;
//         // tambahkan daftar serial ke list
//         var portCount = 0;
//         foreach (var item in ports) {
//             Console.WriteLine($"Using port {item.PortName}");
//             _gcode.Add(new GCode(item));
//             portCount++;
//         }
//
//         // baca ID setiap port, kemudian masukkan ke _gcodeMapping
//         // (belum implementasi)
//         _gcodeMapping.Add(0, 0); // implementasi sementara
//         _gcodeMapping.Add(1, 0);
//         if (portCount > 1) {
//             _gcodeMapping.Add(2, 1);
//             _gcodeMapping.Add(3, 1);
//             if (portCount > 2) {
//                 _gcodeMapping.Add(4, 2);
//                 _gcodeMapping.Add(5, 2);
//                 if (portCount > 3) {
//                     _gcodeMapping.Add(6, 3);
//                     _gcodeMapping.Add(7, 3);
//                 }
//             }
//         }
//         // _ = DelayedInit();
//     }
//
//     public List<string?> CheckId() {
//         for (int i = 0; i < _gcode.Count; i++) {
//             var id = _gcode[i].Id;
//             switch (id) {
//                 case "1000.000":
//                     _gcodeMapping[0] = i;
//                     _gcodeMapping[1] = i;
//                     break;
//                 case "2000.000" when Count > 2:
//                     _gcodeMapping[2] = i;
//                     _gcodeMapping[3] = i;
//                     break;
//                 case "3000.000" when Count > 4:
//                     _gcodeMapping[4] = i;
//                     _gcodeMapping[5] = i;
//                     break;
//                 case "4000.000" when Count > 6:
//                     _gcodeMapping[6] = i;
//                     _gcodeMapping[7] = i;
//                     break;
//             }
//
//             Console.WriteLine(id);
//         }
//         // var id = _gcode[i].Id;
//         // if (id == "3000.000") {
//         //     // 1000 -> 0, 2000 -> 1, 3000 -> 2
//         //     _gcodeMapping[i] = 0;
//         // }
//
//         return _gcode.Select(e => e.Id).ToList();
//     }
//
//     public async Task DelayedInit() {
//         await Task.Delay(5000);
//         foreach (var item in _gcode) {
//             item.GetSettings();
//         }
//
//         for (int i = 0; i < 20; i++) {
//             var set = !_gcode.Any(item => item.Id is null);
//             if (set) break;
//             await Task.Delay(1000);
//         }
//     }
//
//     public void NewCommand(MotorCommand command) {
//         _commandList.Add(command);
//     }
//
//     /// <summary>
//     /// Mengosongkan antrean
//     /// </summary>
//     public void Clear() {
//         _commandList.Clear();
//         _cmdCounter = 0;
//     }
//
//     public int GetCommandCount() {
//         return _commandList.Count;
//     }
//
//     public bool Move(int index) {
//         if (index >= _commandList.Count) return false;
//         var cmd = _commandList[index];
//         Move(cmd);
//         return true;
//     }
//
//     /// <summary>
//     /// Menjalankan perintah berikutnya
//     /// </summary>
//     public bool Next() {
//         // kirim perintah gcode ke motor
//         if (_commandList.Count == 0) return false;
//         if (_cmdCounter >= _commandList.Count) _cmdCounter = 0; // seharusnya tidak perlu cek lagi
//
//         var cmd = _commandList[_cmdCounter];
//         if (++_cmdCounter == _commandList.Count) _cmdCounter = 0;
//         Move(cmd);
//         return true;
//     }
//
//     public void Move(MotorCommand cmd) {
//         // berisi gcode yang akan dikim berikutnya
//         var nextCmd = new List<string>();
//         for (int i = 0; i < _gcode.Count; i++) {
//             nextCmd.Add("G1");
//         }
//
//         // iterasi ambil posisi target setiap motor dan masukkan ke gcode
//         // _motorCount dengan cmd.MotorPosition.Count seharusnya sama
//         for (int i = 0; i < Count; i++) {
//             if (!_gcodeMapping.ContainsKey(i)) continue;
//
//             // target posisi berikutnya
//             var nextPos = cmd.NewPositions[i]; // * TravelMultiplier;
//
//             // jika posisi yang di kirim -1, motor tetap diam
//             if (nextPos < 0) continue;
//             // if (nextPos > TravelDistance) nextPos = TravelDistance;
//
//             char motorCh = _channelMapping[i];
//             int gcodeCh = _gcodeMapping[i];
//
//             // append ke command string
//             nextCmd[gcodeCh] += motorCh + nextPos.ToString("F3");
//
//             MotorCommand.LastPositions[i] = nextPos;
//         }
//
//         // kirim perintah kecepatan jika ada
//         if (cmd.Duration > 0) {
//             for (int i = 0; i < _gcode.Count; i++) {
//                 var speed = cmd.Speeds[i];
//                 if (speed == 0) continue;
//                 var g = _gcodeMapping[i * 2];
//                 _gcode[g].SetSpeed(speed);
//             }
//         } else if (cmd.Speed > 0) {
//             foreach (var item in _gcode) {
//                 item.SetSpeed(cmd.Speed);
//             }
//         }
//
//         // kirim perintah ke motor
//         for (int i = 0; i < _gcode.Count; i++) {
// #if DEBUG
//             Console.WriteLine(nextCmd[i]);
// #endif
//             _gcode[i].Send(nextCmd[i]);
//         }
//     }
//
//     public bool First() {
//         _cmdCounter = 0;
//         return Next();
//     }
//
//     public List<double> GetPosition() {
//         var pos = new List<double>();
//         for (int i = 0; i < Count; i++) {
//             if (!_gcodeMapping.ContainsKey(i)) continue;
//             var gcodeId = _gcodeMapping[i];
//             var ch = _channelMapping[i];
//             var item = _gcode[gcodeId];
//             switch (ch) {
//                 case 'X':
//                     pos.Add(100.0 * item.CurrentPosition.X / MotorSettings.TravelDistance);
//                     break;
//                 case 'Y':
//                     pos.Add(100.0 * item.CurrentPosition.Y / MotorSettings.TravelDistance);
//                     break;
//                 default:
//                     // invalid motor channel, seharusnya tidak mungkin terjadi
//                     pos.Add(-1.0);
//                     break;
//             }
//         }
//
//         return pos;
//     }
//
//     public List<MachineState> GetState() {
//         var state = new List<MachineState>();
//         for (int i = 0; i < Count; i++) {
//             var gcodeId = _gcodeMapping[i];
//             state.Add(_gcode[gcodeId].State);
//         }
//
//         return state;
//     }
//
//     public List<Dictionary<int, string>> GetSettings() {
//         var list = new List<Dictionary<int, string>>();
//
//         foreach (var item in _gcode) {
//             list.Add(item.Settings);
//         }
//
//         return list;
//     }
//
//     public async Task<Dictionary<int, MachineState>> Reset() {
//         var task = new List<Task<MachineState>>();
//         foreach (var item in _gcode) {
//             task.Add(item.Reset());
//         }
//
//         var states = await Task.WhenAll(task);
//         var result = new Dictionary<int, MachineState>();
//         for (int i = 0; i < Count; i++) {
//             if (!_gcodeMapping.ContainsKey(i)) continue;
//             var gcodeId = _gcodeMapping[i];
//             result[i] = states[gcodeId];
//         }
//
//         return result;
//     }
//
//     public void Stop() {
//         foreach (var item in _gcode) {
//             item.Write("`");
//         }
//     }
//
//     public Task Unlock() {
//         var tasks = new List<Task>();
//         foreach (var item in _gcode) {
//             tasks.Add(item.Unlock());
//         }
//
//         return Task.WhenAll(tasks);
//     }
//
//     public void Dispose() {
//         foreach (var item in _gcode) {
//             item.Dispose();
//         }
//
//         GC.SuppressFinalize(this);
//     }
// }
