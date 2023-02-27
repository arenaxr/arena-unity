using System.Collections;
using System.Collections.Generic;
using ArenaUnity;
using Newtonsoft.Json;
using System.Dynamic;
using UnityEngine;
using System;
using ArenaUnity.Components;
using Packages.Rider.Editor.UnitTesting;

public class LaserPointer : MonoBehaviour
{
    private ArenaClientScene _scene;


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

        foreach (var aobj in _scene.arenaObjs.Values)
        {
            // attach callbacks
            ArenaClickListener acl = aobj.GetComponent<ArenaClickListener>();
            if (acl)
            {
                acl.OnEventCallback = MyMouseDown;
                Debug.Log($"Click callback attched to {aobj.name}");
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
        Debug.Log($"Remote Click {data.object_id} ({event_type})!");

        //{"object_id":"box","action":"clientEvent","type":"mousedown","data":{"clickPos":{"x":-2.87,"y":1.6,"z":6.225},"position":{"x":-0.195,"y":0.305,"z":1.913},"source":"camera_2418540601_mwfarb"},"timestamp":"2023-02-15T18:59:02.413Z"}
        if (event_type != "mousedown") return;

        //laser
        string line_id = $"line-{UnityEngine.Random.Range(0, 100000000)}";
        dynamic msg = new ExpandoObject();
        msg.object_id = line_id;
        msg.action = "create";
        msg.type = "object";
        msg.ttl = 1;
        msg.data = new ExpandoObject();
        msg.data.object_type = "thickline";
        string start = $"{data.position.x} {(float)data.position.y - .1f} {data.position.z}";
        string end = $"{data.clickPos.x} {data.clickPos.y} {data.clickPos.z}";
        msg.data.path = $"{start},{end}";
        msg.data.color = ArenaUnity.ArenaUnity.ToArenaColor(new Color32(255, 0, 0, 255));
        msg.data.lineWidth = 5;
        string payload = JsonConvert.SerializeObject(msg);
        _scene.PublishObject(msg.object_id, payload); // remote
        //ball
        string ball_id = $"ball-{UnityEngine.Random.Range(0, 100000000)}";
        msg = new ExpandoObject();
        msg.object_id = ball_id;
        msg.action = "create";
        msg.type = "object";
        msg.ttl = 1;
        msg.data = new ExpandoObject();
        msg.data.object_type = "sphere";
        msg.data.position = new ExpandoObject();
        msg.data.position.x = (float)data.clickPos.x;
        msg.data.position.y = (float)data.clickPos.y;
        msg.data.position.z = (float)data.clickPos.z;
        msg.data.scale = ArenaUnity.ArenaUnity.ToArenaScale(new Vector3(.06f, .06f, .06f));
        msg.data.color = ArenaUnity.ArenaUnity.ToArenaColor(new Color32(255, 0, 0, 255));
        msg.data.lineWidth = 5;
        payload = JsonConvert.SerializeObject(msg);
        _scene.PublishObject(msg.object_id, payload); // remote
    }

}
