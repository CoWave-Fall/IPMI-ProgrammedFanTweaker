using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;

namespace 程控静扇.Services
{
    public class CpuTemperatureService : IDisposable
    {
        private readonly Computer _computer;
        private readonly IHardware[] _cpus;

        public CpuTemperatureService()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true
            };
            _computer.Open();
            _cpus = _computer.Hardware.Where(h => h.HardwareType == HardwareType.Cpu).ToArray();
        }

        public IEnumerable<(string Name, float? Value)> GetCpuTemperatures()
        {
            if (_cpus == null) yield break;

            foreach (var cpu in _cpus)
            {
                cpu.Update();

                var packageSensor = cpu.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Temperature && s.Name.Contains("Package"));
                if (packageSensor != null)
                {
                    yield return ($"{cpu.Name} - {packageSensor.Name}", packageSensor.Value);
                }
                else // Fallback to other temp sensors if no "Package" sensor is found
                {
                    var tempSensors = cpu.Sensors.Where(s => s.SensorType == SensorType.Temperature);
                    foreach (var sensor in tempSensors)
                    {
                        yield return ($"{cpu.Name} - {sensor.Name}", sensor.Value);
                    }
                }
            }
        }

        public void Dispose()
        {
            _computer.Close();
        }
    }
}
