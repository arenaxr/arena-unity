using System;
using Newtonsoft.Json;
using UnityEngine;

[HelpURL("https://arena.conix.io/content/messaging/definitions.html")]
public class SyncedObject : MonoBehaviour
{
    private int updateInterval = 10; // in frames

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
        transform.hasChanged = true;
        //InvokeRepeating("SendPosition", 0.1f, 0.01f);
        // Every 0.01 seconds is 100fps -- that we're sending at, that is
    }

    void Update()
    {
        // send only when changed, each 10 frames or so
        if (Time.frameCount % this.updateInterval != 0)
          return;

        if (transform.hasChanged)
        {
            SendPosition();
            transform.hasChanged = false;
        }
    }

    void SendPosition()
    {
        if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
            return;

        // For now we just send:
        // {"x": "1", "y": "1", "z": "1"}
        MeshFilter mesh = GetComponent<MeshFilter>();
        ObjectMessage msg = new ObjectMessage
        {
            object_id = ToGuid(GetInstanceID()).ToString(),
            action = "update",
            type = "object",
            data = new ObjectData
            {
                object_type = mesh.sharedMesh.name.ToLower(),
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
