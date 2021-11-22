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
        dynamic dataUp = new System.Dynamic.ExpandoObject();
        dataUp.object_type = GetObjectType();
        dataUp.position = ArenaUnity.ToArenaPosition(transform.position);
        if (data.rotation != null && data.rotation.w != null)
            dataUp.rotation = ArenaUnity.ToArenaRotationQuat(transform.rotation);
        else
            dataUp.rotation = ArenaUnity.ToArenaRotationEuler(transform.rotation.eulerAngles);
        dataUp.scale = ArenaUnity.ToArenaScale(dataUp.object_type, transform.localScale);
        if (GetComponent<Renderer>())
        {
            Color color = GetComponent<Renderer>().material.GetColor("_Color");
            dataUp.color = ArenaUnity.ToArenaColor(color);
        }
        data = dataUp;
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
