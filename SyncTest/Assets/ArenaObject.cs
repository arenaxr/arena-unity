using System;
using Newtonsoft.Json;
using UnityEngine;

[HelpURL("https://arena.conix.io/content/messaging/definitions.html")]
public class ArenaObject : MonoBehaviour
{
    [Tooltip("A uuid or otherwise unique identifier for this object")]
    public string objectId = Guid.NewGuid().ToString();
    [Tooltip("Persist this object in the ARENA server database (default false = do not persist)")]
    public bool persist = true;
    //[Tooltip("Time-to-live seconds to create the object and automatically delete (default: 0)")]
    //public Int16 ttl = 0;
    [TextArea(10, 20)]
    [Tooltip("ARENA JSON-encoded message (debug only for now)")]
    public string arenaJson = "";
    [HideInInspector]
    public string parentId = null;
    private int updateInterval = 10; // in frames
    private bool created = false;

    //public ArenaObject(string objectId, bool persist)
    //{
    //    this.objectId = objectId;
    //    this.persist = persist;
    //}

    private class ObjectMessage
    {
        public string object_id { get; set; }
        public string action { get; set; }
        public string type { get; set; }
        public bool persist { get; set; }
        public ObjectData data { get; set; }
    }

    private class ObjectData
    {
        public string object_type { get; set; }
        public Object3D position { get; set; }
        public Object3D rotation { get; set; }
        public Object3D scale { get; set; }
        public string color { get; set; }
    }

    private class Object3D
    {
        public string x { get; set; }
        public string y { get; set; }
        public string z { get; set; }
    }

    private class ObjectQuaternion
    {
        public string w { get; set; }
        public string x { get; set; }
        public string y { get; set; }
        public string z { get; set; }
    }

    Object3D ToArenaScale(string object_type, Vector3 scale)
    {
        // Scale Conversions
        // cube: unity (side) 1, a-frame (side)  1
        // sphere: unity (diameter) 1, a-frame (radius)  0.5
        // cylinder: unity (y height) 1, a-frame (y height) 2
        // cylinder: unity (x,z diameter) 1, a-frame (x,z radius) 0.5

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

        String objectType = "";
        if (GetComponent<MeshFilter>())
        {
            objectType = GetComponent<MeshFilter>().sharedMesh.name.ToLower();
        }
        ObjectMessage msg = new ObjectMessage
        {
            object_id = this.objectId,
            action = this.created ? "update" : "create",
            type = "object",
            persist = this.persist,
            data = new ObjectData
            {
                object_type = objectType,
                position = new Object3D
                {
                    // Position Conversions:
                    // all: z is inverted in a-frame

                    x = transform.position.x.ToString(),
                    y = transform.position.y.ToString(),
                    z = (-transform.position.z).ToString()
                },
                rotation = new Object3D
                {
                    // TODO: quaternions are more direct, but a-frame doesn't like them somehow
                    x = (-transform.rotation.eulerAngles.x).ToString(),
                    y = (-transform.rotation.eulerAngles.y).ToString(),
                    z = (transform.rotation.eulerAngles.z).ToString()
                },
                scale = ToArenaScale(objectType.ToLower(), transform.localScale),
            }
        };
        if (GetComponent<Renderer>())
        {
            Color color = GetComponent<Renderer>().material.GetColor("_Color");
            msg.data.color = $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
        string payload = JsonConvert.SerializeObject(msg);
        // ArenaClient.Instance.Publish(msg.object_id, payload);
        if (!this.created)
            this.created = true;

        return true;
    }

    public void OnDestroy()
    {
        if (Application.isEditor)
        {
            if (ArenaClient.Instance == null || !ArenaClient.Instance.mqttClientConnected)
                return;

            ObjectMessage msg = new ObjectMessage
            {
                object_id = this.objectId,
                action = "delete",
            };
            string payload = JsonConvert.SerializeObject(msg);
            // ArenaClient.Instance.Publish(msg.object_id, payload);
        }
    }
}
