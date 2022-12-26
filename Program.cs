using System.Globalization;
using System.IO.Ports;
using MasjidBandung;
using MasjidBandung.SerialDevice;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// #if DEBUG
builder.Services.AddEndpointsApiExplorer().AddSwaggerGenOptions();

// #endif

var channelMapping = new Dictionary<int, char> {
    {0, 'X'},
    {1, 'Y'},
    {2, 'X'},
    {3, 'Y'},
    {4, 'X'},
    {5, 'Y'},
    {6, 'X'},
    {7, 'Y'},
};

// var port1 = OperatingSystem.IsWindows() ? "COM5" : "/dev/ttyUSB0";
// string[] portList;

var led = new LedService();
led.SetColorSegment("#ffffff", 0, 24);
led.SetColorSegment("#ff0000", 24, 24);
led.SetColorSegment("#00ff00", 48, 24);
led.SetColorSegment("#ffff00", 72, 24);

// portList = OperatingSystem.IsLinux()
//     ? SerialPort.GetPortNames().Where(p => p.Contains("ttyUSB")).Take(2).ToArray()
//     : SerialPort.GetPortNames().Take(2).ToArray();

var portFactory = new SerialPortFactory();
await portFactory.WaitAllDevicesAsync();
var motorService = new MotorService(channelMapping, portFactory.GetDevices() /*, port2*/);
//var port2 = OperatingSystem.IsWindows() ? "COM2" : "/dev/ttyUSB1";
builder.Services.AddSingleton<IMotorService>(motorService);
builder.Services.AddSingleton<ILedService>(led);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSwagger().UseSwaggerUiOptions();
// app.MapRazorPages();
app.MapControllers();

var host = OperatingSystem.IsWindows() ? "http://localhost:8080/" : "http://*:8080";
app.Run(host);

portFactory.Dispose();
