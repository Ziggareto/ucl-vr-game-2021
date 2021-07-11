using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] public GameObject raycatcherPrefab;
    [SerializeField] public List<GameObject> buildingPrefabs;
    [SerializeField] public GameObject fogPlate;

    [Header("Procedural generation settings")]
    [SerializeField] public float mapSizeX = 201f;
    [SerializeField] public float mapSizeY = 150f;
    [SerializeField] public float buildingSeparation = 3f;
    [SerializeField] public int numberOfPaths = 20;
    [SerializeField] public float maxPathLength = 89f;
    [SerializeField] public float minPathLength = 21f;
    [SerializeField] public float maxPathWidth = 15f;
    [SerializeField] public float minPathWidth = 6f;
    [SerializeField] public float maxHeightOffset = 50f;
    [SerializeField] public float heightRandomness = 0.5f;
    //[SerializeField] public float landmarkRatio = 0.1f;
    [SerializeField] public int instancesPerLandmark = 3;

    [Header("Fog settings")]
    [SerializeField] public float fogTop = 25f;
    [SerializeField] public float fogBottom = 15f;
    [SerializeField] public int numberOfPlates = 10;


    [Header("Leave as default")]
    [SerializeField] public List<List<float>> prefabOffsets = new List<List<float>>();
    
    
    List<List<float>> trueBuildingDimensions = new List<List<float>>();

    void Start()
    {
        Fog();
        StartCoroutine(GenerateBuildings());
    }

    void Fog()
    {
        GameObject plate;
        for (int i = 0; i < numberOfPlates; i++)
        {
            plate = Instantiate(fogPlate, Vector3.zero, Quaternion.identity);
            plate.transform.position += new Vector3(0, ((i + 1) * (fogTop - fogBottom) / numberOfPlates) + fogBottom, 0);
            plate.transform.localScale = new Vector3(mapSizeX / 10, 1, mapSizeY / 10);
        }
    }

    int DivideAndRound(float value)
    {
        value /= 10;
        float roundedFloat;
        if (value % 0.5f == 0)
            roundedFloat = Mathf.Ceil(value);
        else
            roundedFloat = Mathf.Round(value);
        if (roundedFloat < 0f)
        {
            return 0;
        }
        else
        {
            return (int)roundedFloat;
        }
    }

    bool CheckCoords(Dictionary<string, bool> grid, int x, int y)
    {
        try
        {
            if (grid[x.ToString() + " " + y.ToString()])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (KeyNotFoundException)
        {
            return true;
        }
    }

    string cardinalTranslate(string origin, string dir, int extendForward, int extendRight)
    {
        List<string> originCoords = new List<string>(origin.Split(' '));
        int originX = int.Parse(originCoords[0]);
        int originY = int.Parse(originCoords[1]);
        switch (dir)
        {
            case "n":
                originX += extendRight;
                originY -= extendForward;
                break;
            case "e":
                originX += extendForward;
                originY += extendRight;
                break;
            case "s":
                originX -= extendRight;
                originY += extendForward;
                break;
            case "w":
                originX -= extendForward;
                originY -= extendRight;
                break;
        }
        return originX.ToString() + " " + originY.ToString();
    }

    IEnumerator GenerateBuildings()
    {
        float startTime = Time.realtimeSinceStartup;
        int gridX = DivideAndRound(mapSizeX);
        int gridY = DivideAndRound(mapSizeY);
        int gap = DivideAndRound(buildingSeparation);
        int maxPL = DivideAndRound(maxPathLength - (gap * 20));
        int minPL = DivideAndRound(minPathLength - (gap * 20));
        int maxPW = DivideAndRound((maxPathWidth - (gap * 20) - 1) / 2) * 2 + 1;
        int minPW = DivideAndRound((minPathWidth - (gap * 20) - 1) / 2) * 2 + 1;
        int originX;
        int originY;
        List<int> possibilities;
        List<int> buildingDimensions = new List<int>();

        //Create list of building prefab dimensions
        List<List<int>> prefabDimensions = new List<List<int>>();
        Vector3 centre;
        Vector3 size;
        float radius;
        float minX;
        float minY;
        float maxX;
        float maxY;
        float maxZ;
        List<float> offset;
        List<int> dimensions;
        List<float> trueDimensions;
        foreach (GameObject prefab in buildingPrefabs)
        {
            minX = 0f;
            minY = 0f;
            maxX = 0f;
            maxY = 0f;
            maxZ = 0f;
            offset = new List<float>();
            dimensions = new List<int>();
            trueDimensions = new List<float>();
            foreach (BoxCollider collider in prefab.GetComponents<BoxCollider>())
            {
                centre = collider.center;
                size = collider.size;
                if (centre.x + (size.x / 2) > maxX)
                {
                    maxX = centre.x + (size.x / 2);
                }
                if (centre.x - (size.x / 2) < minX)
                {
                    minX = centre.x - (size.x / 2);
                }
                if (centre.z + (size.z / 2) > maxY)
                {
                    maxY = centre.z + (size.z / 2);
                }
                if (centre.z - (size.z / 2) < minY)
                {
                    minY = centre.z - (size.z / 2);
                }
                if (centre.y + (size.y / 2) > maxZ)
                {
                    maxZ = centre.y + (size.y / 2);
                }
            }
            foreach (CapsuleCollider collider in prefab.GetComponents<CapsuleCollider>())
            {
                centre = collider.center;
                radius = collider.radius;
                if (centre.x + radius > maxX)
                {
                    maxX = centre.x + radius;
                }
                if (centre.x - radius < minX)
                {
                    minX = centre.x - radius;
                }
                if (centre.z + radius > maxY)
                {
                    maxY = centre.z + radius;
                }
                if (centre.z - radius < minY)
                {
                    minY = centre.z - radius;
                }
                if (centre.y + (collider.height / 2) > maxZ)
                {
                    maxZ = centre.y + (collider.height / 2);
                }
            }
            offset.Add(Mathf.Abs((maxX + minX) / 2));
            offset.Add(Mathf.Abs((maxY + minY) / 2));
            dimensions.Add(Mathf.CeilToInt((maxX - minX) / 10));
            dimensions.Add(Mathf.CeilToInt((maxY - minY) / 10));
            trueDimensions.Add(maxX - minX);
            trueDimensions.Add(maxY - minY);
            trueDimensions.Add(maxZ);
            prefabOffsets.Add(offset);
            prefabDimensions.Add(dimensions);
            trueBuildingDimensions.Add(trueDimensions);
        }

        //Give the offsets and true building dimensions to the drone controller
        GameObject droneController = GameObject.Find("Drone Controller");
        droneController.SendMessage("ReceiveDimensions", trueBuildingDimensions);
        droneController.SendMessage("ReceiveOffsets", prefabOffsets);

        //Give the offsets to the hologram
        GameObject.Find("Hologram").SendMessage("ReceiveOffsets", prefabOffsets);

        //Create grid
        Dictionary<string, bool> grid = new Dictionary<string, bool>();
        List<List<int>> gridCells = new List<List<int>>();
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                grid.Add(x.ToString() + " " + y.ToString(), false);
                gridCells.Insert(Random.Range(0, gridCells.Count + 1), new List<int> { x, y });
            }
        }

        //Generate paths in between buildings
        List<List<string>> corners = new List<List<string>>();
        List<string> currentCorner = new List<string>();
        int cornerEdge;
        int pathsAdded = 0;
        List<string> originCorner = new List<string>();
        int index;
        string cornerCoords;
        string testCoords;
        int pathLength;
        int pathWidth;
        int newPathWidth;
        List<string> cardinalDirections = new List<string> { "n", "e", "s", "w" };
        int offset1;
        int offset2;
        int offset3;
        string originDir;
        while (pathsAdded < numberOfPaths)
        {
            //Start a new path system at the edge if necessary
            if (corners.Count < 1)
            {
                if (Random.Range(0, 2) == 0)
                {
                    cornerEdge = (gridX - 1) * Random.Range(0, 2);
                    currentCorner.Add(cornerEdge.ToString() + " " + Random.Range(0, gridY).ToString());
                    if (cornerEdge == 0)
                    {
                        currentCorner.Add("e");
                    }
                    else
                    {
                        currentCorner.Add("w");
                    }
                }
                else
                {
                    cornerEdge = (gridY - 1) * Random.Range(0, 2);
                    currentCorner.Add(Random.Range(0, gridX).ToString() + " " + cornerEdge.ToString());
                    if (cornerEdge == 0)
                    {
                        currentCorner.Add("s");
                    }
                    else
                    {
                        currentCorner.Add("n");
                    }
                }
                currentCorner.Add(Random.Range(minPW, maxPW + 1).ToString());
                corners.Add(currentCorner);
            }
            index = Random.Range(0, corners.Count);
            originCorner = corners[index];
            corners.RemoveAt(index);
            //Record all options for new paths branching from the path I am about to make
            pathLength = Random.Range(minPL, maxPL + 1);
            pathWidth = int.Parse(originCorner[2]);
            originDir = originCorner[1];
            foreach (string dir in cardinalDirections)
            {
                if (dir == originDir)
                {
                    newPathWidth = pathWidth;
                }
                else
                {
                    newPathWidth = Random.Range(minPW, maxPW + 1);
                }
                if (!(
                    (originDir == "n" && dir == "s") ||
                    (originDir == "s" && dir == "n") ||
                    (originDir == "w" && dir == "e") ||
                    (originDir == "e" && dir == "w")
                    ))
                {
                    if
                    (
                        ((dir == "n" || dir == "s") && (originDir == "w" || originDir == "e")) ||
                        ((dir == "w" || dir == "e") && (originDir == "s" || originDir == "n"))
                    )
                    {
                        offset1 = (newPathWidth - 1) / 2;
                        offset2 = (pathWidth - 1) / 2;
                        offset3 = (pathWidth + 1) / 2;
                    }
                    else
                    {
                        offset1 = 0;
                        offset2 = 0;
                        offset3 = 1;
                    }
                    cornerCoords = cardinalTranslate(originCorner[0], originDir, pathLength - offset1 - 1, 0);
                    cornerCoords = cardinalTranslate(cornerCoords, dir, offset2, 0);
                    testCoords = cardinalTranslate(cornerCoords, dir, offset3, 0);
                    if (grid.ContainsKey(testCoords))
                    {
                        corners.Add(new List<string> { cornerCoords, dir, newPathWidth.ToString() });
                    }
                }
            }
            //Mark new path on grid
            for (int f = 0; f < pathLength; f++)
            {
                for (int r = (1 - pathWidth) / 2; r < (pathWidth + 1) / 2; r++)
                {
                    try
                    {
                        grid[cardinalTranslate(originCorner[0], originDir, f, r)] = true;
                    }
                    catch (KeyNotFoundException) { }
                }
            }
            pathsAdded++;
        }

        //Generate buildings
        Vector3 parentPosition;
        int randomHeight;
        float smoothHeight;
        float normalisedDistance;
        float maxDistance;
        GameObject raycatcher;
        BoxCollider raycatcherCollider;
        List<int> landmarks = new List<int>();
        for (int i = 0; i < buildingPrefabs.Count; i++)
        {
            if (buildingPrefabs[i].tag == "Landmark")
            {
                for (int j = 0; j < instancesPerLandmark; j++)
                {
                    landmarks.Add(i);
                }
            }
        }
        GameObject instantiatedBuilding;
        //For the height randomness
        float distanceFromOrigin;
        float distanceFactor;
        float randomFactor;
        float heightOffset;

        bool firstBuilding = true;
        bool isPossibility = true;

        int numOfLandmarks = 0;
        int numOfGenerics = 0;

        foreach (List<int> originCoords in gridCells)
        {
            possibilities = new List<int>();
            originX = originCoords[0];
            originY = originCoords[1];
            if (!(CheckCoords(grid, originX, originY)))
            {
                //Determine possibilities
                for (int i = 0; i < buildingPrefabs.Count; i++)
                {
                    isPossibility = false;
                    if (landmarks.Count != 0)
                    {
                        if (landmarks.Contains(i))
                        {
                            isPossibility = true;
                        }
                    }
                    else
                    {
                        if (buildingPrefabs[i].tag != "Landmark")
                        {
                            isPossibility = true;
                        }
                    }

                    //This section of code attempts to achieve a certain ratio of landmark buildings to non-landmark buildings - it's a bad way of doing this
                    /*
                    else
                    {
                        if (buildingPrefabs[i].tag == "Landmark")
                        {
                            if (numOfLandmarks / (float) (numOfLandmarks + numOfGenerics) <= landmarkRatio)
                            {
                                isPossibility = true;
                            }
                        }
                        else
                        {
                            if (numOfLandmarks / (float) (numOfLandmarks + numOfGenerics) >= landmarkRatio)
                            {
                                isPossibility = true;
                            }
                        }
                    }
                    */

                    if (isPossibility)
                    {
                        possibilities.Add(i);
                        for (int x = 0; x < prefabDimensions[i][0] + gap * 2; x++)
                        {
                            for (int y = 0; y < prefabDimensions[i][1] + gap * 2; y++)
                            {
                                if (CheckCoords(grid, originX + x, originY + y))
                                {
                                    possibilities.RemoveAt(possibilities.Count - 1);
                                    x = prefabDimensions[i][0] + gap * 2;
                                    break;
                                }
                            }
                        }
                    }
                }

                //Fiddle with the value if city generation is too slow
                if (Time.realtimeSinceStartup - startTime > (1f / 30f))
                {
                    yield return null;
                    startTime = Time.realtimeSinceStartup;
                }

                if (possibilities.Count != 0)
                {
                    index = possibilities[Random.Range(0, possibilities.Count)];
                    if (buildingPrefabs[index].tag == "Landmark")
                    {
                        if (landmarks.Contains(index))
                        {
                            landmarks.Remove(index);
                        }
                        numOfLandmarks++;
                    }
                    else
                    {
                        numOfGenerics++;
                    }
                    //Instantiate raycatcher
                    raycatcher = Instantiate(
                        raycatcherPrefab,
                        new Vector3(
                            (originX + gap - (gridX / 2f)) * 10f + prefabDimensions[index][0] * 5f,
                            0,
                            (originY + gap - (gridY / 2f)) * 10f + prefabDimensions[index][1] * 5f
                            ),
                        Quaternion.identity
                        );
                    raycatcherCollider = raycatcher.GetComponent<BoxCollider>();
                    raycatcherCollider.center = new Vector3(
                        0,
                        150,
                        0
                        );
                    raycatcherCollider.size = new Vector3(
                        prefabDimensions[index][0] * 10f,
                        300,
                        prefabDimensions[index][1] * 10f
                        );

                    //Instantiate building
                    instantiatedBuilding = Instantiate(
                        buildingPrefabs[index],
                        new Vector3(
                            raycatcher.transform.position.x - prefabOffsets[index][0],
                            0,
                            raycatcher.transform.position.z - prefabOffsets[index][1]
                            ),
                        Quaternion.identity,
                        raycatcher.transform
                        );

                    //If the building is a generic building, give it a random height
                    if (instantiatedBuilding.tag == "Generic Building")
                    {
                        distanceFromOrigin = Mathf.Sqrt(Mathf.Pow(instantiatedBuilding.transform.position.x / (mapSizeX / 2), 2)
                            + Mathf.Pow(instantiatedBuilding.transform.position.z / (mapSizeY / 2), 2));
                        distanceFactor = Mathf.Clamp(distanceFromOrigin, 0f, 1f);
                        randomFactor = Random.Range(0f, 1f);
                        heightOffset = (randomFactor * heightRandomness + distanceFactor * (1 - heightRandomness)) * maxHeightOffset;
                        instantiatedBuilding.transform.position += new Vector3(0, -heightOffset, 0);
                    }
                    else
                    {
                        heightOffset = 0f;
                    }

                    if (firstBuilding && buildingPrefabs[index].tag != "Landmark")
                    {
                        GameObject.Find("FPS_Player").transform.position = new Vector3(
                            raycatcher.transform.position.x,
                            trueBuildingDimensions[index][2] - heightOffset + 1,
                            raycatcher.transform.position.z
                            );
                        firstBuilding = false;
                    }

                    //Fill in grid
                    for (int x = gap; x < prefabDimensions[index][0] + gap; x++)
                    {
                        for (int y = gap; y < prefabDimensions[index][1] + gap; y++)
                        {
                            grid[(originX + x).ToString() + " " + (originY + y).ToString()] = true;
                        }
                    }
                }
            }
        }
    }
}
