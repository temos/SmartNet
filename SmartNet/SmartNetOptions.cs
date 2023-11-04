namespace SmartNet;
public class SmartNetOptions
{
    public string WebHookPath { get; set; }
    public string AppId { get; set; }
    public string AppName { get; set; }
    public string AppDescription { get; set; }
    public List<string> Permissions { get; } = new List<string>();
}
