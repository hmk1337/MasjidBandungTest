using MasjidBandung.Common;
using MasjidBandung.Models;

namespace MasjidBandung.Controllers;

[Produces("application/json")]
public sealed class MotorController : ControllerBase {
    // private static string _coreoGraphyName = string.Empty;
    // private readonly IMotorService _motor;
    // private readonly ILedService _led;

    // private static IActionResult SendOk() => new OkObjectResult(new {status = "ok"});
    // private static IActionResult SendError(string? msg) => new OkObjectResult(new {status = "error", message = msg});

    // public MotorController(IMotorService motor, ILedService led) {
    //     // _motor = motor;
    //     // _led = led;
    // }

    /// <summary>
    /// Melihat status motor
    /// </summary>
    /// <returns></returns>
    [HttpGet("/status")]
    [HttpPost("/status")]
    [ProducesResponseType(typeof(List<MotorStatus>), 200)]
    public IActionResult GetStatus([FromServices] Orchestrator orchestrator) {
        var result = new List<MotorStatus>();
        // var positions = _motor.GetPosition();
        orchestrator.GetState();
        foreach (var motor in orchestrator.Motors) {
            var status = new MotorStatus {
                Id = motor.Index,
                Position = motor.PositionPercent,
                State = motor.State
            };
            result.Add(status);
        }

        // for (int i = 0; i < _motor.Count; i++) {
        //     result.Add(
        //         new MotorStatus {
        //             Id = i + 1,
        //             Position = positions[i],
        //             State = states[i],
        //         }
        //     );
        // }

        // _motor.CheckId();
        return Ok(result);
    }

    // /// <summary>
    // /// Menambah perintah gerak, motor tidak langsung gerak. (deprecated)
    // /// </summary>
    // /// <param name="command"></param>
    // /// <returns></returns>
    // [HttpPost("/coreo/set"), ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // public IActionResult AddCoreography([FromBody] MotorCommandModel command) {
    //     _coreoGraphyName = command.Coreography;
    //     _motor.Clear();
    //     var result = Move(command);
    //     var length = command.Color.Length;
    //     if (length != _motor.Count) return result; // Ok(new {status = "ok"});
    //     for (int i = 0; i < length; i++) {
    //         _led.SetColorSegment(command.Color[i], i * 24, 24);
    //     }
    //
    //     return result;
    // }
    //
    // /// <summary>
    // /// Menjalankan koreografi (deprecated)
    // /// </summary>
    // /// <remarks>Tanpa body</remarks>
    // /// <returns></returns>
    // [HttpPost("/coreo/go"), ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // public IActionResult DoCoreography() {
    //     return Next();
    // }
    //
    // [HttpPost("/coreo/check")]
    // [HttpGet("/coreo/check")]
    // [ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // public IActionResult CheckCoreography() {
    //     return Ok(new {coreography = _coreoGraphyName});
    // }

    // /// <summary>
    // /// Menambahkan perintah gerak ke antrean
    // /// </summary>
    // /// <param name="command"></param>
    // /// <returns></returns>
    // [HttpPost("/move"), ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // [ProducesResponseType(typeof(StatusOk), 200)]
    // [ProducesResponseType(typeof(ErrorResult), 400)]
    // public IActionResult Move([FromBody] MotorCommandRequest command) {
    //     var posList = command.Position;
    //     if (posList is null || posList.Length != _motor.Count) return ErrorResult.Create("Jumlah motor di perintah tidak sesuai");
    //     if (command.Speed is null) {
    //         _motor.NewCommand(new MotorCommand(posList));
    //     } else {
    //         var speed = command.Speed.Value;
    //         _motor.NewCommand(new MotorCommand(speed, posList));
    //     }
    //
    //     return Ok(new {status = "ok", index = _motor.GetCommandCount()});
    // }

    // /// <summary>
    // /// Mengosongkan antrean perintah ke motor
    // /// </summary>
    // /// <returns></returns>
    // [HttpPost("/clear")]
    // [ProducesResponseType(typeof(StatusOk), 200)]
    // public IActionResult Clear() {
    //     _motor.Clear();
    //     return StatusOk.Create(); // Ok(new {status = "ok"});
    // }

    // /// <summary>
    // /// Menjalankan perintah berikutnya jika ada
    // /// </summary>
    // /// <returns></returns>
    // [HttpPost("/next"), ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // [ProducesResponseType(typeof(StatusOk), 200)]
    // [ProducesResponseType(typeof(ErrorResult), 400)]
    // public IActionResult Next() {
    //     return _motor.Next() ? StatusOk.Create() : ErrorResult.Create("Tidak ada antrean perintah");
    // }

