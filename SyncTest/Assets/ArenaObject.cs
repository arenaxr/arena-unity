using System;
using Newtonsoft.Json;
using UnityEngine;

[HelpURL("https://arena.conix.io/content/messaging/definitions.html")]
public class ArenaObject : MonoBehaviour
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
        public ObjectQuat rotation { get; set; }
        public Object3D scale { get; set; }
        public string color { get; set; }
    }

    private class Object3D
    {
        public string x { get; set; }
        public string y { get; set; }
        public string z { get; set; }
    }

    private class ObjectQuat
    {
        public string w { get; set; }
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
            if (SendUpdateSuccess())
            {
                transform.hasChanged = false;
            }
        }
    }

    // Scale Conversions
    // cube: unity (side) 1, arena (side)  1
    // sphere: unity (diameter) 1, arena (radius)  0.5
    // cylinder: unity (y height) 1, arena (y height) 2
    // cylinder: unity (x,z diameter) 1, arena (x,z radius) 0.5

    // Position Conversions:
    // all: z is inverted in the arena

    bool SendUpdateSuccess()
    {
        if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
            return false;

        MeshFilter mesh = GetComponent<MeshFilter>();
        Color color = GetComponent<Renderer>().material.GetColor("_Color");

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
                    z = (-transform.position.z).ToString()
                },
                rotation = new ObjectQuat
                {
                    w = transform.rotation.w.ToString(),
                    x = transform.rotation.x.ToString(),
                    y = transform.rotation.y.ToString(),
                    z = transform.rotation.z.ToString()
                },
                scale = new Object3D
                {
                    x = transform.localScale.x.ToString(),
                    y = transform.localScale.y.ToString(),
                    z = transform.localScale.z.ToString()
                },
                color = $"#{ColorUtility.ToHtmlStringRGB(color)}"
            }
        };
        string payload = JsonConvert.SerializeObject(msg);
        Debug.Log(payload);
        ArenaClient.Instance.Publish(msg.object_id, payload);
        return true;
    }
}
