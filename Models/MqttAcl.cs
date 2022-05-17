namespace Models;

public enum Access {
    subscribe = 1,
    publish = 2,
    pubsub = 3
}

public class MqttAcl {
    public uint id { get; set; }
    public uint user_id { get;set; }
    public string username { get; set; } = "";
    public Access access { get; set; } = Access.pubsub;
    public string topic { get; set; } = "";
}