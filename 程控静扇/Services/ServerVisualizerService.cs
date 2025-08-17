//
// Services/ServerVisualizerService.cs
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions; // 引入正则表达式命名空间
using System.Threading.Tasks;
using System.Xml.Linq;

namespace 程控静扇.Services
{
    public class ServerVisualizerService
    {
        private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";
        private static readonly XNamespace InkscapeNs = "http://www.inkscape.org/namespaces/inkscape";

        public Dictionary<string, double> ParseSensorDataForVisualization(string sensorData)
        {
            var values = new Dictionary<string, double>();
            var lines = sensorData.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var tempLines = lines.Where(l => l.Trim().StartsWith("Temp")).ToList();
            if (tempLines.Count >= 2)
            {
                if (double.TryParse(tempLines[tempLines.Count - 2].Split('|')[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double cpu1Temp))
                    values["CPU1"] = cpu1Temp;
                if (double.TryParse(tempLines[tempLines.Count - 1].Split('|')[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out double cpu2Temp))
                    values["CPU2"] = cpu2Temp;
            }

            var fanLines = lines.Where(l => l.Trim().StartsWith("Fan")).ToList();
            foreach (var line in fanLines)
            {
                var parts = line.Split('|').Select(p => p.Trim()).ToArray();
                if (parts.Length > 1 && double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double rpm))
                {
                    var fanName = parts[0].Replace(" ", "");
                    values[fanName] = rpm;
                }
            }
            return values;
        }

        public string GetColorForTemperature(double temperature)
        {
            temperature = Math.Max(0, Math.Min(100, temperature));
            int r, g, b;
            if (temperature <= 50)
            {
                double factor = temperature / 50.0;
                r = 0; g = (int)(255 * factor); b = (int)(255 * (1 - factor));
            }
            else
            {
                double factor = (temperature - 50) / 50.0;
                r = (int)(255 * factor); g = (int)(255 * (1 - factor)); b = 0;
            }
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public string GetColorForFanRpm(double rpm)
        {
            const double minRpm = 2000.0;
            const double maxRpm = 15000.0;
            rpm = Math.Max(minRpm, Math.Min(maxRpm, rpm));
            double normalizedRpm = (rpm - minRpm) / (maxRpm - minRpm);

            int r, g, b;
            if (normalizedRpm <= 0.5)
            {
                double factor = normalizedRpm * 2;
                r = 255; g = (int)(255 * factor); b = 0;
            }
            else
            {
                double factor = (normalizedRpm - 0.5) * 2;
                r = (int)(255 * (1 - factor)); g = 255; b = 0;
            }
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public Task<string> UpdateSvgAsync(string baseSvgContent, Dictionary<string, double> sensorValues)
        {
            return Task.Run(() =>
            {
                var doc = XDocument.Parse(baseSvgContent);
                string defaultFill = "#66ccff";
                string defaultOpacity = "0.4";

                var otherComponentIds = new List<string> { "ram_modules", "psu_area", "pcie_risers" };
                foreach (var id in otherComponentIds)
                {
                    var group = doc.Descendants(SvgNs + "g").FirstOrDefault(g => g.Attribute("id")?.Value == id);
                    group?.Descendants().ToList().ForEach(shape =>
                    {
                        if (new[] { "rect", "path", "circle" }.Contains(shape.Name.LocalName))
                        {
                            shape.SetAttributeValue("fill", defaultFill);
                            shape.SetAttributeValue("fill-opacity", defaultOpacity);
                        }
                    });
                }

                foreach (var item in sensorValues)
                {
                    string color;
                    string targetLabel = item.Key;

                    if (targetLabel.StartsWith("CPU"))
                    {
                        color = GetColorForTemperature(item.Value);
                        var cpuGroup = doc.Descendants(SvgNs + "g").FirstOrDefault(g => g.Attribute(InkscapeNs + "label")?.Value == targetLabel);
                        string rectId = (targetLabel == "CPU1") ? "rect20" : "rect18";
                        cpuGroup?.Descendants(SvgNs + "rect").FirstOrDefault(r => r.Attribute("id")?.Value == rectId)?.SetAttributeValue("fill", color);
                    }
                    else if (targetLabel.StartsWith("Fan"))
                    {
                        color = GetColorForFanRpm(item.Value);
                        var fanLabel = targetLabel.ToLower();
                        var fanGroup = doc.Descendants(SvgNs + "g").FirstOrDefault(g => g.Attribute(InkscapeNs + "label")?.Value == fanLabel);

                        if (fanGroup != null)
                        {
                            var fanCircle = fanGroup.Descendants(SvgNs + "circle").FirstOrDefault();
                            if (fanCircle != null)
                            {
                                // --- **关键修复** ---
                                // 获取当前的 style 属性值
                                string? currentStyle = fanCircle.Attribute("style")?.Value;
                                if (!string.IsNullOrEmpty(currentStyle))
                                {
                                    // 使用正则表达式替换 style 字符串中的 fill 颜色
                                    string newStyle = Regex.Replace(currentStyle, @"fill:#[0-9a-fA-F]{6}", $"fill:{color}");
                                    fanCircle.SetAttributeValue("style", newStyle);
                                }
                                else
                                {
                                    // 如果没有 style 属性，则作为备用方案直接设置 fill 属性
                                    fanCircle.SetAttributeValue("fill", color);
                                }
                                // --- **修复结束** ---
                            }
                        }
                    }
                }
                return doc.ToString();
            });
        }
    }
}