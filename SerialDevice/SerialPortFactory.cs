// using System.IO.Ports;
//
// namespace MasjidBandung.SerialDevice;
//
// public enum DeviceType {
//     GCode1,
//     GCode2,
//     GCode3,
//     GCode4
// }
//
// public class SerialPortFactory : IDisposable {
//     private readonly List<SerialPort> _serialPorts;
//
//     private readonly Dictionary<int, SerialPort> _devices = new();
//     // private readonly Task _delayedInit;
//
//     public SerialPortFactory() {
//         _serialPorts = SerialPort.GetPortNames().Where(port => port.Contains("ttyUSB")) // || port.Contains("ttyACM"))
//             .Select(port => new SerialPort(port, 115200)).ToList();
//         foreach (var port in _serialPorts) {
//             Console.WriteLine($"Detected {port.PortName}");
//             port.DataReceived += SerialEventHandler;
//             port.DtrEnable = true;
//             if (!port.IsOpen) port.Open();
//             // port.WriteLine("$$");
//         }
//
//         _ = DelayedInit();
//     }
//
//     private async Task DelayedInit() {
//         await Task.Delay(100);
//         foreach (var port in _serialPorts) {
//             port.WriteLine("`");
//             // port.WriteLine("$$");
//         }
//     }
//
//     private void SerialEventHandler(object sender, SerialDataReceivedEventArgs args) {
//         Thread.Sleep(10);
//         var port = (SerialPort) sender;
//         if (_devices.ContainsValue(port)) return;
//         while (port.BytesToRead > 0) {
//             var line = port.ReadLine();
//             if (!line.StartsWith('$')) {
//                 port.WriteLine("$$");
//                 continue;
//             }
//
//             var keyValue = line.Split('=');
//             switch (keyValue.Length) {
//                 case 1:
//                     if (line.ToLower().Contains("$ktp")) Console.WriteLine("KTP");
//                     // _devices[DeviceType.Support] = port;
//                     port.DataReceived -= SerialEventHandler;
//                     break;
//                 case 2: {
//                     var key = keyValue[0];
//                     var value = keyValue[1].Trim();
//                     if (key != "$112") break;
//                     Console.WriteLine($"Found {value}");
//                     var id = value switch {"1000.000" => 0, "2000.000" => 1, "3000.000" => 2, "4000.000" => 3, _ => -1};
//                     if (!_devices.ContainsKey(id)) _devices.Add(id, port);
//                     port.DataReceived -= SerialEventHandler;
//                     return;
//                 }
//             }
//         }
//     }
//
//     public async Task WaitAllDevicesAsync(int count = 4, CancellationToken cancellationToken = default) {
//         var devicesCount = 0;
//         var timeout = DateTime.Now.AddSeconds(10);
//         while (!HasAllDevices(count) && !cancellationToken.IsCancellationRequested && DateTime.Now < timeout) {
//             if (devicesCount != _devices.Count) Console.WriteLine(_devices.Count);
//             else Console.Write('.');
//             devicesCount = _devices.Count;
//             await Task.Delay(100, cancellationToken);
//         }
//
//         // foreach (var port in _serialPorts.Where(p => !_devices.ContainsValue(p))) {
//         //     Console.WriteLine($"Close unused port {port.PortName}");
//         //     port.Close();
//         //     port.Dispose();
//         // }
//
//         Console.WriteLine();
//         Console.WriteLine(HasAllDevices(count) ? "Device scan completed" : "Timeout");
//     }
//
//     public bool HasAllDevices(int count = 4) {
//         for (int i = 0; i < count; i++) {
//             if (!_devices.ContainsKey(i)) return false;
//         }
//
//         return true; // _devices.ContainsKey(DeviceType.GCode1) && _devices.ContainsKey(DeviceType.GCode2) && _devices.ContainsKey(DeviceType.Support);
//     }
//
//     public SerialPort? GetDevice(int device) {
//         return _devices.ContainsKey(device) ? _devices[device] : null;
//     }
//
//     public IEnumerable<SerialPort> GetDevices() {
//         return _devices.OrderBy(x => x.Key).Select(x => x.Value);
//     }
//
//     // ~SerialPortFactory() {
//     //     // _delayedInit.Dispose();
//     // }
//     public void Dispose() {
//         foreach (var port in _devices) {
//             // port.Value.Close();
//             port.Value.Dispose();
//         }
//     }
// }
