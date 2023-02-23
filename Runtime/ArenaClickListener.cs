using ArenaUnity;
using UnityEngine;

public class ArenaClickListener : MonoBehaviour
{
    private Camera _mainCamera;

    private Renderer _renderer;

    private Ray _ray;
    private RaycastHit _hit;

    private void Start()
    {
        _mainCamera = Camera.main;
        _renderer = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //_ray = new Ray(
            //_mainCamera.ScreenToWorldPoint(Input.mousePosition),
            //_mainCamera.transform.forward);
            // or:
            _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(_ray, out _hit))
            {
                if (_hit.transform == transform)
                {
                    Debug.Log($"Local Click {name} (Raycast)!");
                    ChangeColorAndPublish();
                }
            }
        }
    }

    internal void OnMouseDown()
    {
        Debug.Log($"Local Click {name} (OnMouseDown)!");
        ChangeColorAndPublish();
    }

    internal void ExternalMouseDown(dynamic data)
    {
        Debug.Log($"Remote Click {name}!");
        ChangeColorAndPublish();
    }

    private void ChangeColorAndPublish()
    {
        if (_renderer)
        {
            _renderer.material.color =
                _renderer.material.color == Color.red ? Color.blue : Color.red;
        }

        var aobj = GetComponent<ArenaObject>();
        if (aobj != null) aobj.PublishCreateUpdate();
    }
}
