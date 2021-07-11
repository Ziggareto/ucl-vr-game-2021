using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneMask : MonoBehaviour
{
    Transform droneTransform;
    public float damping = 2f;
    PlayerDrone droneScript;

    void ReceiveDroneObject(GameObject assignedDrone)
    {
        droneTransform = assignedDrone.transform;
        droneScript = assignedDrone.GetComponent<PlayerDrone>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        
        Quaternion targetRotation = droneTransform.rotation;

        string parentMode = droneScript.mode;
        if (parentMode == "wait for instructions" || parentMode == "orbit")
        {
            Vector3 lookTarget = droneScript.orbitOrigin - droneTransform.position;
            lookTarget.y = 0f;
            targetRotation = Quaternion.LookRotation(lookTarget);
            if (parentMode == "orbit")
            {
                transform.rotation = Quaternion.LookRotation(lookTarget);
            }
        }
        else
        {
            targetRotation = Quaternion.LookRotation(droneScript.targetPosition - droneTransform.position);
        }

        transform.position = droneTransform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * damping);

    }
}
