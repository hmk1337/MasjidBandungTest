using System.Diagnostics;
using MasjidBandung.Models;

namespace MasjidBandung.Controllers;

public class InfoController : ControllerBase {
    /// <summary>
    /// Mematikan sistem. Akan menghasilkan response timeout.
    /// </summary>
    /// <returns></returns>
    [HttpPost("shutdown")]
    public async Task<IActionResult> Shutdown(bool? now, [FromServices] ILedService led) {
        led.SetColorSegment("#000000", 0, 96);
        await led.SetLedEnvirontment(0);
        using var process = new Process();
        process.StartInfo.FileName = "/usr/sbin/shutdown";
        if (now is not null && now.Value) process.StartInfo.ArgumentList.Add("now");
        process.Start();
        return StatusOk.Create();
    }


    /// <summary>
    /// Melihat nomor seri atau informasi yang didefinisikan di info.txt
    /// </summary>
    /// <remarks>Buat file dengan nama info.txt di folder MasjidBandung. Isi file akan ditampilkan di sini sebagai ID.</remarks>
    /// <returns></returns>
    [HttpGet("/info")]
    public async Task<IActionResult> Index() {
        const string fileName = "info.txt";
        var sn = "unknown";
        if (System.IO.File.Exists(fileName)) {
            sn = (await System.IO.File.ReadAllTextAsync(fileName)).Trim();
        }

        var version = typeof(Program).Assembly.GetName().Version!;
        return Ok(
            new {
                id = sn,
                version = $"{version.Major}.{version.Minor}.{version.Build}"
            }
        );
    }

    /// <summary>
    /// Melihat pengaturan GRBL.
    /// </summary>
    /// <param name="motor"></param>
    /// <returns></returns>
    [HttpGet("/settings/grbl")]
    public IActionResult Settings([FromServices] IMotorService motor) {
        return Ok(motor.GetSettings());
    }
}
