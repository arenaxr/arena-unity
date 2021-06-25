using System;
using Newtonsoft.Json;
using UnityEngine;

[HelpURL("https://arena.conix.io/content/messaging/definitions.html")]
public class SyncedObject : MonoBehaviour
{
    private class ObjectMessage
    {
        public string object_id { get; set; }
        public string action { get; set; }
        public string type { get; set; }
        public ObjectData data { get; set; }
    }

    private class ObjectData
    {
        public string object_type { get; set; }
        public Object3D position { get; set; }
        // public Object3D rotation { get; set; }
        // public Object3D scale { get; set; }
    }

    private class Object3D
    {
        public string x { get; set; }
        public string y { get; set; }
        public string z { get; set; }
    }

    public static Guid ToGuid(int value)
    {
        byte[] bytes = new byte[16];
        BitConverter.GetBytes(value).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    void Start()
    {
        InvokeRepeating("SendPosition", 1.0f, 0.1f);
        // Every 0.01 seconds is 100fps -- that we're sending at, that is
    }

    void SendPosition()
    {
        if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
            return;

        // For now we just send:
        // {"x": "1", "y": "1", "z": "1"}
        ObjectMessage msg = new ObjectMessage
        {
            object_id = ToGuid(GetInstanceID()).ToString(),
            action = "update",
            type = "object",
            data = new ObjectData
            {
                object_type = "box",
                position = new Object3D
                {
                    x = transform.position.x.ToString(),
                    y = transform.position.y.ToString(),
                    z = transform.position.z.ToString()
                }
            }
        };
        string payload = JsonConvert.SerializeObject(msg);
        Debug.Log(payload);
        ArenaClient.Instance.Publish(msg.object_id, payload);
    }
}
