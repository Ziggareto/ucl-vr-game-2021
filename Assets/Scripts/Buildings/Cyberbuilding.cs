using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cyberbuilding : MonoBehaviour
{
    [Header("Give each building prefab a unique number, starting from 0")]
    [SerializeField] public int buildingIndex;

    [Header("Prefabs")]
    [SerializeField] public GameObject dustCloud;
    [SerializeField] public GameObject segment;
    [SerializeField] public GameObject intersectionDetector;

    [Header("Collapse settings")]
    [SerializeField] public float spin = 0.6f;
    [SerializeField] public int numberOfSegments = 30;
    [SerializeField] public float verticalRandomness = 0.1f;
    [SerializeField] public float segmentSpin =91000f;

    private Collider[] colliders;
    private Rigidbody rb;
    private Vector3 dir = new Vector3(0, 0, 0);
    private GameObject segmentInstance;
    private GameObject hologram;


    void Start()
    {
        colliders = GetComponentsInChildren<Collider>();
        rb = GetComponent<Rigidbody>();
        hologram = GameObject.Find("Hologram");
    }

    void Update()
    {
        rb.AddTorque(dir * Time.deltaTime * Mathf.Sin(Vector3.Angle(transform.up, Vector3.up) * Mathf.Deg2Rad + 0.02f), ForceMode.VelocityChange);

        if (transform.position.y < -313f)
        {
            Instantiate(
                intersectionDetector,
                transform.parent,
                false
                );
            transform.parent.gameObject.GetComponent<BoxCollider>().enabled = true;
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {

        string colliderName = collision.gameObject.name;
        if (colliderName.Contains("Bullet") || colliderName.Contains("Laser") || colliderName.Contains("Rocket"))
        {
            Collapse(collision);
        }

    }

    Vector3 RandomDirection(float minAngle = 0f, float maxAngle = Mathf.PI * 2)
    {
        float rand = Random.Range(minAngle, maxAngle);
        return new Vector3(Mathf.Sin(rand), 0, Mathf.Cos(rand));
    }

    Vector3 RandomDirection3D()
    {
        float rand = Random.Range(0f, Mathf.PI * 2);
        float pitchAngle = Random.Range(0f, Mathf.PI);
        Vector3 directionVector =  new Vector3(Mathf.Sin(rand), 0f, Mathf.Cos(rand));
        directionVector *= Mathf.Cos(pitchAngle);
        directionVector.y = Mathf.Sin(pitchAngle);
        return directionVector;
    }

    void Collapse(Collision collision)
    {
        hologram.SendMessage("EarnBuilding", buildingIndex);

        foreach (Collider cldr in colliders)
            cldr.enabled = false;
        rb.constraints = RigidbodyConstraints.None;
        dir = RandomDirection() * spin;

        Transform cloudTransform = dustCloud.transform;
        Vector3 explosionOrigin = transform.position;
        explosionOrigin.y = collision.transform.position.y;
        Instantiate(dustCloud, explosionOrigin, cloudTransform.rotation);

        Vector3 randDir;
        float angle = Mathf.PI * 2 / numberOfSegments;
        float segmentAngle;
        for (int i = 0; i < numberOfSegments; i++)
        {
            segmentAngle = angle * i;
            randDir = RandomDirection(segmentAngle, segmentAngle + angle);
            randDir.y += Random.Range(-verticalRandomness, verticalRandomness);
            segmentInstance = Instantiate(segment, explosionOrigin, Quaternion.identity);
            segmentInstance.GetComponent<Rigidbody>().AddForce(randDir * 3000);
            segmentInstance.GetComponent<Rigidbody>().AddTorque(RandomDirection3D() * Random.value * segmentSpin);
        }
    }
}
