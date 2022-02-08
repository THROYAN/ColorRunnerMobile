using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public bool autoRun = true;
    public Player player;
    
    private Vector2 movementDirection;

    // Start is called before the first frame update
    void OnValidate()
    {
        if (player == null) {
            player = FindObjectOfType<Player>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) {
            return;
        }

        movementDirection.x = 0;
        movementDirection.y = 0;

        if (autoRun || Input.GetKey(KeyCode.W)) {
            movementDirection.y += 1;
        }

        if (Input.GetKey(KeyCode.A)) {
            movementDirection.x -= 1;
        }

        if (Input.GetKey(KeyCode.D)) {
            movementDirection.x += 1;
        }

        if (movementDirection.sqrMagnitude < 0.1f) {
            return;
        }

        player.Move(movementDirection);
    }
}
