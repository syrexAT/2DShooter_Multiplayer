using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //we will send input to server and then the server will calculate the servers new position and will send it to all clients
    //we dont want clients sending positions to server because of anti cheat


    private void Start()
    {

    }

    private void FixedUpdate() //Project settings -> Time -> Fixed Timesteps to 0.03 to match with the ticks, no point in sending input more often
    {
        SendInputToServer();
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
