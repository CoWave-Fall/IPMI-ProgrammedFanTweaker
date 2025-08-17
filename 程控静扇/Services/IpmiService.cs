using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace 程控静扇.Services;

public static class IpmiService
{
    public static event EventHandler? CommunicationStarted;
    public static event EventHandler<CommunicationCompletedEventArgs>? CommunicationCompleted;

    private static readonly string IpmiToolPath = Path.Combine(AppContext.BaseDirectory, "ipmitool", "ipmitool.exe");

    public static Task<string> SetManualFanControl(IpmiCredentials creds)
    {
        string args = BuildBaseCommand(creds) + " raw 0x30 0x30 0x01 0x00";
        return RunIpmiToolCommand(args);
    }

    public static Task<string> SetAutomaticFanControl(IpmiCredentials creds)
    {
        string args = BuildBaseCommand(creds) + " raw 0x30 0x30 0x01 0x01";
        return RunIpmiToolCommand(args);
    }

    public static Task<string> SetFanSpeed(IpmiCredentials creds, int speedPercentage)
    {
        string speedHex = speedPercentage.ToString("X2");
        string args = BuildBaseCommand(creds) + $" raw 0x30 0x30 0x02 0xff 0x{speedHex}";
        return RunIpmiToolCommand(args);
    }

    public static Task<string> GetSensorData(IpmiCredentials creds)
    {
        string args = BuildBaseCommand(creds) + " sensor";
        return RunIpmiToolCommand(args);
    }

    private static string BuildBaseCommand(IpmiCredentials creds)
    {
        return $"-I lanplus -H {creds.IpAddress} -U {creds.UserName} -P \"{creds.Password}\"" ;
    }

    private static async Task<string> RunIpmiToolCommand(string arguments)
    {
        CommunicationStarted?.Invoke(null, EventArgs.Empty); // Trigger CommunicationStarted

        if (!File.Exists(IpmiToolPath))
        {
            StatusService.Instance.IpmiStatus = IpmiOperationStatus.Failure; // Update IPMI status
            SettingsService.SaveIpmiStatus(StatusService.Instance.IpmiStatus); // Save IPMI status
            CommunicationCompleted?.Invoke(null, new CommunicationCompletedEventArgs(false)); // Indicate failure
            return $"Error: ipmitool.exe not found at {IpmiToolPath}.";
        }

        using var process = new Process();
        process.StartInfo.FileName = IpmiToolPath;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
        process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

        try
        {
            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(process.WaitForExitAsync(), outputTask, errorTask);

            string? error = await errorTask;
            string? output = await outputTask;

            bool isSuccess = process.ExitCode == 0;
            StatusService.Instance.IpmiStatus = isSuccess ? IpmiOperationStatus.Success : IpmiOperationStatus.Failure; // Update IPMI status
            SettingsService.SaveIpmiStatus(StatusService.Instance.IpmiStatus); // Save IPMI status
            CommunicationCompleted?.Invoke(null, new CommunicationCompletedEventArgs(isSuccess)); // Trigger CommunicationCompleted

            if (isSuccess)
            {
                LogService.Instance.AppendLog($"Command Output:\n{output}");
                return output;
            }
            else
            {
                LogService.Instance.AppendLog($"Command Error (Exit Code: {process.ExitCode}):\n{error}\n{output}");
                return $"Error (Exit Code: {process.ExitCode}):\n{error}\n{output}";
            }
        }
        catch (Exception ex)
        {
            StatusService.Instance.IpmiStatus = IpmiOperationStatus.Failure; // Update IPMI status
            SettingsService.SaveIpmiStatus(StatusService.Instance.IpmiStatus); // Save IPMI status
            CommunicationCompleted?.Invoke(null, new CommunicationCompletedEventArgs(false)); // Indicate failure
            LogService.Instance.AppendLog($"Exception during command execution: {ex.Message}");
            return $"Exception during command execution: {ex.Message}";
        }
    }
}
