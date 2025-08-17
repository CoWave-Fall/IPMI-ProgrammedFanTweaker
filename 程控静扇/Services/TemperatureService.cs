using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace 程控静扇.Services
{
    public class TemperatureService
    {
        private static readonly Lazy<TemperatureService> _instance = new(() => new TemperatureService());
        public static TemperatureService Instance => _instance.Value;

        private readonly CoreTempService _coreTempService;
        private readonly CpuTemperatureService _cpuTemperatureService;
        // private readonly SocketsTempClient _socketsTempClient; // Placeholder

        private TemperatureService()
        {
            _coreTempService = new CoreTempService();
            _cpuTemperatureService = new CpuTemperatureService();
            // _socketsTempClient = new SocketsTempClient(); // Placeholder
        }

        public async Task<List<float>> GetTemperaturesAsync()
        {
            var source = SettingsService.LoadTemperatureSource();

            switch (source)
            {
                case TemperatureSource.CoreTemp:
                    var coreTempData = _coreTempService.GetMaxTemperaturesPerCpu();
                    //Debug.WriteLine($"CoreTempService Temperatures: {string.Join(", ", coreTempData)}");
                    return coreTempData;
                case TemperatureSource.Sockets:
                    await Task.CompletedTask; // Placeholder
                    return new List<float>(); // Placeholder
                case TemperatureSource.Independent:
                    return _cpuTemperatureService.GetCpuTemperatures()
                                               .Select(t => t.Value ?? 0f)
                                               .ToList();
                default:
                    return new List<float>();
            }
        }
    }
}
