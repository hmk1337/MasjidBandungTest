using System.IO.Ports;
using System.Globalization;
using System.Text.Json.Serialization;

namespace MasjidBandung.Services;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MachineState {
    Idle,
    Busy,
    Error,
    Other
}

// ReSharper disable once InconsistentNaming
public class PointXY {
    public double X { get; set; }
    public double Y { get; set; }
}

public class GCode : IDisposable {
    private readonly CultureInfo _cultureInfo = new("en-US");
    public string? Id { get; }
    public bool IsBusy;
    public readonly bool[] PinState = {false, false, false};
    public MachineState State;

    public readonly PointXY CurrentPosition = new();

    //public PointXY NextPosition = new();
    // public int AvailableBuffer;
    // private bool _isPaused;
    // public readonly Dictionary<int, string> Settings = new();
    private readonly SerialPort _serial;

    private bool _autoHome = true;
    private string _error = string.Empty;

    // private string _message = string.Empty;
    private double _xAxisConstant = 1.0;
    private double _yAxisConstant = 1.0;

    private readonly Queue<string> _queue = new();

    // private float _kx = 1.0f;
    // private float _ky = 1.0f;
    // private float _kz = 0.0f;
    // private float _offsetX = -1.0f;
    // private float _offsetY = -1.0f;
    private Action? _onFinishedAction;
    private Action? _onIdleBeginAction;
    private string _lastCommand = string.Empty;
    private int _homingRetryCount;

    private readonly CancellationToken _cancellationToken;
    private readonly CancellationTokenSource _tokenSource;
    private readonly Task _routineTask;

    private readonly object _lock = new();
    // private readonly bool _lockSerial;

    // private string _commandBuffer = string.Empty;

    public GCode(string portName) {
        _tokenSource = new CancellationTokenSource();
        _cancellationToken = _tokenSource.Token;
        // cultureInfo = new CultureInfo("en-US");
        //CurrentPosition = new PointPolar();
        State = MachineState.Other;
        IsBusy = false;

        _serial = new SerialPort();
        _serial.PortName = portName;
        _serial.BaudRate = 115200;
        _serial.DataReceived += SerialDataHandler;

        try {
            Console.WriteLine("Opening serial port at: {0}", _serial.PortName);
            _serial.Open();
            Console.WriteLine("Serial port opened");
        } catch (Exception e) {
            _error = e.Message;
            Console.WriteLine("Open serial port error: {0}", e.Message);
        }

        _routineTask = GetStatusRoutine();
    }

    public GCode(SerialPort port) {
        _tokenSource = new CancellationTokenSource();
        _cancellationToken = _tokenSource.Token;
        _serial = port;
        _serial.DataReceived += SerialDataHandler;
        Console.WriteLine($"Serial port {port.PortName} opened");
        if (_serial.IsOpen) {
            _serial.WriteLine("`");
            _routineTask = GetStatusRoutine();
            return;
        }

        Id = port.PortName;

        _serial.BaudRate = 115200;
        _serial.Open();
        _routineTask = GetStatusRoutine();
    }

    public void OnIdleBegin(Action action) {
        _onIdleBeginAction = action;
    }

    public void OnFinished(Action action) {
        _onFinishedAction = action;
    }

    // public void Init() {
    //     _serial = new SerialPort();
    //     _serial.PortName = OperatingSystem.IsWindows() ? "COM6" : "/dev/ttyUSB0";
    //     _serial.BaudRate = 115200;
    //     _serial.DataReceived += new SerialDataReceivedEventHandler(SerialDataHandler);
    //
    //     try {
    //         Console.WriteLine("Opening serial port at: " + _serial.PortName);
    //         _serial.Open();
    //         Console.WriteLine("Serial port opened");
    //     } catch (Exception e) {
    //         _error = e.Message;
    //         Console.WriteLine("Open serial port error: " + e.Message);
    //     }
    // }

