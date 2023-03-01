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

    void Awake()
    {
        //_scene = ArenaClientScene.Instance;
        //if (!_scene) gameObject.AddComponent<ArenaClientScene>();
        //_scene.hostAddress = "arena-dev1.conix.io";
        //_scene.namespaceName = "mwfarb";
        //_scene.sceneName = "example5";
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunProgram());
    }

    IEnumerator RunProgram()
    {
        _scene = ArenaClientScene.Instance;
        //_scene.ConnectArena();
        //yield return new WaitUntil(() => _scene.mqttClientConnected);
        //yield return new WaitUntil(() => _scene.persistLoaded);
        //if (_scene.arenaObjs == null) yield break;
        //yield return new WaitUntil(() => _scene.arenaObjs.Count >= 3);
        yield return new WaitForSeconds(5);

        // find clickable arena objects

        // TODO: find balance of ACL attached complete, or in progress

        foreach (var aobj in _scene.arenaObjs.Values)
        {
            // attach callbacks
            ArenaClickListener acl = aobj.GetComponent<ArenaClickListener>();
            if (acl)
            {
                acl.OnEventCallback = MyMouseDown;
                Debug.Log($"Click callback attached to {aobj.name}");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (!_scene) _scene = ArenaClientScene.Instance;
    }

    /// <summary>
    /// A delegate method used as a callback to go some special handling on incoming messages.
    /// </summary>
    private void MyMouseDown(string event_type, dynamic data)
    {
        Debug.Log($"Remote Click '{JsonComvert.Deserialize(data)}' ({event_type})!");

        if (event_type != "mousedown") return;
        int instance = UnityEngine.Random.Range(0, 100000000);

        //laser
        string line_id = $"line-{instance}";
        dynamic msg = new ExpandoObject();
        msg.object_id = line_id;
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
        //ball
        string target_id = $"target-{instance}";
        msg = new ExpandoObject();
        msg.object_id = target_id;
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
