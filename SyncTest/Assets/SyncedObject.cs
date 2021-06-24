using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncedObject : MonoBehaviour
{
  public ArenaClient scene;

  void Start()
  {
    //InvokeRepeating("SendPosition", 0.1f, 0.01f);
    // Every 0.01 seconds is 100fps -- that we're sending at, that is
  }

  void SendPosition()
  {
    // For now we just send:
    // {"x": "1", "y": "1", "z": "1"}
    string s = "{\"x\": \"" + transform.position.x.ToString() + "\", \"y\": \"" + transform.position.y.ToString() + "\", \"z\": \"" + transform.position.z.ToString() + "\"}";
    //scene.sendString(s);
  }
}
