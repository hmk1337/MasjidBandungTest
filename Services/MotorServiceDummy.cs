namespace MasjidBandung.Services;

public class MotorServiceDummy : IMotorService {
    public void SetStepPerMm(int stepUnit) {
        throw new NotImplementedException();
    }

    public void SetTravelDistance(int travelDistance) {
        throw new NotImplementedException();
    }

    public void SetMaxSpeed(int maxSpeed) {
        throw new NotImplementedException();
    }

    public void SetMaxAcc(int maxAcc) {
        throw new NotImplementedException();
    }

    public int Count => 4;
    public void NewCommand(MotorCommand command) { }
    public int GetCommandCount() { throw new NotImplementedException(); }
    public void Move(MotorCommand command) { }
    public bool Move(int index) { return true; }
    public bool Next() { throw new NotImplementedException(); }
    public bool First() { throw new NotImplementedException(); }
    public void Clear() { }
    public void Stop() { }

    public async Task<Dictionary<int, MachineState>> Reset() {
        await Task.Yield();
        return new Dictionary<int, MachineState>() {
            {0, MachineState.Idle},
            {1, MachineState.Idle},
            {2, MachineState.Idle},
            {3, MachineState.Idle}
        };
    }

    public async Task Unlock() { await Task.Yield(); }
    public List<double> GetPosition() { throw new NotImplementedException(); }
    public List<MachineState> GetState() { return new List<MachineState> {MachineState.Idle, MachineState.Idle, MachineState.Idle, MachineState.Idle}; }
    public List<Dictionary<int, string>> GetSettings() { throw new NotImplementedException(); }
    public List<string?> CheckId() { return new List<string?> {"", "", "", ""}; }
}
