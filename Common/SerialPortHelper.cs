using Ngb.SerialPortHelper;

namespace MasjidBandung.Common;

public static class SerialPortIdentity {
    /// <summary>
    /// Fungsi pembantu untuk identifikasi gcode
    /// </summary>
    /// <param name="reader">Serial reader</param>
    /// <param name="identity">Kode identifikasi gcode</param>
    /// <returns></returns>
    public static bool Read(SerialPortReader reader, string identity) {
        foreach (string line in reader.ReadAllLines()) {
            if (line.Contains("$112") && line.Contains(identity)) return true;
        }

        return false;
    }

    /// <summary>
    /// Fungsi pembantu untuk identifikasi gcode
    /// </summary>
    /// <param name="reader">Serial reader</param>
    /// <param name="writer"></param>
    /// <param name="identity">Kode identifikasi gcode</param>
    /// <returns></returns>
    public static bool Identify(ref SerialPortReader reader, ref SerialPortWriter writer, string identity) {
        foreach (string line in reader.ReadAllLines()) {
            // Console.WriteLine($"{identity}: {line}");
            if (line.Contains("$X")) {
                writer.WriteLine("$$");
                continue;
            }

            if (line.Contains("$112") && line.Contains(identity)) {
                Console.WriteLine($"Port {identity} found");
                return true;
            }
        }

        return false;
    }

    public static bool Trigger(string line, ref SerialPortWriter writer) {
        if (!line.Contains("$X")) return false;
        writer.WriteLine("$$");
        return true;
    }

    public static bool Identify(string line, string identity) {
        return line.Contains("$112") && line.Contains(identity);
    }
}
