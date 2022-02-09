using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Color color;
    public new Collider collider;
    public Renderer[] bodyRenderers;
    public delegate void OnHit(Enemy enemy);
    public event OnHit onHit;

    void Awake()
    {
        if (collider == null) {
            Debug.Log("Enemy must contain collider");
        }
    }

    void OnEnable()
    {
        if (bodyRenderers == null) {
            return;
        }

        foreach (var renderer in bodyRenderers)
        {
            renderer.material.color = color;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponentInParent<Player>();

        if (player == null) {
            return;
        }

        player.HitColor(color);

        if (onHit != null) {
            onHit(this);
        }
    }

    void OnValidate()
    {
        if (collider == null) {
            collider = GetComponentInChildren<Collider>();
        }
    }
}
