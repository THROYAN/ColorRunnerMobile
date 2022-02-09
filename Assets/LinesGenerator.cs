using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LinesGenerator : MonoBehaviour
{
    public Color[] colors = new Color[0];
    public GameObject roadPrefab;
    public ColorSwitcher colorSwitcherPrefab;
    public Enemy enemyPrefab;
    public Vector3 startPoint;
    public Vector3 startRotation;
    public Vector2 roadSize;
    public int roadSegmentCount = 3;
    public Vector2Int colorSwitcherFrequency = new Vector2Int(0, 2);
    public Vector2Int enemyFrequency = new Vector2Int(5, 20);
    public Camera mainCamera;
    public LayerMask objectLayer;

    private ObjectPool<GameObject> roadPool;
    private List<GameObject> allRoads = new List<GameObject>();
    private int roadNumber = 0;
    private ObjectPool<ColorSwitcher> colorSwitcherPool;
    private Dictionary<GameObject, ColorSwitcher[]> allSwitchers = new Dictionary<GameObject, ColorSwitcher[]>();
    private ObjectPool<Enemy> enemyPool;
    private Dictionary<GameObject, Enemy[]> allEnemies = new Dictionary<GameObject, Enemy[]>();

    void Awake()
    {
        initializeRoads();
        initializeColorSwitchers();
        initializeEnemies();
    }

    void Update()
    {
        // remove invisible line
        if (allRoads.Count > 1 && !isVisible(allRoads[0])) {
            RemoveRoad(allRoads[0].gameObject);
        }

        // last generated road is invisible, no need for new roads
        if (allRoads.Count > 0 && !isVisible(allRoads[allRoads.Count - 1])) {
            return;
        }

        addNewRoad();
    }

    void OnValidate()
    {
        if (mainCamera == null) {
            mainCamera = Camera.main;
        }

        if (roadPrefab == null) {
            roadPrefab = GameObject.Find("Lines");

            ResetStartPositionAndRotation();
            ResetRoadSize();
        }
    }

    [ContextMenu("Reset road size")]
    public void ResetRoadSize()
    {
       var bounds = getBounds(roadPrefab);
       
       roadSize = new Vector2(bounds.size.x, bounds.size.z);
    }

    [ContextMenu("Reset road position and rotation")]
    public void ResetStartPositionAndRotation()
    {
        if (roadPrefab == null) {
            return;
        }
        startPoint = roadPrefab.transform.position;
        startRotation = roadPrefab.transform.rotation.eulerAngles;
    }

    private void initializeRoads()
    {
        if (roadPrefab == null) {
            Debug.LogError("Road prefab must be set");

            return;
        }
        roadPool = new ObjectPool<GameObject>(roadPrefab, 5, false);
        roadPool.SetParent(this);

        if (roadPrefab.scene.IsValid()) {
            allRoads.Add(roadPrefab);
            roadNumber = 1;
        }
    }

    private void initializeColorSwitchers()
    {
        if (colorSwitcherPrefab == null) {
            Debug.LogError("ColorSwitcher prefab must be set");

            return;
        }
        colorSwitcherPool = new ObjectPool<ColorSwitcher>(colorSwitcherPrefab, 5, false);
        colorSwitcherPool.SetParent(this);

        if (colorSwitcherPrefab.gameObject.scene.IsValid()) {
            colorSwitcherPool.FreeObject(colorSwitcherPrefab);
            colorSwitcherPrefab.gameObject.SetActive(false);
        }
    }

    private void initializeEnemies()
    {
        if (enemyPrefab == null) {
            Debug.LogError("Enemy prefab must be set");

            return;
        }
        enemyPool = new ObjectPool<Enemy>(enemyPrefab, 5, false);
        enemyPool.SetParent(this);

        if (enemyPrefab.gameObject.scene.IsValid()) {
            enemyPool.FreeObject(enemyPrefab);
            enemyPrefab.gameObject.SetActive(false);
        }
    }

    private void RemoveRoad(GameObject road)
    {
        roadPool.FreeObject(road);
        allRoads.Remove(road);

        ColorSwitcher[] switchers;
        allSwitchers.TryGetValue(road, out switchers);
        if (switchers != null) {
            foreach (var switcher in switchers)
            {
                colorSwitcherPool.FreeObject(switcher);
            }
            allSwitchers.Remove(road);
        }

        Enemy[] enemies;
        allEnemies.TryGetValue(road, out enemies);
        if (enemies != null) {
            foreach (var enemy in enemies)
            {
                enemyPool.FreeObject(enemy);
            }
            allEnemies.Remove(road);
        }
    }

    private void addNewRoad()
    {
        var newRoad = roadPool.GetObject();
        newRoad.transform.position = startPoint + Quaternion.Euler(startRotation) * new Vector3(
            0f,
            0f,
            roadSize.y * roadNumber
        );
        newRoad.SetActive(true);

        allRoads.Add(newRoad);
        roadNumber++;

        StartCoroutine(addSwitchers(newRoad));
        StartCoroutine(addEnemies(newRoad));
    }

    private IEnumerator addSwitchers(GameObject road)
    {
        int switcherCount = Random.Range(colorSwitcherFrequency.x, colorSwitcherFrequency.y + 1);
        if (switcherCount <= 0 || colors.Length == 0) {
            yield break;
        }

        allSwitchers[road] = new ColorSwitcher[switcherCount];

        var bounds = getBounds(road);

        for (int i = 0; i < switcherCount; i++)
        {
            var c = colorSwitcherPool.GetObject();
            var cb = getBounds(c.gameObject);
            c.transform.position = getRandomPositionOnRoad(bounds, cb.size);
            c.color = colors[Random.Range(0, colors.Length)];
            c.gameObject.SetActive(true);
            allSwitchers[road][i] = c;

            yield return null;
        }
    }

    private IEnumerator addEnemies(GameObject road)
    {
        int enemyCount = Random.Range(enemyFrequency.x, enemyFrequency.y + 1);
        if (enemyCount <= 0 || colors.Length == 0) {
            yield break;
        }

        allEnemies[road] = new Enemy[enemyCount];

        var bounds = getBounds(road);

        for (int i = 0; i < enemyCount; i++)
        {
            var e = enemyPool.GetObject();
            var eb = getBounds(e.gameObject);
            // need to enable for collision
            e.enabled = true;
            e.transform.position = getRandomPositionOnRoad(bounds, eb.size);
            e.color = colors[Random.Range(0, colors.Length)];
            e.gameObject.SetActive(true);

            allEnemies[road][i] = e;

            yield return null;
        }
    }

    private Vector3 getRandomPositionOnRoad(Bounds roadBounds, Vector3 size)
    {
        // int cols = Mathf.CeilToInt(roadBounds.size.x / size.x);
        int cols = roadSegmentCount;
        float cellHeight = roadSize.x / (float)cols;
        float cellWidth = size.z;
        if (cellWidth <= 0.1f) {
            cellWidth = cellHeight;
        }

        int rows = Mathf.CeilToInt(roadSize.y / cellWidth);

        Vector3 cellSize = new Vector3(cellHeight, 0f, cellWidth);

        int col = Random.Range(0, cols);
        int row = Random.Range(0, rows);

        int insurance = 0;

        var min = roadBounds.min;
        min.y += (roadBounds.max.y - roadBounds.min.y) / 2f;

        var point = getPointOnRoad(min, cellSize, col, row);
        while (Physics.OverlapBox(point, cellSize / 2f, Quaternion.identity, objectLayer.value).Length > 0
            && insurance++ < 100
        ) {
            col = Random.Range(0, cols);
            row = Random.Range(0, rows);

            point = getPointOnRoad(min, cellSize, col, row);
        }

        return point;
    }

    private Vector3 getPointOnRoad(Vector3 leftBottom, Vector3 cellSize, int col, int row)
    {
        return new Vector3(leftBottom.x + (col + 0.5f) * cellSize.x, leftBottom.y, leftBottom.z + (row + 0.5f) * cellSize.z);
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

                min.x = Mathf.Min(min.x, cPos.x);
                max.x = Mathf.Max(max.x, cPos.x + cSize.x);

                min.y = Mathf.Min(min.y, cPos.y);
                max.y = Mathf.Max(max.y, cPos.y + cSize.y);

                min.z = Mathf.Min(min.z, cPos.z);
                max.z = Mathf.Max(max.z, cPos.z + cSize.z);
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

    bool isVisible(GameObject go)
    {
        // foreach (var renderer in tr.GetComponentsInChildren<Renderer>())
        // {
        //     if (renderer.isVisible) {
        //         return true;
        //     }
        // }

        // return false;
        var bounds = getBounds(go.transform.gameObject);
        
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
        if (roadPrefab == null) {
            return;
        }

        var bounds = getBounds(roadPrefab);

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

        foreach (var renderer in roadPrefab.GetComponentsInChildren<Renderer>())
        {
            if (renderer.isVisible) {
                Debug.DrawLine(renderer.bounds.min, renderer.bounds.max, Color.green);
            }
        }
        if (isVisible(roadPrefab)) {
            Debug.DrawLine(leftBottom + left, leftBottom + right, Color.green);
        }
    }
}
