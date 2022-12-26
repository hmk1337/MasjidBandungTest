using System.ComponentModel;

namespace MasjidBandung.Models;

public class CoreographyOkResult {
    [DefaultValue("ok")]
    public string Status { get; set; }

    [DefaultValue("NamaKoreografi")]
    public string Coreography { get; set; }

    public CoreographyOkResult() : this(string.Empty) { }

    public CoreographyOkResult(string coreography) {
        Status = "ok";
        Coreography = coreography;
    }

    public static IActionResult Create(string message = "") {
        return new OkObjectResult(new CoreographyOkResult(message));
    }
}
