using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneEyeTwitch : MonoBehaviour
{

    float maxRotationAngle = 29f;
    float slowRotation = 0.1f;
    
    private float continueUntil = 0f;
    private float convertedMaxAngle;
    private bool slowMode = false;
    private Quaternion targetRotation;
    private Quaternion oldRotation = Quaternion.identity;
    private Vector3 randomDirection;

    Quaternion GetRandomRotation()
    {
        float outwardsAngle = Random.Range(-convertedMaxAngle, convertedMaxAngle) + Random.Range(-convertedMaxAngle, convertedMaxAngle);
        Vector3 directionVector = Quaternion.Euler(outwardsAngle, 0, 0) * Vector3.forward;
        Quaternion random2Ddirection = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        randomDirection = random2Ddirection * (Quaternion.Euler(Random.Range(0f, slowRotation), 0, 0) * Vector3.forward);
        directionVector = random2Ddirection * directionVector;
        return Quaternion.LookRotation(directionVector);
    }

    void Start()
    {
        convertedMaxAngle = maxRotationAngle / 2;
        targetRotation = GetRandomRotation();
    }

    void Update()
    {
        float currentTime = Time.realtimeSinceStartup;
        if (slowMode)
        {
            Quaternion rotationIncrease = targetRotation;
            rotationIncrease.Normalize();
            oldRotation = transform.localRotation;
            transform.localRotation = Quaternion.LookRotation(transform.localRotation * randomDirection);
            //transform.Rotate(targetRotation * Vector3.up * slowRotation);
            //transform.localRotation = Quaternion.Lerp(oldRotation, oldRotation * rotationIncrease, slowRotation);
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(targetRotation, oldRotation, (continueUntil - currentTime) / 0.1f);
        }
        if (currentTime > continueUntil)
        {
            if (slowMode)
            {
                continueUntil = currentTime + 0.1f;
                targetRotation = GetRandomRotation();
                oldRotation = transform.localRotation;
            }
            else
            {
                transform.localRotation = targetRotation;
                continueUntil = currentTime + Random.Range(0.2f, 1.5f);
            }
            slowMode = !slowMode;
        }
    }
}
