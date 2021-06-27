﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class TeleportOnClickGrappling : MonoBehaviour
{
    [SerializeField] KeyCode teleportKey = KeyCode.Mouse1;
    [SerializeField] InputFeatureUsage<bool> teleportKeyVR = CommonUsages.triggerButton;

    private RaycastHit lastRaycastHit;

    private InputDevice device;

    [SerializeField] float range = 50;
    [SerializeField] float speed = 2.5f;

    [Tooltip("distance to hooked point after which released")]
    [SerializeField] float epsilon=2.0f;
    private bool travelling = false;
    private float gravity;
    private Vector3 targetPos;
    private float t = 0;
    private Vector3 dir;
    [SerializeField] PlayerMovementFPS movement;
    private void Start() {
        gravity = movement.gravity;
        Debug.Log("movement: " + movement);

        var leftHandDevices = new List<InputDevice>();   
        InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);
        if(leftHandDevices.Count == 1)
        {
            device = leftHandDevices[0];
            Debug.Log("Found left hand device");

            var inputFeatures = new List<InputFeatureUsage>();
            if (device.TryGetFeatureUsages(inputFeatures)) {
                foreach (var feature in inputFeatures) {
                    if (feature.type == typeof(bool)) {
                        bool featureValue;
                        if (device.TryGetFeatureValue(feature.As<bool>(), out featureValue))
                        {
                            Debug.Log(string.Format("Bool feature '{0}''s value is '{1}'", feature.name, featureValue.ToString()));
                        }
                    }
                    else {
                        Debug.Log(string.Format("Non bool feature '{0}''s has type is '{1}'", feature.name, feature.type));
                    }
                }
            }
        }
        else {
            Debug.Log("No left hand devices!!");
        }

        var inputDevices = new List<InputDevice>();
        InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.role.ToString()));
        }
    }

    private GameObject GetLookedAtObject()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        if (Physics.Raycast(origin, direction, out lastRaycastHit, range))
            return lastRaycastHit.collider.gameObject;
        else
            return null;
    }
    private void TeleportToLookAt(GameObject teleportationTarget)
    {
        //transform.position = lastRaycastHit.point + lastRaycastHit.normal * 2; 
        Transform targetTrans = teleportationTarget.transform;
        
        if (Vector3.Dot(lastRaycastHit.normal, Vector3.up) > 1/Mathf.Sqrt(2)) {
            //This is designed for teleporting next to objects - i.e. backtrack along the surface normal a little, and go up so as not to fall through the floor
            //transform.root.position = new Vector3(lastRaycastHit.point.x, lastRaycastHit.point.y + 2, lastRaycastHit.point.z) - lastRaycastHit.normal * 2;
            targetPos = new Vector3(lastRaycastHit.point.x, lastRaycastHit.point.y + 2, lastRaycastHit.point.z) - lastRaycastHit.normal * 2;
        }
        else {
            //This is designed for teleporting on top of buildings
            Debug.Log("teleporting on top of building: it's position is" + targetTrans.position.ToString());
            //transform.root.position = new Vector3(targetTrans.position.x, targetTrans.position.y + targetTrans.lossyScale.y/2 + 2, targetTrans.position.z);
            targetPos = new Vector3(targetTrans.position.x, targetTrans.position.y + targetTrans.lossyScale.y/2 + 2, targetTrans.position.z);
        }

        movement.gravity = 0;
        t = 0;
        dir = targetPos - transform.root.position;
        travelling = true;
    }
    void Travel()
    {
        if (travelling)
        {
            t += Time.deltaTime * speed / dir.magnitude;
            Transform root = transform.root;
            root.position = Vector3.Lerp(root.position, targetPos, t);

            if (Vector3.Distance(root.position, targetPos) <= epsilon)
            {
                travelling = false;
                movement.gravity = gravity;
            }
        }
    }
    void Update()
    {
        bool triggerValue;
        if  (   
                Input.GetKeyDown(teleportKey) ||
                (device.TryGetFeatureValue(teleportKeyVR, out triggerValue) && triggerValue)
            )
        {
            GameObject targetObject = GetLookedAtObject();
            if (targetObject != null && !travelling)
                TeleportToLookAt(targetObject);
        }

        Travel();
    }
}