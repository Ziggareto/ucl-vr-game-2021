using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hologram : MonoBehaviour
{

    List<GameObject> buildings;

    public Material hologramMaterial;

    public float scrollSensitivity = 10f;

    public int debrisCost = 6;

    public bool canOnlyBuildCyberbuildings = true;

    [Header("Leave as default")]
    [SerializeField] public int listIndex = 0;

    Transform gun;
    RaycastHit[] hits;
    List<bool> buildingEarned = new List<bool>();

    bool creatingBuilding = false;

    GameObject droneController;

    int debris = 0;


    void AddDebris()
    {
        debris++;
    }
    

    //Sorting algorithm from the internet
    public static void RaycastHitsSort(RaycastHit[] data)
    {
        int i, j;
        RaycastHit temp;
        int N = data.Length;

        for (j = N - 1; j > 0; j--)
        {
            for (i = 0; i < j; i++)
            {
                if (data[i].distance > data[i + 1].distance)
                {
                    temp = data[i];
                    data[i] = data[i + 1];
                    data[i + 1] = temp;
                }
            }
        }
    }

    void ReceiveOffsets(List<List<float>> buildingOffsets)
    {
        //Create the holographic buildings
        GameObject holographicBuilding;
        MeshRenderer renderer;
        MeshFilter filter;
        int j = 0;
        foreach (GameObject building in buildings)
        {
            holographicBuilding = new GameObject();
            holographicBuilding.transform.parent = transform;
            holographicBuilding.transform.SetSiblingIndex(j);
            holographicBuilding.transform.position = new Vector3(
                -buildingOffsets[j][0],
                0,
                -buildingOffsets[j][1]
                );
            renderer = holographicBuilding.AddComponent<MeshRenderer>();
            renderer.enabled = false;
            filter = holographicBuilding.AddComponent<MeshFilter>();

            //Get the mesh from the building prefab
            try
            {
                filter.mesh = building.GetComponent<SkinnedMeshRenderer>().sharedMesh;
            }
            catch (MissingComponentException e)
            {
                filter.mesh = building.GetComponent<MeshFilter>().sharedMesh;
            }

            //Apply the material
            Material[] materialsTemp = new Material[filter.mesh.subMeshCount];
            for (int i = 0; i < filter.mesh.subMeshCount; i++)
            {
                materialsTemp[i] = hologramMaterial;
            }
            renderer.materials = materialsTemp;

            j++;
        }
    }

    void Start()
    {
        buildings = GameObject.Find("City Builder").GetComponent<CityBuilder>().buildingPrefabs;

        for (int i = 0; i < buildings.Count; i++){
            buildingEarned.Add(false);
        }

        gun = GameObject.Find("Gun").transform;
        droneController = GameObject.Find("Drone Controller");

    }

    void Update()
    {
        //Debug.Log("Debris: " + debris.ToString());

        transform.GetChild(listIndex).GetComponent<Renderer>().enabled = false;
        if (!creatingBuilding && debris >= debrisCost)
        {
            hits = Physics.RaycastAll(gun.position, gun.forward, 1000f, 1 << 10);
            RaycastHitsSort(hits);
            if (hits.Length != 0)
            {
                int indexChange = (int) Mathf.Round(-Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity);
                listIndex += indexChange;

                while (listIndex < 0)
                {
                    listIndex += buildingEarned.Count;
                }
                listIndex %= buildingEarned.Count;

                int j = 0;

                while (j <= buildingEarned.Count && j >= -1 && (!(buildingEarned[listIndex]) || hits[0].transform.GetChild(0).GetChild(listIndex).gameObject.GetComponent<ColliderTester>().blocked))
                {
                    if (indexChange < 0)
                    {
                        listIndex--;
                    }
                    else
                    {
                        listIndex++;
                    }
                    while (listIndex < 0)
                    {
                        listIndex += buildingEarned.Count;
                    }
                    listIndex %= buildingEarned.Count;

                    if (j == buildingEarned.Count || j == -1)
                    {
                        return;
                    }
                    j++;
                }

                //Create new building
                if (Input.GetKey(KeyCode.Mouse2))
                {
                    debris -= debrisCost;
                    creatingBuilding = true;
                    hits[0].collider.enabled = false;
                    droneController.SendMessage("CreateBuilding", hits[0].transform);
                }
                else
                {
                    transform.position = hits[0].transform.position;
                    transform.GetChild(listIndex).GetComponent<Renderer>().enabled = true;
                }
            }
        }
    }

    void ConstructionComplete()
    {
        creatingBuilding = false;
    }

    void EarnBuilding(int buildingIndex)
    {
        if (!canOnlyBuildCyberbuildings || buildings[buildingIndex].tag == "Landmark")
        {
            buildingEarned[buildingIndex] = true;
        }
    }
}
