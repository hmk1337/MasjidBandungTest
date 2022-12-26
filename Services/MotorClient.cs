using System.Text.Json.Serialization;
namespace MasjidBandung.Services.Client;

/// <summary>
/// Berisi informasi posisi dan warna lampu untuk individual motor
/// </summary>
public class MotorPosition {
    public int Index { get; set; }
    public double Position { get; set; }
    public short[] Color { get; set; } = new short[3] { 255, 255, 255 };
}

/// <summary>
/// Tipe data matrix 2 dimensi dengan key x dan y sebagai integer
/// </summary>
public class MotorMatrix : Dictionary<(int, int), MotorPosition> { }
public class MotorController {
    public string IpAddress { get; set; } = string.Empty;
    public int MotorIndex { get; set; }
}

/// <summary>
/// Berisi mapping sebuah motor ke IP address dan controller yang berkaitan
/// </summary>
public class MotorMapping : Dictionary<(int, int), MotorController> {
    public List<string> IpAddressList = new List<string>();
    /// <summary>
    /// Mapping sebuah motor ke sebuah IP
    /// </summary>
    public Dictionary<(int, int), int> MotorControllerMapping = new Dictionary<(int, int), int>();

    /// <summary>
    /// Mapping sebuah motor ke suatu index di sebuah controller
    /// </summary>
    public Dictionary<(int, int), int> MotorIndexMapping = new Dictionary<(int, int), int>();

    public void AddMotor((int, int) coordinate, string ipAddress, int motorIndex) {
        // tambahkan IP jika belum tersedia
        if (!IpAddressList.Contains(ipAddress)) IpAddressList.Add(ipAddress);

        // dapatkan index IP dari list untuk dipakai di mapping
        int controllerIndex = IpAddressList.IndexOf(ipAddress);

        // atur atau tambahkan controller mapping
        if (MotorControllerMapping.ContainsKey(coordinate)) {
            MotorControllerMapping[coordinate] = controllerIndex;
        } else {
            MotorControllerMapping.Add(coordinate, controllerIndex);
        }

        // atur atau tambahkan motor index mapping
        if (MotorIndexMapping.ContainsKey(coordinate)) {
            MotorIndexMapping[coordinate] = motorIndex;
        } else {
            MotorIndexMapping.Add(coordinate, motorIndex);
        }

    }
    public List<(int, int)> GetMotorsByController(int controllerIndex) {
        var list = MotorControllerMapping.Where(item => item.Value == controllerIndex);
        var result = new List<(int, int)>();
        foreach (var item in list) {
            result.Add(item.Key);
        }
        return result;
    }
    public int GetMotorIndex((int, int) coordinate) {
        if (MotorIndexMapping.ContainsKey(coordinate)) {
            return MotorIndexMapping[coordinate];
        } else {
            // motor tidak ditemukan
            return -1;
        }
    }
}

/// <summary>
/// Berisi matrix 2 dimensi yang berisi posisi motor dan warna lampu
/// </summary>
public class MotorCommand {
    public MotorCommand() { }

    /// <summary>
    /// Menambah atau update elemen pada matrix
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="position"></param>
    public void Set(int x, int y, MotorPosition position) {
        if (Matrix.ContainsKey((x, y))) {
            // di matrix suda ada, update saja
            Matrix[(x, y)] = position;
        } else {
            // di matrix belum ada, tambahkan
            Matrix.Add((x, y), position);
        }
    }
    public MotorMatrix Matrix { get; set; } = new MotorMatrix();
}

public class MotorClient {
    // Mapping

    /// <summary>
    /// Berisi koleksi command berurut index dari 0
    /// </summary>
    public List<MotorCommand> CommandList = new List<MotorCommand>();

    public void Add(MotorCommand command) {
        CommandList.Add(command);
    }

    /// <summary>
    /// Mengirim daftar perintah ke masing-masing motor
    /// </summary>
    public void SendCommandAsync(MotorMapping motors) {
        // buat daftar paket perintah
        var packet = new Dictionary<string, CommandPayload>();
        foreach (var ip in motors.IpAddressList) {
            packet.Add(ip, new CommandPayload());
        }

        for (int j = 0; j < CommandList.Count; j++) {
            var cmd = CommandList[j];
            // iterasi untuk setiap controller, ambil motor-motor yang ada di koordinat tersebut, kemudian masukkan ke list
            for (int i = 0, n = motors.IpAddressList.Count; i < n; i++) {
                // ambil daftar motor yang ada di ip tersebut
                List<(int, int)> coords = motors.GetMotorsByController(i);

                // siapkan daftar perintah untuk ditambahkan nanti
                var commandItem = new List<CommandPayload>();
                var motorList = new List<MotorPosition>();

                // ambil target posisi dan warna lampu untuk motor tersebut
                foreach (var coord in coords) {
                    var position = cmd.Matrix[coord];
                    motorList.Add(position);
                }

                // masukkan posisi motor ke payload, dengan index adalah urutan nomor perintah
                commandItem.Add(new CommandPayload { Index = j, Motors = motorList });

            }

        }

    }


    /// <summary>
    /// Mengambil status dari motor
    /// </summary>
    public void GetStatusAsync() {

    }

    /// <summary>
    /// Menjalankan perintah index tertentu
    /// </summary>
    public void ExecuteIndexAsync(int index) {

    }
}

/// <summary>
/// Berisi data JSON yang akan dikirim ke masing-masing controller
/// </summary>
public class CommandPayload {
    //public string? IpAddress { get; set; }
    public int Index { get; set; }
    public List<MotorPosition> Motors { get; set; } = new List<MotorPosition>();
}

public class CommandPayloadCollection {
    public string? IpAddress { get; set; }
    public List<CommandPayload> Commands { get; set; } = new List<CommandPayload>();
}