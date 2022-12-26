// using System.Device.Pwm;

using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
using rpi_ws281x;
using Controller = rpi_ws281x.Controller;

namespace MasjidBandung.Services;

public interface ILedService {
    Task SetColorAsync(string[] color);
    void SetColorSegment(string ledColor, int start, int count);
    Task SetLedEnvirontment(double value);
    void SetColor(string[] color);
    Task SetColorAsync();
    void SetColor();
}

public class LedService : ILedService, IDisposable {
    // private PwmChannel _pwm;
    private static string[] _ledColors = {"#000000", "#000000", "#000000", "#000000"};

    public LedService() {
        // _pwm = PwmChannel.Create(0, 1, 400, 0.8);
        // _pwm.Start();
        // var t = new Thread(() => _pwm.Start());
        // t.Start();
    }

    public void SetColor(string[] color) => _ledColors = color;

    public void SetColor() {
        var length = _ledColors.Length;
        for (int i = 0; i < length; i++) {
            SetColorSegment(_ledColors[i], i * 24, 24);
        }
    }

    public async Task SetColorAsync() => await SetColorAsync(_ledColors);

    public async Task SetColorAsync(string[] color) {
        if (color.Length < 4) return;
        using var process = new Process();
        process.StartInfo.FileName = "/usr/bin/python";
        process.StartInfo.ArgumentList.Add("/home/pi/led/main.py");
        process.StartInfo.ArgumentList.Add(color[0]);
        process.StartInfo.ArgumentList.Add(color[1]);
        process.StartInfo.ArgumentList.Add(color[2]);
        process.StartInfo.ArgumentList.Add(color[3]);
        process.StartInfo.ArgumentList.Add(color.Length > 4 ? color[0] : "FFFFFF");
        process.StartInfo.ArgumentList.Add("1");
        process.Start();
        await process.WaitForExitAsync();
    }

    private static readonly Settings LedSettings = rpi_ws281x.Settings.CreateDefaultSettings();

    private static readonly Controller LedController = LedSettings.AddController(
        ledCount: 96,
        pin: Pin.Gpio18,
        stripType: StripType.WS2812_STRIP,
        controllerType: ControllerType.PWM0,
        brightness: 255
    );

    public void SetColorSegment(string ledColor, int start, int count) {
        // if (ledColor.Length < 4) return;
        try {
            if (!ledColor.StartsWith('#')) {
                ledColor = '#' + ledColor;
            }

            if (ledColor.Length != 7) return;

            using var led = new WS281x(LedSettings);
            var color = ColorTranslator.FromHtml(ledColor);
            var end = start + count;
            for (int i = start; i < end; i++) {
                LedController.SetLED(i, color);
            }

            // LedController.SetAll(color);
            led.Render();
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task SetLedEnvirontment(double value) {
        // var pwm = PwmChannel.Create(0, 0, 400, 0.8);
        // _pwm.Stop();
        // _pwm.Dispose();
        // // _pwm.DutyCycle = value;
        // _pwm = PwmChannel.Create(0, 1, 400, value);
        // _pwm.Start();
        // var v = value;
        if (value < 0) return;
        if (value > 100) value = 100;
        using var httpClient = new HttpClient();
        // var v = value switch {
        //     > 80.0 => "5",
        //     > 60.0 => "4",
        //     > 40.0 => "3",
        //     > 20.0 => "2",
        //     > 10.0 => "1",
        //     _ => "0"
        // };

        var req = new {
            value
        };
        await httpClient.PostAsJsonAsync<object>("http://localhost:9001/pwmpins", req);
    }

    public void Dispose() {
        // _pwm.Dispose();
        GC.SuppressFinalize(this);
    }
}
