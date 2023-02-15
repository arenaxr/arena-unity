using ArenaUnity;
using UnityEngine;

public class RaycastClickExample : MonoBehaviour
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

            if (Physics.Raycast(_ray, out _hit, 1000f))
            {
                if (_hit.transform == transform)
                {
                    Debug.Log("Click");
                    _renderer.material.color =
                        _renderer.material.color == Color.red ? Color.blue : Color.red;

                    var aobj = GetComponent<ArenaObject>();
                    if (aobj != null) aobj.PublishCreateUpdate();
                }
            }
        }
    }
}