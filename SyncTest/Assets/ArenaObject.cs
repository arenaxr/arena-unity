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
    [TextArea(10, 15)]
    [Tooltip("ARENA JSON-encoded message (debug only for now)")]
    public string jsonData = null;
    [HideInInspector]
    public dynamic data = null;
    [HideInInspector]
    public string parentId = null;
    private bool created = false;

    public string GetObjectType()
    {
        string objectType = "entity";
        if (GetComponent<MeshFilter>())
        {
            objectType = GetComponent<MeshFilter>().sharedMesh.name.ToLower();
        }
        return objectType.ToLower();
    }

    dynamic ToArenaScale(string object_type, Vector3 scale)
    {
        // Scale Conversions
        // cube: unity (side) 1, a-frame (side)  1
        // sphere: unity (diameter) 1, a-frame (radius)  0.5
        // cylinder: unity (y height) 1, a-frame (y height) 2
        // cylinder: unity (x,z diameter) 1, a-frame (x,z radius) 0.5

        switch (object_type)
        {
            // case "sphere":
            //     return new
            //     {
            //         x = (scale.x * 0.5),
            //         y = (scale.y * 0.5),
            //         z = (scale.z * 0.5)
            //     };
            // case "cylinder":
            //     return new
            //     {
            //         x = (scale.x * 0.5),
            //         y = (scale.y * 2),
            //         z = (scale.z * 0.5)
            //     };
            // case "cube":
            default:
                return new
                {
                    x = scale.x,
                    y = scale.y,
                    z = scale.z
                };
        }
    }

    void Start()
    {
        transform.hasChanged = false;

    }

    void Update()
    {
        // send only when changed, each 10 frames or so
        if (!ArenaClient.Instance || Time.frameCount % ArenaClient.Instance.updateInterval != 0)
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

        dynamic msg = new System.Dynamic.ExpandoObject();
        msg.object_id = this.objectId;
        msg.action = this.created ? "update" : "create";
        msg.type = "object";
        msg.persist = this.persist;
        dynamic data = new System.Dynamic.ExpandoObject();
        data.object_type = GetObjectType();
        data.position = new
        {
            // Position Conversions:
            // all: z is inverted in a-frame
            x = transform.position.x,
            y = transform.position.y,
            //z = transform.position.z
            z = -transform.position.z
        };
        data.rotation = new
        {
            // TODO: quaternions are more direct, but a-frame doesn't like them somehow
            x = transform.rotation.eulerAngles.x,
            //x = -transform.rotation.eulerAngles.x,
            y = transform.rotation.eulerAngles.y,
            //y = -transform.rotation.eulerAngles.y,
            z = transform.rotation.eulerAngles.z
        };
        data.scale = ToArenaScale(data.object_type, transform.localScale);
        if (GetComponent<Renderer>())
        {
            Color color = GetComponent<Renderer>().material.GetColor("_Color");
            data.color = $"#{ColorUtility.ToHtmlStringRGB(color)}";
        }
        msg.data = data;
        jsonData = data.ToString();
        string payload = JsonConvert.SerializeObject(msg);
        ArenaClient.Instance.Publish(msg.object_id, payload);
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

            dynamic msg = new
            {
                object_id = this.objectId,
                action = "delete",
            };
            string payload = JsonConvert.SerializeObject(msg);
            ArenaClient.Instance.Publish(msg.object_id, payload);
        }
    }
}