    // /// <summary>
    // /// (deprecated)
    // /// </summary>
    // /// <param name="index"></param>
    // /// <returns></returns>
    // [HttpPost("/goto/{index:int}"), ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // public IActionResult GoTo(int index) {
    //     return _motor.Move(index) ? SendOk() : SendError("Index di luar rentang");
    // }
    //
    // /// <summary>
    // /// Menjalankan perintah paling awal, motor akan langsung gerak. (deprecated)
    // /// </summary>
    // /// <returns></returns>
    // [HttpPost("/first"), ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // public IActionResult First() {
    //     return _motor.First() ? SendOk() : BadRequest(new {message = "Tidak ada antrean perintah"});
    // }

    /// <summary>
    /// Menggerakkan motor secara manual, motor akan langsung gerak.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    [HttpPost("/manual")]
    [ProducesResponseType(typeof(StatusOk), 200)]
    [ProducesResponseType(typeof(ErrorResult), 400)]
    public IActionResult Manual([FromBody] MotorCommandRequest command, [FromServices] IServiceProvider serviceProvider) {
        var coreographyController = serviceProvider.GetRequiredService<CoreographyController>();
        var orchestrator = serviceProvider.GetRequiredService<Orchestrator>();

        coreographyController.Set(command, orchestrator);
        coreographyController.Go(orchestrator);

        return StatusOk.Create();

        // var posList = command.Position;
        // if (posList is null || posList.Length != _motor.Count) return ErrorResult.Create("Jumlah motor tidak sesuai");
        // if (command.Speed is null or <= 0) {
        //     _motor.Move(new MotorCommand(posList));
        // } else {
        //     var speed = command.Speed.Value;
        //     _motor.Move(new MotorCommand(speed, posList));
        // }
        //
        // // await _led.SetColorAsync(command.Color);
        // await Task.Yield();
        // var length = command.Color.Length;
        // if (length > 4) return Ok(new {status = "ok"});
        // for (int i = 0; i < length; i++) {
        //     _led.SetColorSegment(command.Color[i], i * 24, 24);
        // }
        //
        // return StatusOk.Create();
    }

    /// <summary>
    /// Membuat motor reset dan kembali ke posisi home
    /// </summary>
    /// <remarks>Tanpa request body</remarks>
    /// <returns></returns>
    [HttpPost("/home")]
    [ProducesResponseType(typeof(StatusOk), 200)]
    public IActionResult Home([FromServices] Orchestrator orchestrator) {
        // _ = _motor.Reset();
        orchestrator.Reset();

        return StatusOk.Create();
    }

    // /// <summary>
    // /// Test LED (tidak dipakai)
    // /// </summary>
    // /// <param name="req"></param>
    // /// <returns></returns>
    // [HttpGet("/led"), ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // public IActionResult SetLed(string? req) {
    //     var color = new[] {"#ffff00", "#00ff00", "#00ffff", "#ff0000"};
    //     Console.WriteLine("atur warna");
    //     if (req is not null) color = new[] {req, req, req, req};
    //     var length = color.Length;
    //     if (length > 4) return Ok(new {status = "ok"});
    //     for (int i = 0; i < length; i++) {
    //         _led.SetColorSegment(color[i], i * 24, 24);
    //     }
    //
    //     return Ok();
    // }


    /// <summary>
    /// Berhenti darurat.
    /// </summary>
    /// <remarks>Tanpa request body</remarks>
    /// <returns></returns>
    [HttpPost("/stop")]
    [ProducesResponseType(typeof(StatusOk), 200)]
    public IActionResult Stop([FromServices] Orchestrator orchestrator) {
        orchestrator.Stop();
        return StatusOk.Create();
    }

    // [HttpPost("/settings"), ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // public IActionResult Settings() {
    //     _motor.GetSettings();
    //     return BadRequest(new {message = "Belum implementasi"});
    // }

    /// <summary>
    /// Reset posisi pilar ke posisi 0
    /// </summary>
    /// <returns></returns>
    [HttpGet("/reset")]
    [HttpPost("/reset")]
    public IActionResult Reset([FromServices] Orchestrator orchestrator) {
        return Home(orchestrator);
        // var result = await _motor.Reset();
        // MotorCommand.Reset();
        // return Ok(result);
    }

    // // [HttpGet("/unlock")]
    // /// <summary>
    // /// (deprecated)
    // /// </summary>
    // /// <returns></returns>
    // [HttpPost("/unlock"), ApiExplorerSettings(GroupName = ApiGroup.Internal)]
    // [ProducesResponseType(typeof(StatusOk), 200)]
    // public async Task<IActionResult> Unlock() {
    //     await _motor.Unlock();
    //     return StatusOk.Create();
    // }
}
