namespace Models;

public enum Access {
    subscribe,
    publish,
    pubsub
}

public class MqttAcl {
    public uint id { get; set; }
    public string username { get; set; } = "";
    public Access access { get; set; } = Access.pubsub;
    public string topic { get; set; } = "";
}