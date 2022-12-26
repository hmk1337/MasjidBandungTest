namespace MasjidBandung.Controllers;

[Route("settings")]
public class SettingsController : ControllerBase {
    private readonly IMotorService _motorService;

    public SettingsController(IMotorService motorService) {
        _motorService = motorService;
    }

    [HttpPost("distance")]
    public IActionResult SetTravelDistance(int travelDistance) {
        if (travelDistance <= 0) return BadRequest("Travel distance must be greater than 0");
        _motorService.SetTravelDistance(travelDistance);
        return Ok();
    }

    [HttpPost("speed")]
    public IActionResult SetMaxSpeed(int maxSpeed) {
        if (maxSpeed <= 0) return BadRequest("Maximum speed must be greater than 0");
        _motorService.SetMaxSpeed(maxSpeed);
        return Ok();
    }

    [HttpPost("stepunit")]
    public IActionResult SetStepPerMm(int stepPerMm) {
        if (stepPerMm <= 0) return BadRequest("Step per mm must be greater then 0");
        _motorService.SetStepPerMm(stepPerMm);
        return Ok();
    }

    [HttpPost("acceleration")]
    public IActionResult SetMaxAcceleration(int maxAcc) {
        if (maxAcc <= 0) return BadRequest("Maximum acceleration must be reater than 0");
        _motorService.SetMaxAcc(maxAcc);
        return Ok();
    }
}
