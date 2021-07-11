using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDrone : MonoBehaviour
{
    Rigidbody rb;
    GameObject player;
    float realAcceleration;
    int damping = 2;

    GameObject debrisTarget;
    GameObject bubble;
    Rigidbody targetRb;
    float targetOffset;

    
    bool trueTarget;

    Vector3[] bubbleOffsets;

    [Header("Flight settings")]
    [SerializeField] public float acceleration = 10f;
    [SerializeField] public float speedLimit = 2f;
    [SerializeField] public float defaultHeight = 340f;
    [SerializeField] public float playerOrbitRadius = 20f;
    [SerializeField] public float playerOrbitPeriod = 4f;
    Vector3 playerOrbitVelocity;
    float playerOrbitSpeed;

    Vector3 constructionSiteTarget;

    GameObject droneController;

    [Header("Leave as default")]
    [SerializeField] public Vector3 orbitOrigin;
    [SerializeField] public Vector3 targetPosition;
    [SerializeField] public string mode = "follow player";

    //Orbiting information
    float orbitStartTime;
    float orbitOffset;
    float orbitRadius;
    float orbitTopSpeed;
    int orbitRotationsNumber;
    float orbitDuration;
    float orbitDistance;

    GameObject hologram;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        player = GameObject.Find("FPS_Player");
        droneController = GameObject.Find("Drone Controller");
        hologram = GameObject.Find("Hologram");
        playerOrbitSpeed = playerOrbitRadius * 2f * Mathf.PI / playerOrbitPeriod;
        Debug.Log(playerOrbitSpeed.ToString());
    }

    void GoToConstructionSite (Vector3 siteTarget)
    {
        constructionSiteTarget = siteTarget;
        mode = "move towards site";
        orbitOrigin = droneController.GetComponent<PDController>().raycatcherTransform.position;
    }

    void EndTask()
    {
        mode = "follow player";
        rb.constraints = RigidbodyConstraints.None;
    }

    void AssignDebrisTarget(GameObject target)
    {
        debrisTarget = target;
        bubble = target.transform.GetChild(0).gameObject;
        targetRb = debrisTarget.GetComponent<Rigidbody>();
        targetOffset = bubble.transform.localScale.y * target.transform.localScale.y / 2;
        mode = "grab debris";
    }

    //This is how drones used to follow the player
    /*
    void FollowPlayer()
    {
        targetPosition = player.transform.position;
        targetPosition.y = defaultHeight;
        //targetPosition += new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        realAcceleration = (((targetPosition - transform.position).magnitude - 10) * acceleration / 10);
        if (realAcceleration > acceleration)
        {
            realAcceleration = acceleration;
        }
        else if (realAcceleration < 0)
        {
            realAcceleration = 0f;
        }


        Quaternion rotation = Quaternion.LookRotation(targetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
        rb.AddForce(transform.forward * realAcceleration);
        if (rb.velocity.magnitude > speedLimit)
        {
            rb.velocity = Vector3.Normalize(rb.velocity) * speedLimit;
        }
    }
    */

    bool CastRays()
    {
        if (mode == "bring debris")
        {
            Vector3 rayOrigin;
            bubbleOffsets = new Vector3[]
            {
                bubble.transform.right * (-1),
                bubble.transform.right,
                bubble.transform.up,
                bubble.transform.up * (-1)
            };
            foreach (Vector3 offset in bubbleOffsets)
            {
                rayOrigin = bubble.transform.position + offset * targetOffset;
                if (Physics.Raycast(rayOrigin, targetPosition - rayOrigin, Vector3.Distance(targetPosition, rayOrigin), 1 << 9))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return Physics.Raycast(transform.position, targetPosition - transform.position, Vector3.Distance(targetPosition, transform.position), 1 << 9);
        }
    }

    void Navigate()
    {
        Vector3 lookVector;
        bool doAccelerate = true;

        //If I don't have an unobstructed path to the target, try to get to a point in the sky above it
        if (CastRays())
        {
            targetPosition.y = defaultHeight;
            trueTarget = false;
        }
        else
        {
            trueTarget = true;
        }
        //If I can't get to the sky above the target, go to the sky above me
        if (CastRays())
        {
            targetPosition = transform.position;
            targetPosition.y = defaultHeight;
        }

        //Point in the right direction
        if (trueTarget)
        {
            Vector3 targetDirection = (targetPosition - transform.position);
            targetDirection.Normalize();
            Vector3 idealVelocity = targetDirection * Mathf.Sqrt(2 * acceleration * Vector3.Distance(targetPosition, transform.position));
            if (mode == "grab debris")
            {
                idealVelocity += targetRb.velocity;
            }
            else if (mode == "follow player")
            {
                idealVelocity += playerOrbitVelocity;
            }
            if (idealVelocity.magnitude > speedLimit)
            {
                idealVelocity = Vector3.Normalize(idealVelocity) * speedLimit;
            }
            Vector3 directionalAcceleration = idealVelocity - rb.velocity;
            if (mode == "grab debris" && targetRb.velocity.y < -0.1f)
            {
                float angleA = Vector3.Angle(directionalAcceleration, Vector3.up) * Mathf.Deg2Rad;
                float angleB = Mathf.Asin(Mathf.Sin(angleA) * (-Physics.gravity.y) / acceleration);
                lookVector = directionalAcceleration + Vector3.down * directionalAcceleration.magnitude * Mathf.Sin(angleB) / Mathf.Sin(Mathf.PI - angleA - angleB);
            }
            else
            {
                if (directionalAcceleration.magnitude < acceleration * Time.deltaTime)
                {
                    rb.velocity = idealVelocity;
                    lookVector = idealVelocity;
                    doAccelerate = false;
                }
                else
                {
                    lookVector = idealVelocity - rb.velocity;
                }
            }
        }
        else
        {
            lookVector = targetPosition - transform.position;
        }
        transform.rotation = Quaternion.LookRotation(lookVector);

        //Accelerate
        if (doAccelerate)
        {
            rb.AddForce(transform.forward * acceleration * Time.deltaTime, ForceMode.VelocityChange);
        }
        if (rb.velocity.magnitude > speedLimit)
        {
            rb.velocity = Vector3.Normalize(rb.velocity) * speedLimit;
        }
    }

    void FollowPlayer()
    {
        Vector3 orbitPoint = player.transform.position;
        orbitPoint.y = defaultHeight;
        Vector3 leadingOrbitPoint = orbitPoint;

        List<GameObject> dronesFollowingPlayer = droneController.GetComponent<PDController>().dronesFollowingPlayer;
        float angle = dronesFollowingPlayer.IndexOf(gameObject) * 2f * Mathf.PI / dronesFollowingPlayer.Count;
        angle += ((Time.realtimeSinceStartup / playerOrbitPeriod) % 1) * 2f * Mathf.PI;

        orbitPoint += new Vector3(
            Mathf.Sin(angle) * playerOrbitRadius,
            0,
            Mathf.Cos(angle) * playerOrbitRadius
            );

        leadingOrbitPoint += new Vector3(
            Mathf.Sin(angle + 0.01f) * playerOrbitRadius,
            0,
            Mathf.Cos(angle + 0.01f) * playerOrbitRadius
            );

        playerOrbitVelocity = leadingOrbitPoint - orbitPoint;
        playerOrbitVelocity = Vector3.Normalize(playerOrbitVelocity) * playerOrbitSpeed;

        targetPosition = orbitPoint;

        Navigate();

        //For the benefit of the drone mask
        targetPosition = leadingOrbitPoint;
    }

    void GrabDebris()
    {
        targetPosition = debrisTarget.transform.position;
        targetPosition.y += targetOffset;

        if (Vector3.Distance(targetPosition, transform.position) < rb.velocity.magnitude * Time.deltaTime + 0.5f)
        {
            mode = "bring debris";
            Destroy(targetRb);
            targetRb = player.GetComponent<Rigidbody>();
            bubble.GetComponent<Renderer>().enabled = true;
            BringDebris();
            return;
        }

        Navigate();

    }

    void BringDebris()
    {
        //Move the debris
        debrisTarget.transform.position = transform.position + Vector3.down * targetOffset;

        //Give the debris to the player if I am close enough
        if (Vector3.Distance(debrisTarget.transform.position, player.transform.position) < targetOffset)
        {
            hologram.SendMessage("AddDebris");
            Destroy(bubble);
            Destroy(debrisTarget);
            mode = "follow player";
            FollowPlayer();
            return;
        }

        targetPosition = player.transform.position;
        targetPosition.y += targetOffset * 2 - 1;
        transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);

        Navigate();
    }

    void MoveTowardsSite()
    {
        targetPosition = constructionSiteTarget;

        if (Vector3.Distance(targetPosition, transform.position) < rb.velocity.magnitude * Time.deltaTime)
        {
            transform.position = targetPosition;
            mode = "wait for instructions";
            rb.velocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            droneController.SendMessage("InPosition");
            return;
        }

        Navigate();
    }

    void BeginOrbit(float offset)
    {
        mode = "orbit";
        orbitStartTime = Time.realtimeSinceStartup;

        //Get information from drone controller
        orbitOffset = offset;
        orbitOrigin.y = droneController.GetComponent<PDController>().buildingDimensions[droneController.GetComponent<PDController>().buildingIndex][2];
        orbitRadius = droneController.GetComponent<PDController>().radius;
        orbitDuration = droneController.GetComponent<PDController>().buildingConstructionTime;

        //Work out details of orbit
        float d;
        int i = 0;
        float num1 = acceleration * orbitDuration / 2f;
        float num2 = Mathf.Pow(num1 * 2f, 2f);
        float num3;
        float sAdd;
        float sSubtract;
        while (true)
        {
            i++;
            d = Mathf.Sqrt(Mathf.Pow(i * orbitRadius * 2f * Mathf.PI, 2f) + Mathf.Pow(orbitOrigin.y, 2f));
            num3 = Mathf.Sqrt(num2 - (4f * acceleration * d)) / 2f;
            sAdd = num1 + num3;
            sSubtract = num1 - num3;
            if (sAdd < sSubtract && sAdd < speedLimit)
            {
                orbitTopSpeed = sAdd;
            }
            else if (sSubtract < speedLimit)
            {
                orbitTopSpeed = sSubtract;
            }
            else
            {
                break;
            }
            orbitDistance = d;
        }
        orbitRotationsNumber = i - 1;
    }

    void Orbit()
    {
        float t = Time.realtimeSinceStartup - orbitStartTime;
        float accelerationDuration = orbitTopSpeed / acceleration;
        float remainingT = orbitDuration - t;
        float orbitDistanceCovered;

        //Starting acceleration
        if (t < accelerationDuration)
        {
            orbitDistanceCovered = acceleration * Mathf.Pow(t, 2f) / 2f;
        }
        //Braking deceleration
        else if (remainingT < accelerationDuration)
        {
            orbitDistanceCovered = orbitDistance - (acceleration * Mathf.Pow(remainingT, 2f) / 2f);
        }
        //Moving at constant speed
        else
        {
            orbitDistanceCovered = orbitTopSpeed * t - (Mathf.Pow(orbitTopSpeed, 2f) / (acceleration * 2f));
        }

        //Move to where in the orbit I should be
        float angle = (orbitOffset + (orbitDistanceCovered / orbitDistance) * orbitRotationsNumber) * 2f * Mathf.PI;
        Vector3 goToPosition = new Vector3(
            Mathf.Sin(angle) * orbitRadius,
            0,
            Mathf.Cos(angle) * orbitRadius
            ) + orbitOrigin;
        goToPosition.y = (orbitDistanceCovered / orbitDistance) * orbitOrigin.y;
        transform.position = goToPosition;
    }

    void Update()
    {
        switch (mode)
        {
            case "follow player":
                FollowPlayer();
                break;
            case "grab debris":
                GrabDebris();
                break;
            case "bring debris":
                BringDebris();
                break;
            case "move towards site":
                MoveTowardsSite();
                break;
            case "wait for instructions":
                break;
            case "orbit":
                Orbit();
                break;
        }
    }
}



