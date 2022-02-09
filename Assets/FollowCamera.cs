using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    public Transform target;

    public Vector3 shift = new Vector3(0, 5f, -5f);

    public bool keepRotation = true;
    public bool lookAtTarget = true;

    public float speed = 10f;

    // Update is called once per frame
    void Update()
    {
        if (!target) {
            return;
        }

        Vector3 targetPosition;

        if (keepRotation) {
            targetPosition = target.TransformPoint(shift);
        } else {
            targetPosition = target.position + shift;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);

        if (lookAtTarget) {
            // transform.LookAt(target);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(target.position - transform.position, Vector3.up),
                Time.deltaTime * speed
            );
        }
    }
}
