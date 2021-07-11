using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildProjectile : MonoBehaviour
{

    public GameObject buildingParent;
    public GameObject buildingSegment;
    public GameObject cyberbuilding;

    //void OnCollisionEnter(Collision collision)
    //{
    //    string colliderName = collision.gameObject.name;
    //    if (colliderName != "Gun" && colliderName != "Player")
    //    {
    //        Build();
    //        GameObject.Find("Gun").SendMessage("AddSegments", -49);
    //        Destroy(gameObject);
    //    }
    //}


    void Update()
    {
        if (transform.position.y < 200f)
        {
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            Instantiate(cyberbuilding, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }

    void Build()
    {
        Vector3 myPosition = transform.position;
        float originX = myPosition.x;
        float originY = myPosition.z;

        GameObject parentClone = Instantiate(buildingParent, new Vector3 (originX, 9, originY), Quaternion.identity);
        parentClone.GetComponent<BoxCollider>().size = new Vector3 (9, 18, 9);

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int z = 0; z < 6; z++)
                {
                    if
                    (x != 0 || y != 0 || z == 5)
                    {
                        Instantiate
                        (
                            buildingSegment,
                            new Vector3(
                                originX + (x * 3),
                                1.5f + (z * 3),
                                originY + (y * 3)
                                ),
                            Quaternion.identity,
                            //Again Building -> Building Parent -> Building Segment
                            parentClone.transform.GetChild(0)
                        );
                    }
                }
            }
        }
    }
}
