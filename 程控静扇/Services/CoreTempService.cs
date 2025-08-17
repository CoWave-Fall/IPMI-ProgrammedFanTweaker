using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using GetCoreTempInfoNET;

namespace 程控静扇.Services
{
    public class CoreTempService
    {
        private readonly CoreTempInfo _ctInfo;

        public CoreTempService()
        {
            _ctInfo = new CoreTempInfo();
        }

        public List<float> GetMaxTemperaturesPerCpu()
        {
            var temps = new List<float>();
            if (!_ctInfo.GetData()) return temps;

            uint cpuCount = _ctInfo.GetCPUCount;
            uint coresPerCpu = _ctInfo.GetCoreCount;

            // 如果库报告没有CPU或核心，则直接返回
            if (cpuCount == 0 || coresPerCpu == 0) return temps;

            uint totalExpectedCores = cpuCount * coresPerCpu;

            //Debug.WriteLine($"[CoreTempService] CPU Count: {cpuCount}, Cores Per CPU: {coresPerCpu}, Total Temp Readings in Array: {_ctInfo.GetTemp.Length}");

            for (uint cpuIndex = 0; cpuIndex < cpuCount; cpuIndex++)
            {
                float maxTempForThisCpu = 0f;
                for (uint coreIndex = 0; coreIndex < coresPerCpu; coreIndex++)
                {
                    uint absoluteIndex = (cpuIndex * coresPerCpu) + coreIndex;

                    // 关键安全检查：确保我们不会读取超出数组边界的数据
                    if (absoluteIndex < _ctInfo.GetTemp.Length)
                    {
                        float currentCoreTemp = _ctInfo.GetTemp[absoluteIndex];
                        if (currentCoreTemp > maxTempForThisCpu)
                        {
                            maxTempForThisCpu = currentCoreTemp;
                        }
                    }
                }
                temps.Add(maxTempForThisCpu);
            }
            return temps;
        }


public string GetCoreTempDataAsString()
        {
            bool bReadSuccess = _ctInfo.GetData();

            if (bReadSuccess)
            {
                var sb = new StringBuilder();
                char tempType = _ctInfo.IsFahrenheit ? 'F' : 'C';

                sb.AppendLine($"CPU Name: {_ctInfo.GetCPUName}");
                sb.AppendLine($"CPU Speed: {_ctInfo.GetCPUSpeed:F2}MHz ({_ctInfo.GetFSBSpeed:F2} x {_ctInfo.GetMultiplier})");
                sb.AppendLine($"CPU VID: {_ctInfo.GetVID:F4}v");
                sb.AppendLine($"Reported Physical CPUs: {_ctInfo.GetCPUCount}"); // 明确是“报告的”数量
                sb.AppendLine($"Reported Cores per CPU: {_ctInfo.GetCoreCount}");
                sb.AppendLine("--------------------");

                for (uint i = 0; i < _ctInfo.GetCPUCount; i++)
                {
                    sb.AppendLine($"CPU #{i}");
                    sb.AppendLine($"  Tj.Max: {_ctInfo.GetTjMax[i]}°{tempType}");
                    for (uint g = 0; g < _ctInfo.GetCoreCount; g++)
                    {
                        uint index = g + (i * _ctInfo.GetCoreCount);
                        // 安全检查
                        if (index < _ctInfo.GetTemp.Length && index < _ctInfo.GetCoreLoad.Length)
                        {
                            if (_ctInfo.IsDistanceToTjMax)
                            {
                                sb.AppendLine($"  Core #{g} (Abs #{index}): {_ctInfo.GetTemp[index]}°{tempType} to TjMax, {_ctInfo.GetCoreLoad[index]}% Load");
                            }
                            else
                            {
                                sb.AppendLine($"  Core #{g} (Abs #{index}): {_ctInfo.GetTemp[index]}°{tempType}, {_ctInfo.GetCoreLoad[index]}% Load");
                            }
                        }
                    }
                }
                return sb.ToString();
            }
            else
            {
                return $"Failed to read Core Temp data.\n" +
                       "Is Core Temp running with administrator privileges?\n\n" +
                       $"Error Code: {_ctInfo.GetLastError}\n" +
                       $"Error Message: {_ctInfo.GetErrorMessage(_ctInfo.GetLastError)}";
            }
        }
    }
}