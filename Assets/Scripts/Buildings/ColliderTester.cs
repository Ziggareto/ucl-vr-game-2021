using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderTester : MonoBehaviour
{
    public bool blocked = false;

    float lastCollisionTime = 0f;

    void OnTriggerStay()
    {
        lastCollisionTime = Time.realtimeSinceStartup;
    }

    void Update()
    {
        if (Time.realtimeSinceStartup - lastCollisionTime > 0.1f)
        {
            blocked = false;
        }
        else
        {
            blocked = true;
        }
    }

}
