using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingUtils : MonoBehaviour
{
    public List<GameObject> objects;

    public void AddTracked() {
        foreach (GameObject obj in objects)
            if (obj.GetComponent<Tracked>())
                obj.GetComponent<Tracked>().id = System.Guid.NewGuid().ToString();
            else
                obj.AddComponent<Tracked>();
    }

    public void FindObjects() {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Dynamic"))
            objects.Add(obj);
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Static"))
            objects.Add(obj);
    }
}
