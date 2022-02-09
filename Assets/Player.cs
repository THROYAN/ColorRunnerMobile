using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int startLevel = 0;
    public int winLevel = 10;
    public float maxSpeed = 10.0f;
    public float acceleration = 1f;
    public bool clampMovement = false;
    public Animator animator;
    public Renderer[] bodyRenderers;

    private int currentLevel = 0;
    private float currentSpeed = 0;
    private Vector2 movementDirection = Vector2.zero;

    void OnValidate()
    {
        if (!animator) {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentLevel = startLevel;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentSpeed < 0.1f && movementDirection.sqrMagnitude < 0.1f) {
            animator.SetFloat("Speed", 0);

            return;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, movementDirection.magnitude * maxSpeed, acceleration * Time.deltaTime);

        if (currentSpeed < 0.1f) {
            animator.SetFloat("Speed", 0);

            return;
        }

        animator.SetFloat("Speed", currentSpeed);
        animator.SetBool("Left Turn", movementDirection.x < -0.1f);
        animator.SetBool("Right Turn", movementDirection.x > 0.1f);

        Vector3 direction3D = new Vector3(movementDirection.x, 0f, movementDirection.y);

        transform.position = Vector3.Lerp(transform.position, transform.TransformPoint(direction3D * currentSpeed), Time.deltaTime);
    }

    public void Move(Vector2 direction)
    {
        movementDirection = direction;
        if (clampMovement) {
            movementDirection.Normalize();

            return;
        }

        movementDirection.x = Mathf.Clamp(movementDirection.x, -1, 1);
        movementDirection.y = Mathf.Clamp(movementDirection.y, -1, 1);
    }

    public void SetColor(Color color)
    {
        if (bodyRenderers == null) {
            return;
        }

        foreach (var renderer in bodyRenderers)
        {
            renderer.material.color = color;
        }
    }
}
