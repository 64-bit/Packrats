using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TestCameraController : MonoBehaviour
{
    public Camera Camera;

    public float RotationSpeed = 60.0f;
    public float VerticalSpeed = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        Camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessMove();
        ProcessScrollZoom();
    }

    private void ProcessScrollZoom()
    {
        float fov = Camera.fieldOfView;

        fov -= Input.mouseScrollDelta.y * 2.0f;
        fov = math.clamp(fov, 15.0f, 90.0f);

        Camera.fieldOfView = fov;
    }

    private void ProcessMove()
    {
        float2 move = float2.zero;

        if (Input.GetKey(KeyCode.A))
        {
            move.x -= RotationSpeed;
        }

        if (Input.GetKey(KeyCode.D))
        {
            move.x += RotationSpeed;
        }

        if (Input.GetKey(KeyCode.W))
        {
            move.y += VerticalSpeed;
        }

        if (Input.GetKey(KeyCode.S))
        {
            move.y -= VerticalSpeed;
        }

        move *= Time.deltaTime;

        transform.Rotate(0.0f, move.x, 0.0f);
        transform.position += new Vector3(0.0f, move.y, 0.0f);
    }
}
