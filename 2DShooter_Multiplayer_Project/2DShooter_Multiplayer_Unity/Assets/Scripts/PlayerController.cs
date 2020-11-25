using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //we will send input to server and then the server will calculate the servers new position and will send it to all clients
    //we dont want clients sending positions to server because of anti cheat

    public Camera cam;
    Vector3 mousePos;
    //Vector2 mousePos;
    public Rigidbody rb;
    public PlayerManager player;


    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {

        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;



        Debug.DrawRay(transform.position, transform.up * 2, Color.red);
        Vector3 cursorInWorldPos = cam.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);
        Debug.Log(cursorInWorldPos);
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(cursorInWorldPos);
        }

        mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
    }

    private void FixedUpdate() //Project settings -> Time -> Fixed Timesteps to 0.03 to match with the ticks, no point in sending input more often
    {
        SendInputToServer();
        //Vector2 lookDir = mousePos - rb.position;

        Vector3 lookDir = mousePos - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f; //- or + 90?
        player.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void SendInputToServer()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),

        };

        ClientSend.PlayerMovement(_inputs);
    }
}
