using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ColorSwitcher : MonoBehaviour
{
    public Color color;

    private new Collider collider;
    private new Renderer renderer;

    void Awake()
    {
        collider = GetComponent<Collider>();
        renderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        renderer.material.color = color;
    }

    void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponentInParent<Player>();

        if (player == null) {
            return;
        }

        player.SetColor(color);
    }
}
