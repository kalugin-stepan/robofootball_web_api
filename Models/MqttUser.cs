namespace Models;

public class MqttUser {
    public uint id { get; set; }
    public string username { get; set; } = "";
    public string password { get; set; } = "";
    public bool is_superuser { get; set; } = false;
}