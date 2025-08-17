namespace 程控静扇.ViewModels;

public enum FanHealth
{
    Ok,
    Warning,
    Critical
}

public class FanStatusViewModel
{
    public string Name { get; set; } = string.Empty;
    public double Rpm { get; set; }
    public FanHealth Health { get; set; }
}
