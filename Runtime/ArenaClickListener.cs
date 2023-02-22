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
            Debug.Log("Input.mousePosition " + Input.mousePosition.ToString());

            //_ray = new Ray(
            //_mainCamera.ScreenToWorldPoint(Input.mousePosition),
            //_mainCamera.transform.forward);
            // or:
            _ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            Debug.Log("_ray " + _ray.ToString());

            if (Physics.Raycast(_ray, out _hit))
            {
                Debug.Log("_hit " + _hit.ToString());

                if (_hit.transform == transform)
                {
                    Debug.Log("Local Click (Raycast)!");
                    ChangeColorAndPublish();
                }
            }
        }
    }

    internal void OnMouseDown()
    {
        Debug.Log("Local Click (OnMouseDown)!");
        ChangeColorAndPublish();
    }

    internal void ExternalMouseDown(dynamic data)
    {
        Debug.Log("Remote Click!");
        ChangeColorAndPublish();
    }

    private void ChangeColorAndPublish()
    {
        _renderer.material.color =
            _renderer.material.color == Color.red ? Color.blue : Color.red;

        var aobj = GetComponent<ArenaObject>();
        if (aobj != null) aobj.PublishCreateUpdate();
    }
}
