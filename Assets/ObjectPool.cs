using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T: Object
{
    private T obj;
    private int poolSize;
    private List<T> objectList;
    private MonoBehaviour parent;

    private int lastId = 0;

    public ObjectPool(T obj, int poolSize, bool autopopulate)
    {
        objectList = new List<T>(poolSize);

        this.obj = obj;
        this.poolSize = poolSize;
        if (autopopulate) {
            populate();
        }
    }

    public ObjectPool(T obj, int poolSize): this(obj, poolSize, true)
    { }

    public ObjectPool(T obj): this(obj, 10)
    { }

    public T GetObject()
    {
        if (objectList.Count == 0) {
            populate();
        }

        T obj = objectList[0];

        objectList.RemoveAt(0);

        if (objectList.Count == 0) {
            // @todo pass monobehaviour to run populate in coroutine
            populate();
        }

        return obj;
    }

    public void FreeObject(T obj)
    {
        if (objectList.Contains(obj)) {
            return;
        }

        objectList.Add(obj);
        if (obj is GameObject) {
            (obj as GameObject).SetActive(false);
        }
    }

    public void SetParent(MonoBehaviour parent)
    {
        this.parent = parent;
    }

    private void populate()
    {
        objectList.Capacity += poolSize;

        // create in background
        if (parent != null && poolSize > 1) {
            objectList.Add(createNewObject());
            parent.StartCoroutine(populateCouroutine(poolSize - 1));

            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            objectList.Add(createNewObject());
        }
    }

    private T createNewObject()
    {
        T newObj = parent == null
            ? Object.Instantiate<T>(obj)
            : Object.Instantiate<T>(obj, parent.transform);

        newObj.name = obj.name + " #" + lastId++;
        if (newObj is GameObject) {
            (newObj as GameObject).SetActive(false);
        }

        return newObj;
    }   

    private IEnumerator populateCouroutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            objectList.Add(Object.Instantiate<T>(obj));

            yield return null;
        }
    }
}
