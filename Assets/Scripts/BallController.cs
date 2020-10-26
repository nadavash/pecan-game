using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {
    public float MovementSpeed = 0.1f;

    void FixedUpdate() {
        if (Input.GetKey(KeyCode.UpArrow)) {
            transform.position += new Vector3(0, MovementSpeed, 0);
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            transform.position += new Vector3(0, -MovementSpeed, 0);
        }

        if (Input.GetKey(KeyCode.RightArrow)) {
            transform.position += new Vector3(MovementSpeed, 0, 0);
        } else if (Input.GetKey(KeyCode.LeftArrow)) {
            transform.position += new Vector3(-MovementSpeed, 0, 0);
        }
    }

    public void MoveBallUp() {
        transform.position += Vector3.up * MovementSpeed;
    }
}
