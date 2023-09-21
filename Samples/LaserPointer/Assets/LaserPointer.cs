using System.Collections;
using ArenaUnity;
using ArenaUnity.Components;
using ArenaUnity.Schemas;
using Newtonsoft.Json;
using UnityEngine;

public class LaserPointer : MonoBehaviour
{
    private ArenaClientScene _scene;
    private Color32 _laserColor = new Color32(255, 0, 0, 255);
    private Vector3 _targetScale = new Vector3(.06f, .06f, .06f);

    public bool useThickline = true;

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
    private void MouseEventCallback(string event_type, string message)
    {
        if (event_type != "mousedown") return;
        int instance = UnityEngine.Random.Range(0, 100000000);

        ArenaObjectJson m = JsonConvert.DeserializeObject<ArenaObjectJson>(message);
        ArenaEventJson evt = JsonConvert.DeserializeObject<ArenaEventJson>(m.data.ToString());

        ArenaPositionJson start = JsonConvert.DeserializeObject<ArenaPositionJson>(evt.ClickPos.ToString());
        ArenaPositionJson end = JsonConvert.DeserializeObject<ArenaPositionJson>(evt.Position.ToString());
        start.Y = (float)start.Y - .1f; // lower position for visibility

        // laser
        ArenaObjectJson msg = new ArenaObjectJson
        {
            object_id = $"line-{instance}",
            action = "create",
            type = "object",
            ttl = 1,
        };
        string payload;
        if (useThickline)
        {
            var data = new ArenaThicklineJson();
            string startTL = $"{start.X} {start.Y} {start.Z}";
            string endTL = $"{end.X} {end.Y} {end.Z}";
            data.Path = $"{startTL},{endTL}";
            data.LineWidth = 5;
            data.Color = ArenaUnity.ArenaUnity.ToArenaColor(_laserColor);
            msg.data = data;
            payload = JsonConvert.SerializeObject(msg);
        }
        else
        {
            var data = new ArenaLineJson();
            data.Start = start;
            data.End = end;
            data.Color = ArenaUnity.ArenaUnity.ToArenaColor(_laserColor);
            msg.data = data;
            payload = JsonConvert.SerializeObject(msg);
        }
        _scene.PublishObject(msg.object_id, payload);
        // target
        msg = new ArenaObjectJson
        {
            object_id = $"target-{instance}",
            action = "create",
            type = "object",
            ttl = 1,
        };
        var data1 = new ArenaObjectDataJson {
            object_type = "sphere",
            position = end,
            scale = ArenaUnity.ArenaUnity.ToArenaScale(_targetScale),
            material = new ArenaMaterialJson {
                Color = ArenaUnity.ArenaUnity.ToArenaColor(_laserColor),
            },
        };
        msg.data = data1;
        payload = JsonConvert.SerializeObject(msg);
        _scene.PublishObject(msg.object_id, payload);
    }
}
