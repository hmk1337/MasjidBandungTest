using System.ComponentModel;
using MasjidBandung.Common;
using MasjidBandung.Models;

namespace MasjidBandung.Controllers;

[Produces("application/json")]
[ProducesResponseType(typeof(StatusOk), 200)]
public class LedController : ControllerBase {
    private readonly ILedService _led;
    private readonly Orchestrator _orchestrator;

    public LedController(Orchestrator orchestrator, ILedService led) {
        _orchestrator = orchestrator;
        _led = led;
    }

    /// <summary>
    /// Mengatur warna LED
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    [HttpPost("/rgb")]
    public IActionResult SetColor(
        [FromBody, DefaultValue(new[] {"#ffffff", "#ffffff", "#ffffff", "#ffffff"})]
        string[]? color
    ) {
        if (color is null || color.Length != _orchestrator.Count) return ErrorResult.Create("Jumlah tidak sesuai");
        for (int i = 0; i < _orchestrator.Count; i++) {
            _led.SetColorSegment(color[i], i * 24, 24);
        }
        return StatusOk.Create();
    }

    /// <summary>
    /// Mengatur tingkat intensitas LED lingkungan
    /// </summary>
    /// <remarks>
    /// Rentang nilai 0 (mati) sampai 100 (paling terang)
    /// </remarks>
    /// <param name="req"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPost("/led")]
    public async Task<IActionResult> SetEnvirontmentLed(
        [FromBody] LedSetModel? req,
        [FromQuery] double? value
    ) {
        // try {
        if (req is not null) {
            // var v = req.Value / 100.0;
            // if (v > 1) v = 1.0;
            // if (v < 0) v = 0.0;
            await _led.SetLedEnvirontment(req.Value);
        } else if (value is not null) {
            // var v = value.Value / 100.0;
            // if (v > 1) v = 1.0;
            // if (v < 0) v = 0.0;
            await _led.SetLedEnvirontment(value.Value);
        } else {
            return BadRequest("Nilai tidak didefinikan");
        }

        return StatusOk.Create();
        // } catch (Exception ex) {
        //     return BadRequest(ex.Message);
        // }
    }
}
