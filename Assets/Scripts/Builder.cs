using System;
using System.Collections.Generic;
using UnityEngine;

public class Builder : MonoBehaviour
{
    [Range(200, 1000)]
    public int xlength = 400;

    [Range(200, 1000)]
    public int zlength = 400;

    [Range(1, 35)]
    public int nodeCount = 10;

    [Range(0, 20)]
    public int maxFloor = 5;

    public Material targetMaterial;

    [Range(0.1f, 10f)]
    public float windowX = 1f;

    [Range(0.1f, 10f)]
    public float windowY = 1f;

    List<Vector2Int> pathPoints = new List<Vector2Int>();
    List<Vector2Int> cityCoordinates = new List<Vector2Int>();

    System.Random rand = new System.Random();
    public GameObject strightRoad;
    public GameObject curveRoad;
    public GameObject fourWayRoad;
    public GameObject tRoad;
    public List<GameObject> floorPrefabs;

    float timer = 0f;
    float checkInterval = 1f;
    int prevXLength;
    int prevZLength;
    int prevNodeCount;
    int prevMaxFloor;
    public Transform buildingsParent;


    void Start()
    {
        SaveCurrentValues();
        randompath();
        randomCity();

    }


    void Update()
    {
        timer += Time.deltaTime;

        if (targetMaterial != null)
        {
            targetMaterial.mainTextureScale = new Vector2(windowX, windowY);
        }

        if (timer >= checkInterval)
        {
            timer = 0f;

            if (HasValueChanged())
            {
                SaveCurrentValues();
                ClearAllClones();
                randompath();
                randomCity();
               
            }
        }
    }

    void SaveCurrentValues()
    {
        prevXLength = xlength;
        prevZLength = zlength;
        prevNodeCount = nodeCount;
        prevMaxFloor = maxFloor;
    }

    bool HasValueChanged()
    {
        return prevXLength != xlength || prevZLength != zlength ||
               prevNodeCount != nodeCount || prevMaxFloor != maxFloor;
    }

    void ClearAllClones()
    {
        foreach (Transform child in buildingsParent)
        {
            Destroy(child.gameObject);
        }
    }

    void randomCity()
    {
        cityCoordinates.Clear(); 
        int multipx= xlength / 10;
        int multipz = zlength / 10;

        for (int z = multipz; z <= multipz*9; z += multipz)
        {
            for (int x = multipx; x <= multipx*9; x += multipx)
            {
                Vector2Int coord = new Vector2Int(x, z);
                cityCoordinates.Add(coord);
                

                
                int floorCount = rand.Next(0, maxFloor + 1);

                for (int i = 0; i < floorCount; i++)
                {
                    Vector3 position = new Vector3(coord.x, i * 12f, coord.y);
                    GameObject floorPrefab = floorPrefabs[rand.Next(floorPrefabs.Count)];
                    GameObject clone = Instantiate(floorPrefab, position, Quaternion.identity);
                    clone.transform.parent = buildingsParent;
                }
            }
        }

        
    }

    void randompath()
    {
        pathPoints.Clear();
        int z, x;
        int temp1 = nodeCount;
        int multx= xlength / 40 -1 ;
        int multz = zlength / 40 - 1;

       
        while (temp1 > 0)
        {
            Vector2Int newPoint;

            do
            {
                z = rand.Next(0, multx);
                x = rand.Next(0, multz);
                newPoint = new Vector2Int(20 + x * 40, 20 + z * 40);
            }
            while (pathPoints.Contains(newPoint));

            pathPoints.Add(newPoint);
            temp1--;
        }

   
        string allPoints = string.Join(", ", pathPoints);
        

        
        List<Vector2Int> fullPath = new List<Vector2Int>();
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Vector2Int start = pathPoints[i];
            Vector2Int end = pathPoints[i + 1];
            Vector2Int current = start;
            fullPath.Add(current);

            while (current.x != end.x)
            {
                current.x += (end.x > current.x) ? 40 : -40;
                fullPath.Add(new Vector2Int(current.x, current.y));
            }
            while (current.y != end.y)
            {
                current.y += (end.y > current.y) ? 40 : -40;
                fullPath.Add(new Vector2Int(current.x, current.y));
            }
        }
        fullPath.Add(pathPoints[pathPoints.Count - 1]);


        HashSet<Vector2Int> uniqueSet = new HashSet<Vector2Int>(fullPath);
        List<Vector2Int> finalPath = new List<Vector2Int>(uniqueSet);
        finalPath.Sort((a, b) => fullPath.IndexOf(a).CompareTo(fullPath.IndexOf(b)));

  
        foreach (Vector2Int current in finalPath)
        {
            bool hasUp = finalPath.Contains(new Vector2Int(current.x, current.y + 40));
            bool hasDown = finalPath.Contains(new Vector2Int(current.x, current.y - 40));
            bool hasLeft = finalPath.Contains(new Vector2Int(current.x - 40, current.y));
            bool hasRight = finalPath.Contains(new Vector2Int(current.x + 40, current.y));

            GameObject prefabToPlace;
            Quaternion rotation = Quaternion.identity;

            int connectionCount = 0;
            if (hasUp) connectionCount++;
            if (hasDown) connectionCount++;
            if (hasLeft) connectionCount++;
            if (hasRight) connectionCount++;

            if (connectionCount == 4)
            {
                prefabToPlace = fourWayRoad;
            }
            else if (connectionCount == 3)
            {
                prefabToPlace = tRoad;

                
                if (!hasRight) 
                    rotation = Quaternion.Euler(0, 0, 0);
                else if (!hasDown) 
                    rotation = Quaternion.Euler(0, 90, 0);
                else if (!hasLeft) 
                    rotation = Quaternion.Euler(0, 180, 0);
                else if (!hasUp) 
                    rotation = Quaternion.Euler(0, 270, 0);
            }
            else if (connectionCount == 2)
            {
                if ((hasLeft && hasRight) || (hasUp && hasDown))
                {
                    prefabToPlace = strightRoad;
                    rotation = (hasLeft && hasRight) ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    prefabToPlace = curveRoad;

                    if (hasUp && hasRight)           
                        rotation = Quaternion.Euler(0, 0, 0);
                    else if (hasRight && hasDown)   
                        rotation = Quaternion.Euler(0, 90, 0);
                    else if (hasDown && hasLeft)     
                        rotation = Quaternion.Euler(0, 180, 0);
                    else if (hasLeft && hasUp)       
                        rotation = Quaternion.Euler(0, 270, 0);
                }
            }
            else
            {
                prefabToPlace = strightRoad; 
            }

            GameObject clone = Instantiate(prefabToPlace, new Vector3(current.x, 1, current.y), rotation);
            clone.transform.parent = buildingsParent;
        }

    }
   

}




