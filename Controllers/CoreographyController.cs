using MasjidBandung.Common;
using MasjidBandung.Models;

namespace MasjidBandung.Controllers;

[Produces("application/json")]
[ProducesResponseType(typeof(CoreographyOkResult), 200)]
[ProducesResponseType(typeof(ErrorResult), 400)]
public sealed class CoreographyController : ControllerBase {
    // private readonly IMotorService _motor;
    // private readonly ILedService _led;
    private static string _coreoGraphyName = string.Empty;

    // private static IActionResult SendOk() => new OkObjectResult(new {status = "ok"});
    // private static IActionResult SendError(string? msg) => new OkObjectResult(new {status = "error", message = msg});

    // public CoreographyController() {
    //     // _motor = motor;
    //     // _led = led;
    // }

    /// <summary>
    /// Menerima perintah koreografi. Motor tidak langsung gerak.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="orchestrator"></param>
    /// <returns></returns>
    [HttpPost("/coreography/set")]
    public IActionResult Set([FromBody] MotorCommandRequest command, [FromServices] Orchestrator orchestrator) {
        _coreoGraphyName = command.Coreography;
        var positions = command.Position;
        var colors = command.Color;
        
        // pastikan perintah valid
        if (positions is null || positions.Length != orchestrator.Count) return BadRequest();
        
        // untuk warna, jika jumlah tidak sesuai, abaikan (warna tidak berubah)
        var setColor = colors != null && colors.Length == orchestrator.Count;
        
        for (int i = 0; i < positions.Length; i++) {
            if (positions[i] >= 0) //update
            {
                orchestrator.SetPosition(i, positions[i]);
            }
            if (setColor) orchestrator.SetColor(i, colors![i]);
        }

        // jika variabel waktu tidak ada nilainya, maka akan menggunakan nilai sebelumnya
        // variabel speed tidak lagi digunakan
        if (command.Time is > 0) {
            orchestrator.SetDuration(command.Time.Value);
        }

        orchestrator.Prepare();

        return CoreographyOkResult.Create(command.Coreography);
    }

    /// <summary>
    /// Melihat koreografi aktif.
    /// </summary>
    /// <returns></returns>
    [HttpPost("/coreography/check")]
    [HttpGet("/coreography/check")]
    public IActionResult CheckCoreography() => CoreographyOkResult.Create(_coreoGraphyName);

    /// <summary>
    /// Menjalankan koreografi yang sudah diset. Motor akan gerak.
    /// </summary>
    /// <returns></returns>
    [HttpPost("/coreography/go")]
    public IActionResult Go([FromServices] Orchestrator orchestrator) {
        orchestrator.Execute();
        return CoreographyOkResult.Create(_coreoGraphyName);
    }

    // private IActionResult Move(MotorCommandRequest command) {
    //     var posList = command.Position;
    //     if (posList is null || posList.Length != _motor.Count) return ErrorResult.Create("Jumlah motor di perintah tidak sesuai");
    //     if (command.Time is > 0) {
    //         _motor.NewCommand(MotorCommand.WithDuration(posList, command.Time.Value));
    //     } else if (command.Speed is null) {
    //         _motor.NewCommand(new MotorCommand(posList));
    //     } else {
    //         var speed = command.Speed.Value;
    //         _motor.NewCommand(new MotorCommand(speed, posList));
    //     }
    //
    //     return CoreographyOkResult.Create(command.Coreography);
    // }
}
