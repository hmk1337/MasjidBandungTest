using System.ComponentModel;

namespace MasjidBandung.Models;

public class ErrorResult {
    [DefaultValue("error")]
    public string Status { get; set; }

    public string Message { get; set; }

    public ErrorResult(string message) {
        Status = "error";
        Message = message;
    }

    public ErrorResult() : this(string.Empty) { }

    public static IActionResult Create(string message = "") {
        return new BadRequestObjectResult(new ErrorResult(message));
    }
}