    public void GetSettings() {
        Send("$$");
        // try {
        //     _serial.WriteLine("$$");
        // } catch (Exception ex) {
        //     _error = ex.Message;
        // }
    }

    public void SetSetting(int key, string value) {
        Send($"${key}={value}");
    }

    public void GetStatus() {
        try {
            _serial.WriteLine("?");
        } catch (Exception ex) {
            _error = ex.Message;
        }
    }

    private async Task GetStatusRoutine() {
        await Task.Delay(3000, _cancellationToken);
        var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500)); //1
        while (!_cancellationToken.IsCancellationRequested) {
            // await Task.Delay(1500, _cancellationToken);
            await timer.WaitForNextTickAsync(_cancellationToken);
            _serial.WriteLine("?");
        }
        // ReSharper disable once FunctionNeverReturns
    }

    private void ParseStatus(string message) {
        if (message.Length < 5) return;
        var reportedParams = message.Split('|');
        var state = reportedParams[0]; //.ToLower(); // sudah ToLower di sebelum masuk fungsi
        // Console.WriteLine(state);
        switch (state) {
            case "Idle":
                // if (IsBusy) {
                // Console.WriteLine($"{Id} - State: {IsBusy} {State}");
                IsBusy = false;
                if (State == MachineState.Busy && _onIdleBeginAction != null) {
                    // di sini saat baru transisi menuju idle
                    _onIdleBeginAction();
                    // break;
                }

                // }
                // Console.WriteLine("Idle " + IsBusy + MachineState);
                State = MachineState.Idle;
                // IsBusy = false;
                break;
            case "Run":
                State = MachineState.Busy;
                // Console.WriteLine("Run");
                break;
            case "Alarm":
                State = MachineState.Error;
                break;
            default:
                State = MachineState.Other;
                break;
        }

        if (reportedParams.Length == 1) return;
        for (int i = 1, n = reportedParams.Length; i < n; i++) {
            var currentParam = reportedParams[i].Split(':');
            if (currentParam.Length == 1) return;
            switch (currentParam[0]) {
                case "WPos":
                    var position = currentParam[1].Split(',');
                    if (double.TryParse(position[0], NumberStyles.Any, _cultureInfo, out var x)) {
                        CurrentPosition.X = x;
                    }

                    if (double.TryParse(position[1], NumberStyles.Any, _cultureInfo, out var y)) {
                        CurrentPosition.Y = y;
                    }

                    //CurrentPosition.Set(radius, theta);
                    // CurrentPosition.Z = float.Parse(position[2], Config.FormatProvider);
                    // Console.WriteLine("Position: " + CurrentPosition.ToString());
                    break;
                case "Bf":
                    // var buffer = currentParam[1].Split(',');
                    // Console.WriteLine("Available buffer: " + buffer[0]);
                    break;
                case "Pn":
                    var pinState = currentParam[1];
                    ReadPinState(pinState, PinState);
                    // Console.WriteLine("Pin state: " + pinState);
                    // Span<bool> currentState = stackalloc bool[3];
                    // // var currentState = new[] {false, false, false};
                    // for (int j = 0, o = pinState.Length; j < o; j++) {
                    //     // Console.WriteLine(pinState[j] == 'x');
                    //     switch (pinState[j]) {
                    //         case 'x':
                    //             currentState[0] = true;
                    //             break;
                    //         case 'y':
                    //             currentState[1] = true;
                    //             break;
                    //         case 'z':
                    //             currentState[2] = true;
                    //             break;
                    //     }
                    // }
                    //
                    // PinState = currentState;
                    // Console.WriteLine(PinState[0] + " " + PinState[1] + " " + PinState[2]);
                    break;
                case "FS":
                    break;
            }
        }
    }

    private static void ReadPinState(ReadOnlySpan<char> pinState, Span<bool> output) {
        Span<bool> currentState = stackalloc bool[3];
        currentState.Fill(false);
        for (int j = 0, o = pinState.Length; j < o; j++) {
            // Console.WriteLine(pinState[j] == 'x');
            switch (pinState[j]) {
                case 'x':
                    currentState[0] = true;
                    break;
                case 'y':
                    currentState[1] = true;
                    break;
                case 'z':
                    currentState[2] = true;
                    break;
                case 'X':
                    currentState[0] = true;
                    break;
                case 'Y':
                    currentState[1] = true;
                    break;
                case 'Z':
                    currentState[2] = true;
                    break;
            }
        }

        currentState.CopyTo(output);
    }

    public void Clear() {
        _queue.Clear();
    }

    public void SetHome() {
        Send("G92X0Y0");
    }

    public void Write(string command) {
        try {
            _serial.WriteLine(command);
            _lastCommand = command;
            Thread.Sleep(1);
            // await Task.Delay(5, _cancellationToken);
            Console.WriteLine($"{Id} - Sent: {command}");
        } catch (Exception ex) {
            Console.WriteLine($"{Id} Failed to send command: {ex.Message}");
            _error = ex.Message;
        }
    }

    public void KickStart() {
        _error = string.Empty;
        Write("$X");
    }

    public async Task UnlockGrbl() {
        _error = string.Empty;
        _queue.Clear();
        IsBusy = false;
        // while (queue.Count > 0) {
        //     Console.WriteLine(queue.Dequeue());
        // };
        Send("$X");
        await Task.Delay(500, _cancellationToken);
        Send("G90");
        Send("G92X0Y0Z0");
    }

    public void Dump() {
        // queue.Clear();

        foreach (var _ in _queue) {
            // var line = queue.Peek();
            // if (line.StartsWith("G1"))

            _queue.Dequeue();
        }
    }

    public int GetProgress() {
        return _queue.Count;
    }

    public void Reset() {
        _autoHome = true;
        _homingRetryCount = 0;
        Console.WriteLine("Reset...");
        _queue.Clear();
        Write("`\r\n");
        IsBusy = false;
        State = MachineState.Busy;
    }

     public void Stop() {
        _autoHome = false;
        Write("`\r\n");
       // Send("$X");
       // Send("G90");
       // Send("G92X0Y0");
        _queue.Clear();
    }

    // public async Task<MachineState> ResetAsync() {
    //     _homingRetryCount = 0;
    //     Console.WriteLine("Reset...");
    //     // _error = null;
    //     // while (_queue.Count > 0) {
    //     //     Console.WriteLine(_queue.Dequeue());
    //     // }
    //     _queue.Clear();
    //
    //     await Task.Yield();
    //     Write("`\n");
    //     IsBusy = false;
    //     // await Task.Delay(2000);
    //     // IsBusy = true;
    //     // // Send("$H");
    //     // // Write("$X");
    //     // // GetStatus();
    //     // // await Task.Delay(100);
    //     State = MachineState.Busy;
    //     // // Console.WriteLine("State " + MachineState);
    //     // // var timeout = 100;
    //     // var timeStart = DateTime.Now;
    //     // while (State == MachineState.Busy && DateTime.Now - timeStart < TimeSpan.FromSeconds(20)) {
    //     //     // await Task.Delay(10);
    //     //     await Task.Yield();
    //     //     // timeout--;
    //     // }
    //     //
    //     // // Thread.Sleep(1000);
    //     // // Send("G91");
    //     // // Move(_offsetX, _offsetY, null, 60);
    //     // // Send("G1X-1Y-1");
    //     // Send("G90");
    //     // Send("G92X0Y0Z0");
    //     return State;
    // }

    public void ResetError() {
        _error = string.Empty;
    }

    public void Send(string command) {
        // _error = null;
        if (IsBusy) {
            // if (queue.IsFull()) _error = "Controller not ready";
            // else
            _queue.Enqueue(command);
            // Console.WriteLine($"{Id} - Saved to queue: {command}");
            return;
        }

        IsBusy = true;
        // try {
        Write(command);
// #if DEBUG
//             Console.WriteLine($"{Id} - Sent: {command}");
// #endif
        // } catch (Exception ex) {
        //     Console.WriteLine($" {Id} - Failed to send command: {ex.Message}");
        //     _error = ex.Message;
        // }
    }

    private void SerialDataHandler(object sender, SerialDataReceivedEventArgs e) {
        CheckResponse(); //.GetAwaiter().GetResult();
    }

    private void CheckResponse() {
        // await Task.Delay(5);
        while (_serial.BytesToRead > 0) {
            try {
                string line = _serial.ReadLine();

#if DEBUG
                // Console.WriteLine($"{Id} -> {line}");
#endif
                // var receivedLines = response.Split('\r');
                // foreach (var line in receivedLines) {
                string msg = line.Trim();
                if (msg == "ok") {
                    if (_queue.Count > 0) {
                        // sudah dicek IsBusy = false, bisa langsung kirim
                        // IsBusy tetap true
                        Write(_queue.Dequeue());
                    } else if (_onFinishedAction != null && IsBusy) {
                        _onFinishedAction();
                        IsBusy = false;
                    }
                } else if (msg.StartsWith('<') && msg.EndsWith('>')) {
                    ParseStatus(msg.Substring(1, msg.Length - 2));
                } else if (msg.StartsWith("error") || msg.StartsWith("alarm")) {
                    Console.WriteLine($"{Id} - Error {msg}");
                    switch (msg) {
                        case "error:9":
                            Console.WriteLine($"{Id} G-Code Lock: G-code commands are locked out during alarm or jog state.");
                            if (_homingRetryCount > 20) break;
                            Send("$H");
                            _homingRetryCount++;
                            break;
                        case "error:2":
                        case "error:24":
                            Write(_lastCommand);
                            Console.WriteLine("Repeat " + _lastCommand);
                            break;
                        case "error:11":
                            Thread.Sleep(10);
                            // await Task.Delay(10, _cancellationToken);
                            Write(_lastCommand);
                            Console.WriteLine("Repeat " + _lastCommand);
                            break;
                        default:
                            _error = msg;
                            // IsBusy = false;
                            break;
                    }
                    // } else if (msg.StartsWith("$")) {
                    //     var configLine = line.Split('=');
                    //     var configKey = int.Parse(configLine[0].Trim()[1..]);
                    //     var configValue = configLine[1].Trim();
                    //     // Console.WriteLine(configKey + ": " + configValue);
                    //     if (configKey == 112) Id = configValue;
                    //     Settings.TryAdd(configKey, configValue);
                    //     // try {
                    //     //     Settings[configKey] = configValue;
                    //     // } catch (Exception e) {
                    //     //     Console.WriteLine("GRBL config load failed {0}", e);
                    //     // }
                } else if (msg.Contains("$H") || msg.Contains("'$'")) {
                    // if (string.IsNullOrEmpty(Id) || Settings.Count == 0) {
                    //     GetSettings();
                    //     await Task.Delay(100, _cancellationToken);
                    // }

                    IsBusy = false;
                    _queue.Clear();
                     if (_autoHome) {
                        Send("$H");
                        Thread.Sleep(100);
                        // await Task.Delay(100, _cancellationToken);
                        Send("G90");
                        Send("G92X0Y0");
                        Send("G1F1000");
                        _homingRetryCount++;
                    }
                    
                } else {
                    // _message = line;
                    Console.WriteLine($"{Id} - Other {line}");
                }
                // }
            } catch (Exception e) {
                Console.WriteLine("Response check error: {0}", e);
            }

            Thread.Sleep(1);
            // await Task.Delay(5, _cancellationToken);
        }
    }

    public void SetStepUnit(double x, double y) {
        Send("$100=" + x.ToString("F3", _cultureInfo));
        Send("$101=" + y.ToString("F3", _cultureInfo));
    }

    public void SetYAxisConstant(double value) {
        _yAxisConstant = value;
    }

    public void SetXAxisConstant(double value) {
        _xAxisConstant = value;
    }

    public void SetPositionX(double x) {
        Send($"G92X{x.ToString("F3", _cultureInfo)}");
    }

    public string GetPinState() {
        // Console.WriteLine($"Pin {PinState[0]} {PinState[1]} {PinState[2]}");
        char x = PinState[0] ? '1' : '0';
        char y = PinState[1] ? '1' : '0';
        char z = PinState[2] ? '1' : '0';
        return $"{x}{y}{z}";
    }

    // ReSharper disable once InconsistentNaming
    public void MoveXY(double x, double y, int f = 0) {
        var xStr = (_xAxisConstant * x).ToString("F3", _cultureInfo);
        var yStr = (_yAxisConstant * y).ToString("F3", _cultureInfo);
        var cmd = $"G1 X{xStr} Y{yStr}{_feedrate(f)}";
        // Console.WriteLine(cmd);
        Send(cmd);
    }

    // public void MoveXY(float x, float y, Int32 f = 0) {
    //     var cmd = $"G1{_x(x)}{_y(y)}{_feedrate(f)}";
    //     Console.WriteLine(cmd);
    //     Send(cmd);
    // }
    public void SetSpeed(int f) {
        var cmd = $"G1{_feedrate(f)}";
        Console.WriteLine($"{Id} set speed {cmd}");
        Send(cmd);
    }

    public void Move(double? x, double? y, int f = 0) {
        // var cmd = $"G1{_x(_kx * x + _offsetX)}{_y(_ky * y + _offsetY)}{_z(z + _kz)}{_feedrate(f)}";
        var cmd = $"G1 {_x(x)} {_y(y)} {_feedrate(f)}";

        Console.WriteLine(cmd);
        // _serial.WriteLine(cmd);
        Send(cmd);
    }

    public string GetError() {
        string errorMsg = _error;
        // _error = null;
        return errorMsg;
    }
    // public void SetCalibration(float kx, float ky, float z) {
    //     _kx = kx;
    //     _ky = ky;
    //     _kz = z;
    // }
    // public void SetOffset(float x, float y) {
    //     _offsetX = x;
    //     _offsetY = y;
    // }

    private string _x(double? val) {
        return val == null ? "" : "X" + val.Value.ToString("F3", _cultureInfo);
    }

    private string _y(double? val) {
        return val == null ? "" : "Y" + val.Value.ToString("F3", _cultureInfo);
    }

    // private string _z(double? val) {
    //     return val == null ? "" : "Z" + val.Value.ToString("F3", _cultureInfo);
    // }

    private static string _feedrate(int f) {
        return f == 0 ? "" : "F" + f.ToString("D");
    }

    // public void Pause() {
    //     _isPaused = true;
    //     // _serial.WriteLine("!");
    // }
    //
    // public void Continue() {
    //     // _serial.WriteLine("~");
    //     _isPaused = false;
    //     if (!IsBusy && _queue.Count > 0) {
    //         Write(_queue.Dequeue());
    //     }
    // }

    ~GCode() {
        Dispose(false);
    }

    // private void ReleaseUnmanagedResources() {
    //     // TODO release unmanaged resources here
    // }

    private void Dispose(bool disposing) {
        // ReleaseUnmanagedResources();
        _tokenSource.Cancel();

        if (!disposing) return;
        _serial.Dispose();
        _tokenSource.Dispose();
        _routineTask.Dispose();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    // public void SetBuffer(string command) {
    //     _commandBuffer = command;
    // }
    // public void SendBuffer() {
    //     Send(_commandBuffer);
    // }

    private bool _lockFlag;

    private bool IsLocked() {
        lock (_lock) {
            return _lockFlag;
        }
    }

    private bool LockWhenUnlocked() {
        lock (_lock) {
            if (_lockFlag) return false;
            _lockFlag = true;
        }

        return true;
    }

    private void Lock() {
        lock (_lock) {
            _lockFlag = true;
        }
    }

    private void Unlock() {
        lock (_lock) {
            _lockFlag = false;
        }
    }
}
