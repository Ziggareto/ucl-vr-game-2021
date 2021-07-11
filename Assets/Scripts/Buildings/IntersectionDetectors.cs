using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionDetectors : MonoBehaviour
{
    List<GameObject> buildings;

    public GameObject colliderTester;

    void Start()
    {
        GameObject empty;
        List<List<float>> prefabOffsets = GameObject.Find("City Builder").GetComponent<CityBuilder>().prefabOffsets;
        buildings = GameObject.Find("City Builder").GetComponent<CityBuilder>().buildingPrefabs;
        BoxCollider boxColl;
        CapsuleCollider capColl;

        int i = 0;
        foreach (GameObject building in buildings)
        {
            empty = Instantiate(colliderTester);
            empty.transform.position = new Vector3(
                transform.position.x - prefabOffsets[i][0],
                0,
                transform.position.z - prefabOffsets[i][1]
                );
            empty.transform.parent = transform;

            i++;

            foreach (BoxCollider collider in building.GetComponents<BoxCollider>())
            {
                boxColl = empty.AddComponent<BoxCollider>();
                boxColl.size = collider.size;
                boxColl.center = collider.center;
                boxColl.isTrigger = true;
            }
            foreach (CapsuleCollider collider in building.GetComponents<CapsuleCollider>())
            {
                capColl = empty.AddComponent<CapsuleCollider>();
                capColl.radius = collider.radius;
                capColl.height = collider.height;
                capColl.center = collider.center;
                capColl.isTrigger = true;
            }
        }
    }

    void Update()
    {
        
    }
}
