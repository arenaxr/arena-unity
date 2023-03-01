using System.Collections;
using System.Dynamic;
using ArenaUnity;
using ArenaUnity.Components;
using Newtonsoft.Json;
using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    private ArenaClientScene _scene;
    private Color32 _laserColor = new Color32(255, 0, 0, 255);
    private Vector3 _targetScale = new Vector3(.06f, .06f, .06f);

    void Start()
    {
        StartCoroutine(RunProgram());
    }

    IEnumerator RunProgram()
    {
        _scene = ArenaClientScene.Instance;
        yield return new WaitUntil(() => _scene.persistLoaded);

        // find clickable arena objects
        foreach (var aobj in _scene.arenaObjs.Values)
        {
            // attach callbacks
            ArenaClickListener acl = aobj.GetComponent<ArenaClickListener>();
            if (acl != null)
            {
                acl.OnEventCallback = MouseEventCallback;
                Debug.Log($"Laser: mouse callback attached to {aobj.name}");
            }
        }
    }

    /// <summary>
    /// A delegate method used as a callback to go some special handling on incoming messages.
    /// </summary>
    private void MouseEventCallback(string event_type, dynamic data)
    {
        if (event_type != "mousedown") return;
        int instance = UnityEngine.Random.Range(0, 100000000);

        // laser
        dynamic msg = new ExpandoObject();
        msg.object_id = $"line-{instance}";
        msg.action = "create";
        msg.type = "object";
        msg.ttl = 1;
        msg.data = new ExpandoObject();
        msg.data.object_type = "thickline";
        string start = $"{data.clickPos.x} {(float)data.clickPos.y - .1f} {data.clickPos.z}";
        string end = $"{data.position.x} {data.position.y} {data.position.z}";
        msg.data.path = $"{start},{end}";
        msg.data.color = ArenaUnity.ArenaUnity.ToArenaColor(_laserColor);
        msg.data.lineWidth = 5;
        string payload = JsonConvert.SerializeObject(msg);
        _scene.PublishObject(msg.object_id, payload);
        // target
        msg = new ExpandoObject();
        msg.object_id = $"target-{instance}"; ;
        msg.action = "create";
        msg.type = "object";
        msg.ttl = 1;
        msg.data = new ExpandoObject();
        msg.data.object_type = "sphere";
        msg.data.position = data.position;
        msg.data.scale = ArenaUnity.ArenaUnity.ToArenaScale(_targetScale);
        msg.data.color = ArenaUnity.ArenaUnity.ToArenaColor(_laserColor);
        msg.data.lineWidth = 5;
        payload = JsonConvert.SerializeObject(msg);
        _scene.PublishObject(msg.object_id, payload);
    }

}
