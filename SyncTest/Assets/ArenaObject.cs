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

    Object3D ToArenaScale(string object_type, Vector3 scale)
    {
        // Scale Conversions
        // cube: unity (side) 1, arena (side)  1
        // sphere: unity (diameter) 1, arena (radius)  0.5
        // cylinder: unity (y height) 1, arena (y height) 2
        // cylinder: unity (x,z diameter) 1, arena (x,z radius) 0.5

        switch (object_type)
        {
            case "sphere":
                return new Object3D
                {
                    x = (scale.x * 0.5).ToString(),
                    y = (scale.y * 0.5).ToString(),
                    z = (scale.z * 0.5).ToString()
                };
            case "cylinder":
                return new Object3D
                {
                    x = (scale.x * 0.5).ToString(),
                    y = (scale.y * 2).ToString(),
                    z = (scale.z * 0.5).ToString()
                };
            case "cube":
            default:
                return new Object3D
                {
                    x = scale.x.ToString(),
                    y = scale.y.ToString(),
                    z = scale.z.ToString()
                };
        }
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

    bool SendUpdateSuccess()
    {
        if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
            return false;

        String objectType = GetComponent<MeshFilter>().sharedMesh.name;
        Color color = GetComponent<Renderer>().material.GetColor("_Color");

        ObjectMessage msg = new ObjectMessage
        {
            object_id = ToGuid(GetInstanceID()).ToString(),
            action = "update",
            type = "object",
            data = new ObjectData
            {
                object_type = objectType.ToLower(),
                position = new Object3D
                {
                    // Position Conversions:
                    // all: z is inverted in the arena

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
                scale = ToArenaScale(objectType.ToLower(), transform.localScale),
                color = $"#{ColorUtility.ToHtmlStringRGB(color)}"
            }
        };
        string payload = JsonConvert.SerializeObject(msg);
        Debug.Log(payload);
        ArenaClient.Instance.Publish(msg.object_id, payload);
        return true;
    }
}
