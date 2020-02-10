using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseControls : MonoBehaviour
{
    private RaycastHit hit;
    public GameObject gameCam;
    private Camera cam;

    void Start()
    {
        cam = gameCam.GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow, 3.0f);
            if(Physics.Raycast(ray, out hit, 1000.0f, 9))
            {
                hit.collider.GetComponent<SpriteRenderer>().material.SetColor("_Color", Color.red);
            }
        }
    }

}
