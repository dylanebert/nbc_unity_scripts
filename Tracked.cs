using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracked : MonoBehaviour
{
    public string id;

    private void Reset() {
        id = System.Guid.NewGuid().ToString();
    }
}
