using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialRandomiser : MonoBehaviour
{
    public Material[] surfaceMaterials;
    public Material[] windowMaterials;

    Material chooseRandom(Material[] materialsArray)
    {
        return materialsArray[Random.Range(0, materialsArray.Length)];
    }

    void Start()
    {
        int surfaceMatIndex = 0;
        int windowMatIndex = 0;
        Material[] materialsTemp = GetComponent<MeshRenderer>().materials;
        for (int i = 0; i < materialsTemp.Length; i++)
        {
            if (materialsTemp[i].name.Contains("_004_"))
            {
                surfaceMatIndex = i;
            }
            else if (materialsTemp[i].name.Contains("_005_"))
            {
                windowMatIndex = i;
            }
        }
        materialsTemp[surfaceMatIndex] = chooseRandom(surfaceMaterials);
        materialsTemp[windowMatIndex] = chooseRandom(windowMaterials);
        GetComponent<MeshRenderer>().materials = materialsTemp;

    }
}
