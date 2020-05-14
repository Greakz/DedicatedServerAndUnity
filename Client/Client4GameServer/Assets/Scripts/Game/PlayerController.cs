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
            Input.GetKey(KeyCode.W) && Application.isFocused,
            Input.GetKey(KeyCode.S) && Application.isFocused,
            Input.GetKey(KeyCode.A) && Application.isFocused,
            Input.GetKey(KeyCode.D) && Application.isFocused
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
