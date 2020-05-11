using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void FixedUpdate()
    {
        SendInputToServer();
    }

    private void SendInputToServer()
    {
        bool[] inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D)
        };

        if (inputs[0] || inputs[1] ||inputs[2] || inputs[3])
        {
            this.gameObject.GetComponent<Animator>().SetBool("walking", true);
        }
        else
        {
            this.gameObject.GetComponent<Animator>().SetBool("walking", false);
        }


        ClientSend.PlayerMovement(inputs);
    }
}
