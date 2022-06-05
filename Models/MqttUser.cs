using System.ComponentModel.DataAnnotations;

namespace Models;

public class MqttUser {
    [Key]
    public uint id { get; set; }
    public string username { get; set; } = "";
    public string password { get; set; } = "";
    public bool is_superuser { get; set; } = false;
    public string token { get; set; } = "";
}