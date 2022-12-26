using MasjidBandung.Models;

namespace MasjidBandung.Controllers;

[Produces("application/json")]
[ProducesResponseType(typeof(CoreographyOkResult), 200)]
[ProducesResponseType(typeof(ErrorResult), 400)]
public class CoreographyController : ControllerBase {
    private readonly IMotorService _motor;
    private readonly ILedService _led;
    private static string _coreoGraphyName = string.Empty;

    // private static IActionResult SendOk() => new OkObjectResult(new {status = "ok"});
    // private static IActionResult SendError(string? msg) => new OkObjectResult(new {status = "error", message = msg});

    public CoreographyController(IMotorService motor, ILedService led) {
        _motor = motor;
        _led = led;
    }

    /// <summary>
    /// Menerima perintah koreografi. Motor tidak langsung gerak.
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [HttpPost("/coreography/set")]
    public IActionResult Set([FromBody] MotorCommandRequest command) {
        _coreoGraphyName = command.Coreography;
        _motor.Clear();
        var result = Move(command);
        var length = command.Color.Length;
        if (length != _motor.Count) return result; // Ok(new {status = "ok"});
        _led.SetColor(command.Color);
        // for (int i = 0; i < length; i++) {
        //     _led.SetColorSegment(command.Color[i], i * 24, 24);
        // }

        return result;
    }

    /// <summary>
    /// Melihat koreografi aktif.
    /// </summary>
    /// <returns></returns>
    [HttpPost("/coreography/check")]
    [HttpGet("/coreography/check")]
    public IActionResult CheckCoreography() {
        return CoreographyOkResult.Create(_coreoGraphyName);
    }

    /// <summary>
    /// Menjalankan koreografi yang sudah diset. Motor akan gerak.
    /// </summary>
    /// <returns></returns>
    [HttpPost("/coreography/go")]
    public IActionResult Go() {
        var result = _motor.Next() ? CoreographyOkResult.Create(_coreoGraphyName) : ErrorResult.Create("Tidak ada koreografi");
        _led.SetColor();
        return result;
    }

    private IActionResult Move(MotorCommandRequest command) {
        var posList = command.Position;
        if (posList is null || posList.Length != _motor.Count) return ErrorResult.Create("Jumlah motor di perintah tidak sesuai");
        if (command.Time is > 0) {
            _motor.NewCommand(MotorCommand.WithDuration(posList, command.Time.Value));
        } else if (command.Speed is null) {
            _motor.NewCommand(new MotorCommand(posList));
        } else {
            var speed = command.Speed.Value;
            _motor.NewCommand(new MotorCommand(speed, posList));
        }

        return CoreographyOkResult.Create(command.Coreography);
    }
}
