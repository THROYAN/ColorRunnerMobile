using UnityEngine;
using System.Collections.Generic;

public class LinesGenerator : MonoBehaviour
{
    public GameObject prefab;

    public Vector3 startPoint;
    public Vector3 startRotation;
    public Vector2 lineSize;
    public Camera mainCamera;

    private ObjectPool<GameObject> pool;
    private List<Transform> allLines = new List<Transform>();
    private int lineNumber = 0;

    void Awake()
    {
        if (prefab == null) {
            Debug.LogError("Prefab should be set");

            return;
        }
        pool = new ObjectPool<GameObject>(prefab);
        pool.SetParent(this);

        if (prefab.scene.IsValid()) {
            allLines.Add(prefab.transform);
            lineNumber = 1;
        }
    }

    void Update()
    {
        // remove invisible line
        if (allLines.Count > 1 && !isVisible(allLines[0])) {
            pool.FreeObject(allLines[0].gameObject);
            allLines.RemoveAt(0);
        }

        if (allLines.Count > 0 && !isVisible(allLines[allLines.Count - 1])) {
            return;
        }

        var newLine = pool.GetObject();
        newLine.transform.position = startPoint + Quaternion.Euler(startRotation) * new Vector3(
            0f,
            0f,
            lineSize.y * lineNumber
        );
        newLine.SetActive(true);

        allLines.Add(newLine.transform);
        lineNumber++;
    }

    void OnValidate()
    {
        if (mainCamera == null) {
            mainCamera = Camera.main;
        }

        if (prefab == null) {
            prefab = GameObject.Find("Lines");

            ResetPositionAndRotation();
            GetSizeFromPrefab();
        }
    }

    [ContextMenu("Get size from Prefab")]
    public void GetSizeFromPrefab()
    {
       var bounds = getBounds(prefab);
       
       lineSize = new Vector2(bounds.size.x, bounds.size.z);
    }

    [ContextMenu("Reset position and rotation")]
    public void ResetPositionAndRotation()
    {
        if (prefab == null) {
            return;
        }
        startPoint = prefab.transform.position;
        startRotation = prefab.transform.rotation.eulerAngles;
    }

    // Getting prefab bounds in any way
    private Bounds getBounds(GameObject go)
    {
        if (go == null) {
            return new Bounds();
        }

        var c = go.GetComponent<Collider>();
        if (c != null) {
            return c.bounds;
        }

        var r = go.GetComponent<Renderer>();
        if (r != null) {
            return r.bounds;
        }

        var colliderList = go.GetComponentsInChildren<Collider>();

        if (colliderList.Length > 0) {
            var min = go.transform.position;
            var max = go.transform.position;
            foreach (var collider in colliderList)
            {
                var cSize = collider.bounds.size;
                var cPos = collider.bounds.min;

                if (min.x > cPos.x) {
                    min.x = cPos.x;
                }
                if (max.x < cPos.x + cSize.x) {
                    max.x = cPos.x + cSize.x;
                }
                if (min.y > cPos.y) {
                    min.y = cPos.y;
                }
                if (max.y < cPos.y + cSize.y) {
                    max.y = cPos.y + cSize.y;
                }
                if (min.z > cPos.z) {
                    min.z = cPos.z;
                }
                if (max.z < cPos.z + cSize.z) {
                    max.z = cPos.z + cSize.z;
                }
            }
            Vector3 size = max - min;;

            return new Bounds(
                min + size / 2f,
                size
            );
        }

        // @todo: count for all renderers

        return new Bounds();
    }

    bool isVisible(Transform tr)
    {
        // foreach (var renderer in tr.GetComponentsInChildren<Renderer>())
        // {
        //     if (renderer.isVisible) {
        //         return true;
        //     }
        // }

        // return false;
        var bounds = getBounds(tr.gameObject);
        
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        return GeometryUtility.TestPlanesAABB(planes, bounds);
        // bounds.SetMinMax(mainCamera.WorldToViewportPoint(bounds.min), mainCamera.WorldToViewportPoint(bounds.max));

        // // if (bounds.min.z < 0 && bounds.max.z < 0) {
        // //     return false;
        // // }

        // // bounds.min = new Vector3(bounds.min.x, bounds.min.y, 0f);
        // // bounds.max = new Vector3(bounds.max.x, bounds.max.y, 0f);

        // var viewBounds = new Bounds();
        // viewBounds.SetMinMax(Vector3.zero, new Vector3(1f, 1f, 1000f));

        // return bounds.Intersects(viewBounds);

        // Quaternion rotation = Quaternion.Euler(startRotation);
        // Vector3[] rect = boundsToRect(bounds, rotation);

        // foreach (var point in rect)
        // {
        //     Vector3 viewPos = mainCamera.WorldToViewportPoint(point);
        //     if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0) {
        //         return true;
        //     }
        // }

        // return false;
    }

    Vector3[] boundsToRect(Bounds bounds, Quaternion rotation)
    {
        Vector3[] rect = new Vector3[4];
        Vector3 diag = rotation * new Vector3(bounds.size.x, 0f, bounds.size.z);
        Vector3 left = rotation * new Vector3(0f, 0f, bounds.size.z);
        Vector3 right = rotation * new Vector3(bounds.size.x, 0f, 0f);

        return new Vector3[] {
            bounds.min,
            bounds.min + left,
            bounds.min + diag,
            bounds.min + right,
        };
    }

    void OnDrawGizmosSelected()
    {
        if (prefab == null) {
            return;
        }

        var bounds = getBounds(prefab);

        if (bounds.size.sqrMagnitude < 0.1f) {
            return;
        }

        Debug.DrawLine(bounds.min, bounds.max, Color.red);

        var rotation = Quaternion.Euler(startRotation);

        Vector3 diag = rotation * new Vector3(bounds.size.x, 0f, bounds.size.z);
        Vector3 left = rotation * new Vector3(0f, 0f, bounds.size.z);
        Vector3 right = rotation * new Vector3(bounds.size.x, 0f, 0f);

        Vector3 leftBottom = startPoint + bounds.min;

        Debug.DrawLine(leftBottom, leftBottom + left, Color.cyan);
        // Debug.DrawRay()
        Debug.DrawLine(leftBottom + left, leftBottom + diag, Color.cyan);
        Debug.DrawLine(leftBottom + diag, leftBottom + right, Color.cyan);
        Debug.DrawLine(leftBottom, leftBottom + right, Color.cyan);

        bounds.SetMinMax(mainCamera.WorldToViewportPoint(bounds.min), mainCamera.WorldToViewportPoint(bounds.max));
        Debug.DrawLine(bounds.min, bounds.max, Color.magenta);

        foreach (var renderer in prefab.GetComponentsInChildren<Renderer>())
        {
            if (renderer.isVisible) {
                Debug.DrawLine(renderer.bounds.min, renderer.bounds.max, Color.green);
            }
        }
        if (isVisible(prefab.transform)) {
            Debug.DrawLine(leftBottom + left, leftBottom + right, Color.green);
        }
    }
}
