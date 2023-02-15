using ArenaUnity;
using UnityEngine;

public class OnMouseDownExample : MonoBehaviour
{
    private Renderer _renderer;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    internal void OnMouseDown()
    {
        Debug.Log("Click!");
        _renderer.material.color =
            _renderer.material.color == Color.red ? Color.blue : Color.red;

        var aobj = GetComponent<ArenaObject>();
        if (aobj != null) aobj.PublishCreateUpdate();
    }
}