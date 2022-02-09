using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public Color color;
    public new Collider collider;
    public new Rigidbody rigidbody;
    public Renderer[] bodyRenderers;
    public UnityEvent onHit = new UnityEvent();

    void Awake()
    {
        if (collider == null) {
            Debug.Log("Enemy must contain collider");
        }
    }

    void OnEnable()
    {
        collider.isTrigger = false;
        rigidbody.isKinematic = false;

        if (bodyRenderers == null) {
            return;
        }

        foreach (var renderer in bodyRenderers)
        {
            renderer.material.color = color;
        }
    }

    void OnDisable()
    {
        collider.isTrigger = true;
        rigidbody.isKinematic = true;
    }

    void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponentInParent<Player>();

        if (player == null) {
            return;
        }

        player.HitColor(color);

        onHit.Invoke();
    }

    void OnValidate()
    {
        if (collider == null) {
            collider = GetComponentInChildren<Collider>();
        }
        if (rigidbody == null) {
            rigidbody = GetComponentInChildren<Rigidbody>();
        }
    }
}
