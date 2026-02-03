using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SmartFactoryMini
{
    // -----------------------------
    // Common DTOs / Enums
    // -----------------------------
    enum MachineState { Idle, Ready, Running, Complete, Error }

    record Recipe(string Name, int ProcessSeconds);

    record WorkOrder(string LotId, string MachineId, Recipe Recipe, string Destination);

    // -----------------------------
    // PLC (lowest layer) - I/O simulation
    // -----------------------------
    class PlcSim
    {
        private readonly ConcurrentDictionary<string, bool> _bits = new();
        private readonly ConcurrentDictionary<string, int> _words = new();

        public bool ReadBit(string addr) => _bits.TryGetValue(addr, out var v) && v;
        public void WriteBit(string addr, bool value) => _bits[addr] = value;

        public int ReadWord(string addr) => _words.TryGetValue(addr, out var v) ? v : 0;
        public void WriteWord(string addr, int value) => _words[addr] = value;

        public override string ToString()
            => $"PLC bits={{SensorOK:{ReadBit("SensorOK")}, Motor:{ReadBit("Motor")}, StartCmd:{ReadBit("StartCmd")}, StopCmd:{ReadBit("StopCmd")}}} " +
               $"words={{ErrorCode:{ReadWord("ErrorCode")}}}";
    }

    // -----------------------------
    // ECS (Equipment Control SW on PC) - talks to PLC, exposes "commands" to SCADA
    // -----------------------------
    class EquipmentController
    {
        private readonly PlcSim _plc;
        private readonly string _machineId;

        public MachineState State { get; private set; } = MachineState.Idle;
        public event Action<string, MachineState>? OnStateChanged;
        public event Action<string, string>? OnAlarm;
        public event Action<string, string>? OnLog;

        private Recipe? _currentRecipe;

        public EquipmentController(string machineId, PlcSim plc)
        {
            _machineId = machineId;
            _plc = plc;
        }

        public void Initialize()
        {
            State = MachineState.Ready;
            Log("Initialize -> Ready");
            OnStateChanged?.Invoke(_machineId, State);
        }

        public void ApplyRecipe(Recipe recipe)
        {
            _currentRecipe = recipe;
            Log($"Recipe applied: {recipe.Name} ({recipe.ProcessSeconds}s)");
        }

        // SCADA calls these "high level commands"
        public async Task<bool> StartAsync(TimeSpan timeout)
        {
            if (State != MachineState.Ready)
            {
                Alarm($"Start rejected: State={State}");
                return false;
            }
            if (!_plc.ReadBit("SensorOK"))
            {
                State = MachineState.Error;
                _plc.WriteWord("ErrorCode", 1001);
                Alarm("Interlock: SensorOK=FALSE");
                OnStateChanged?.Invoke(_machineId, State);
                return false;
            }
            if (_currentRecipe is null)
            {
                Alarm("Start rejected: No recipe");
                return false;
            }

            // Command to PLC (write bit)
            _plc.WriteBit("StartCmd", true);
            Log("StartCmd -> PLC");

            // Wait for PLC ack: Motor ON
            var ok = await WaitUntilAsync(() => _plc.ReadBit("Motor"), timeout);
            _plc.WriteBit("StartCmd", false);

            if (!ok)
            {
                State = MachineState.Error;
                _plc.WriteWord("ErrorCode", 2001);
                Alarm("Timeout: Motor did not turn ON");
                OnStateChanged?.Invoke(_machineId, State);
                return false;
            }

            State = MachineState.Running;
            OnStateChanged?.Invoke(_machineId, State);
            Log("State -> Running");

            // Simulate processing duration controlled by "recipe"
            _ = RunProcessAsync(_currentRecipe.ProcessSeconds);

            return true;
        }

        public async Task<bool> StopAsync(TimeSpan timeout)
        {
            // Command to PLC (write bit)
            _plc.WriteBit("StopCmd", true);
            Log("StopCmd -> PLC");

            // Wait for PLC ack: Motor OFF
            var ok = await WaitUntilAsync(() => !_plc.ReadBit("Motor"), timeout);
            _plc.WriteBit("StopCmd", false);

            State = MachineState.Idle;
            OnStateChanged?.Invoke(_machineId, State);

            if (!ok)
            {
                Alarm("Timeout: Motor did not turn OFF (forcing Idle)");
                return false;
            }

            Log("Stopped -> Idle");
            return true;
        }

        // PLC scan loop simulation (normally PLC runs independently)
        public async Task PlcScanLoopAsync(CancellationToken ct)
        {
            // Initial conditions
            _plc.WriteBit("Motor", false);
            _plc.WriteWord("ErrorCode", 0);

            while (!ct.IsCancellationRequested)
            {
                // Start command handling
                if (_plc.ReadBit("StartCmd") && _plc.ReadBit("SensorOK"))
                {
                    _plc.WriteBit("Motor", true);
                }

                // Stop command handling
                if (_plc.ReadBit("StopCmd"))
                {
                    _plc.WriteBit("Motor", false);
                }

                await Task.Delay(50, ct); // PLC scan cycle
            }
        }

        private async Task RunProcessAsync(int seconds)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds));
                if (State == MachineState.Running)
                {
                    State = MachineState.Complete;
                    Log("Process complete");
                    OnStateChanged?.Invoke(_machineId, State);

                    // After complete, go back to Ready (typical)
                    await Task.Delay(200);
                    State = MachineState.Ready;
                    OnStateChanged?.Invoke(_machineId, State);
                    Log("State -> Ready");
                }
            }
            catch { /* ignore */ }
        }

        private async Task<bool> WaitUntilAsync(Func<bool> predicate, TimeSpan timeout)
        {
            var start = DateTime.UtcNow;
            while (DateTime.UtcNow - start < timeout)
            {
                if (predicate()) return true;
                await Task.Delay(50);
            }
            return false;
        }

        private void Alarm(string msg)
        {
            OnAlarm?.Invoke(_machineId, msg);
            Log("ALARM: " + msg);
        }

        private void Log(string msg) => OnLog?.Invoke(_machineId, msg);
    }

    // -----------------------------
    // SCADA - supervises equipment controllers, provides UI-like actions
    // -----------------------------
    class Scada
    {
        public event Action<string, MachineState>? OnMachineState;
        public event Action<string, string>? OnAlarm;
        public event Action<string, string>? OnLog;

        public void Attach(EquipmentController ecs)
        {
            ecs.OnStateChanged += (mid, st) => OnMachineState?.Invoke(mid, st);
            ecs.OnAlarm += (mid, msg) => OnAlarm?.Invoke(mid, msg);
            ecs.OnLog += (mid, msg) => OnLog?.Invoke(mid, msg);
        }

        // SCADA "command patterns" with retry/timeout
        public async Task<bool> StartWithRetryAsync(EquipmentController ecs, int retries, TimeSpan timeout)
        {
            for (int i = 1; i <= retries; i++)
            {
                OnLog?.Invoke("SCADA", $"Start attempt {i}/{retries}");
                var ok = await ecs.StartAsync(timeout);
                if (ok) return true;
                await Task.Delay(200);
            }
            return false;
        }
    }

    // -----------------------------
    // AGV / Conveyor simulation + MCS
    // -----------------------------
    class AgvSim
    {
        public event Action<string>? OnArrived;
        public event Action<string>? OnLog;

        public async Task MoveAsync(string lotId, string destination)
        {
            OnLog?.Invoke($"AGV moving lot={lotId} -> {destination}");
            await Task.Delay(800); // move time
            OnArrived?.Invoke(lotId);
            OnLog?.Invoke($"AGV arrived lot={lotId} at {destination}");
        }
    }

    class Mcs
    {
        private readonly AgvSim _agv;

        public event Action<string>? OnLog;
        public event Action<string>? OnLotArrived;

        public Mcs(AgvSim agv)
        {
            _agv = agv;
            _agv.OnArrived += lotId => OnLotArrived?.Invoke(lotId);
            _agv.OnLog += msg => OnLog?.Invoke(msg);
        }

        public async Task DispatchToAsync(string lotId, string destination)
        {
            OnLog?.Invoke($"MCS dispatch: lot={lotId} destination={destination}");
            await _agv.MoveAsync(lotId, destination);
        }
    }

    // -----------------------------
    // MES (Host) - issues work orders, receives reports
    // -----------------------------
    class Mes
    {
        public event Action<WorkOrder>? OnWorkOrder;
        public event Action<string>? OnLog;

        public void IssueWorkOrder(WorkOrder wo)
        {
            OnLog?.Invoke($"MES issued WO: lot={wo.LotId}, machine={wo.MachineId}, recipe={wo.Recipe.Name}, dest={wo.Destination}");
            OnWorkOrder?.Invoke(wo);
        }

        public void Report(string message) => OnLog?.Invoke("MES report: " + message);
    }

    // -----------------------------
    // Orchestrator - ties MES, MCS, SCADA, ECS together
    // -----------------------------
    class Orchestrator
    {
        private readonly Mes _mes;
        private readonly Mcs _mcs;
        private readonly Scada _scada;
        private readonly EquipmentController _ecs;

        private WorkOrder? _current;

        public Orchestrator(Mes mes, Mcs mcs, Scada scada, EquipmentController ecs)
        {
            _mes = mes; _mcs = mcs; _scada = scada; _ecs = ecs;

            _mes.OnWorkOrder += HandleWorkOrder;
            _mcs.OnLotArrived += HandleLotArrived;

            _scada.OnMachineState += (mid, st) =>
            {
                Console.WriteLine($"[STATE] {mid} -> {st}");
                if (_current is not null && mid == _current.MachineId && st == MachineState.Complete)
                {
                    _mes.Report($"Lot {_current.LotId} completed on {mid}");
                    // After complete, request MCS to move to destination
                    _ = _mcs.DispatchToAsync(_current.LotId, _current.Destination);
                }
            };

            _scada.OnAlarm += (mid, msg) => Console.WriteLine($"[ALARM] {mid}: {msg}");
            _scada.OnLog += (mid, msg) => Console.WriteLine($"[LOG] {mid}: {msg}");
            _mcs.OnLog += msg => Console.WriteLine($"[LOG] MCS: {msg}");
            _mes.OnLog += msg => Console.WriteLine($"[LOG] {msg}");
        }

        private void HandleWorkOrder(WorkOrder wo)
        {
            _current = wo;
            // Step 1) MCS moves lot to machine
            _ = _mcs.DispatchToAsync(wo.LotId, $"Machine:{wo.MachineId}");
        }

        private async void HandleLotArrived(string lotId)
        {
            if (_current is null || _current.LotId != lotId) return;

            // Step 2) Apply recipe (MES -> SCADA -> ECS)
            _ecs.ApplyRecipe(_current.Recipe);

            // Step 3) SCADA starts equipment (with timeout/retry)
            var ok = await _scada.StartWithRetryAsync(_ecs, retries: 2, timeout: TimeSpan.FromSeconds(2));

            if (!ok)
            {
                _mes.Report($"Lot {lotId} failed to start (alarm/timeout).");
            }
            else
            {
                _mes.Report($"Lot {lotId} started on {_current.MachineId}");
            }
        }
    }

    // -----------------------------
    // Program - demo run
    // -----------------------------
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("=== Smart Factory Mini (MES + MCS + SCADA + ECS + PLC) ===");

            var plc = new PlcSim();
            var ecs = new EquipmentController(machineId: "EQP-01", plc);
            var scada = new Scada();
            scada.Attach(ecs);

            var agv = new AgvSim();
            var mcs = new Mcs(agv);
            var mes = new Mes();

            var orchestrator = new Orchestrator(mes, mcs, scada, ecs);

            // Initialize equipment
            ecs.Initialize();

            // PLC scan loop
            var cts = new CancellationTokenSource();
            var plcTask = ecs.PlcScanLoopAsync(cts.Token);

            // Set sensor OK (interlock pass)
            plc.WriteBit("SensorOK", true);

            // MES issues a work order
            mes.IssueWorkOrder(new WorkOrder(
                LotId: "LOT-A001",
                MachineId: "EQP-01",
                Recipe: new Recipe("Formation-IR", ProcessSeconds: 2),
                Destination: "Inspection"
            ));

            // Let it run
            await Task.Delay(5000);

            // Demo: sensor fail causes interlock error on next order
            Console.WriteLine("\n=== Next order with SensorOK=false (should alarm) ===");
            plc.WriteBit("SensorOK", false);

            mes.IssueWorkOrder(new WorkOrder(
                LotId: "LOT-A002",
                MachineId: "EQP-01",
                Recipe: new Recipe("Formation-OCV", ProcessSeconds: 2),
                Destination: "Rework"
            ));

            await Task.Delay(3000);

            // Cleanup
            cts.Cancel();
            try { await plcTask; } catch { /* ignore */ }

            Console.WriteLine("\n=== End ===");
        }
    }
}
