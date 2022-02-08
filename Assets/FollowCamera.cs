using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    public Transform target;

    public Vector3 shift = new Vector3(0, 5f, -5f);

    public bool keepRotation = true;
    public bool lookAtTarget = true;

    // Update is called once per frame
    void Update()
    {
        if (!target) {
            return;
        }

        if (keepRotation) {
            transform.position = target.TransformPoint(shift);
        } else {
            transform.position = target.position + shift;
        }
        if (lookAtTarget) {
            transform.LookAt(target);
        }
    }
}