//Adjust direction so I don't overshoot (bad way of doing this)
/*if (mode == "grab debris")
{
    targetVelocity = targetRb.velocity;
}
else
{
    targetVelocity = Vector3.zero;
}
if (
    trueTarget &&
    Mathf.Pow((rb.velocity - targetVelocity).magnitude, 2) > 2 * acceleration * Vector3.Distance(targetPosition, transform.position)
    )
{
    //transform.rotation = Quaternion.LookRotation(transform.position - targetPosition);
    transform.rotation = Quaternion.LookRotation(-rb.velocity);
}
//Otherwise aim at where I'm trying to go
else
{
    transform.rotation = Quaternion.LookRotation(targetPosition - transform.position);
}*/


//Adjust max speed so I don't overshoot
/*if (trueTarget)
{
    if (mode == "grab debris")
    {
        targetSpeed = (targetRb.velocity + rb.velocity).magnitude - rb.velocity.magnitude;
    }
    else
    {
        targetSpeed = 0f;
    }
    realMaxSpeed = Mathf.Sqrt(2 * acceleration * Vector3.Distance(targetPosition, transform.position)) + targetSpeed;
    if (realMaxSpeed > speedLimit)
    {
        realMaxSpeed = speedLimit;
    }
}
else
{
    realMaxSpeed = speedLimit;
}*/