using System.Diagnostics;
using System.Globalization;
using MasjidBandung;
using MasjidBandung.Common;
using MasjidBandung.Configurations;
using Ngb.SerialPortHelper;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

var services = builder.Services;

services.Configure<MotorConfiguration>(config.GetSection("MotorSettings"));
// services.Configure<MotorConfiguration>(x => x.MotorCount = 8);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// #if DEBUG
builder.Services.AddEndpointsApiExplorer().AddSwaggerGenOptions();

// #endif

// var channelMapping = new Dictionary<int, char> {
//     {0, 'X'},
//     {1, 'Y'},
//     {2, 'X'},
//     {3, 'Y'},
//     {4, 'X'},
//     {5, 'Y'},
//     {6, 'X'},
//     {7, 'Y'},
// };

// var port1 = OperatingSystem.IsWindows() ? "COM5" : "/dev/ttyUSB0";
// string[] portList;

// portList = OperatingSystem.IsLinux()
//     ? SerialPort.GetPortNames().Where(p => p.Contains("ttyUSB")).Take(2).ToArray()
//     : SerialPort.GetPortNames().Take(2).ToArray();

// var portFactory = new SerialPortFactory();
// await portFactory.WaitAllDevicesAsync();
// var motorService = new MotorService(channelMapping, portFactory.GetDevices() /*, port2*/);
//var port2 = OperatingSystem.IsWindows() ? "COM2" : "/dev/ttyUSB1";
// builder.Services.AddSingleton<IMotorService>(motorService);
if (OperatingSystem.IsWindows()) {
    services.AddSingleton<ILedService, LedTestService>();
} else {
    services.AddSingleton<ILedService, LedService>();
}

services.AddSingleton<Orchestrator>();

// daftarkan serial port di sini
services.AddSerialPort(
    configuration => configuration.WithDefaultBaudRate(115200)
        .Filter(x => x.Contains(OperatingSystem.IsWindows() ? "COM" : "ttyUSB"))
        // .AddPort("1", (r, _) => SerialPortIdentity.Read(r, "1000"))
        // .AddPort("2", (r, _) => SerialPortIdentity.Read(r, "2000"))
        // .AddPort("3", (r, _) => SerialPortIdentity.Read(r, "3000"))
        // .AddPort("4", (r, _) => SerialPortIdentity.Read(r, "4000"))
        // .AddPort(
        //     "2", (r, w) => {
        //         foreach (var line in r.ReadAllLines()) {
        //             if (SerialPortIdentity.Trigger(line, ref w)) continue;
        //             if (SerialPortIdentity.Identify(line, "2000")) return true;
        //         }
        //
        //         return false;
        //     }
        // )
        // .AddPort(
        //     "3", (r, w) => {
        //         foreach (var line in r.ReadAllLines()) {
        //             if (SerialPortIdentity.Trigger(line, ref w)) continue;
        //             if (SerialPortIdentity.Identify(line, "3000")) return true;
        //         }
        //
        //         return false;
        //     }
        // )
        // .AddPort(
        //     "4", (r, w) => {
        //         foreach (var line in r.ReadAllLines()) {
        //             if (SerialPortIdentity.Trigger(line, ref w)) continue;
        //             if (SerialPortIdentity.Identify(line, "4000")) return true;
        //         }
        //
        //         return false;
        //     }
        // )
        .AddPort("1", (r, w) => SerialPortIdentity.Identify(ref r, ref w, "1000"))
        .AddPort("2", (r, w) => SerialPortIdentity.Identify(ref r, ref w, "2000"))
       // .AddPort("3", (r, w) => SerialPortIdentity.Identify(ref r, ref w, "3000"))
        //.AddPort("4", (r, w) => SerialPortIdentity.Identify(ref r, ref w, "4000"))
        .SetTimeout(10)
);

var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment()) {
//     app.UseExceptionHandler("/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }

using (var scope = app.Services.CreateScope()) {
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Initalizer");

    // jika perlu untuk mengatur warna default LED saat pertama dinyalakan
    var led = scope.ServiceProvider.GetRequiredService<ILedService>();
    led.SetColorSegment("#ffffff", 0, 24);
    led.SetColorSegment("#ff0000", 24, 24);
    led.SetColorSegment("#00ff00", 48, 24);
    led.SetColorSegment("#ffff00", 72, 24);

    // daftarkan semua motor di sini
    var sw = Stopwatch.StartNew();
    scope.ServiceProvider.GetRequiredService<ISerialPortProvider>().WaitAsync().Wait();
    sw.Stop();
    logger.LogInformation("Serial ports scanning finished in {Time}", sw.Elapsed);
    scope.ServiceProvider.GetRequiredService<Orchestrator>()
        .AddMotor(0, "1", MotorChannel.X)
        .AddMotor(1, "1", MotorChannel.Y)
        .AddMotor(2, "2", MotorChannel.X)
        .AddMotor(3, "2", MotorChannel.Y)
        //.AddMotor(4, "3", MotorChannel.X)
        //.AddMotor(5, "3", MotorChannel.Y)
        //.AddMotor(6, "4", MotorChannel.X)
        //.AddMotor(7, "4", MotorChannel.Y)
        .Reset();
}

app.UseStaticFiles();

app.UseRouting();

// app.UseAuthorization();
app.UseSwagger().UseSwaggerUiOptions();
app.MapControllers();

var host = OperatingSystem.IsWindows() ? "http://localhost:8080/" : "http://*:8080";
app.Run(host);

// portFactory.Dispose();
