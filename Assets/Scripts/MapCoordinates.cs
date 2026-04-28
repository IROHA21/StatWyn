using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;  

public class MapCoordinates : MonoBehaviour
{
    // dictionary for quick lookup of province data by hex color
    Dictionary<string, ProvinceData> provinceLookup = new Dictionary<string, ProvinceData>();

    // Dictionaries for regions and countries
    public Dictionary<string, RegionData> regionLookup = new Dictionary<string, RegionData>();
    public Dictionary<string, CountryData> countryLookup = new Dictionary<string, CountryData>();

    // File references
    public TextAsset regionGroupsFile;
    public TextAsset countryGroupsFile;
    public TextAsset provinceDataFile;
    public TextAsset centerDataFile;
    public TextAsset provincePixelsFile;

    // Map and units
    private Texture2D mapTexture;
    public GameObject unitPrefab;
    private Unit CurrentUnit;
    private ProvinceData currentProvinceID;
    private glowClick highlighter;
    
    // Pathfinding variables
    private List<ProvinceData> currentPath;
    private bool isMovingAlongPath = false;
    
    void Start()
    {
        // Load provinces (tiles)
        ProvinceLoader.LoadProvincesFromFile(provinceDataFile, provinceLookup);
        
        // Load regions
        RegionLoader.LoadRegionsFromFile(regionGroupsFile, regionLookup);
        
        // Load countries
        CountryLoader.LoadCountriesFromFile(countryGroupsFile, countryLookup);

        // Load centers
        LoadCentersFromFile();
        
        // Get texture
        Renderer myRenderer = GetComponent<Renderer>();
        mapTexture = (Texture2D)myRenderer.material.mainTexture;
        
        // Make logic map invisible
        myRenderer.enabled = false;
        
        // Show centers (optional)
        ShowAllCenters();
        
        // Spawn unit
        SpawnUnit spawnUnit = new SpawnUnit();
        CurrentUnit = spawnUnit.spawnUnit("#4A6FD9", "Knight", provinceLookup, unitPrefab);
        
        if (CurrentUnit != null && provinceLookup.ContainsKey("#4A6FD9"))
        {
            currentProvinceID = provinceLookup["#4A6FD9"];
        }
        
        // Load pixel data for highlighting
        BorderPixelLoader.LoadPixelsFromFile(provincePixelsFile);
        
        // Setup highlighter and pass hierarchy data
        highlighter = GetComponent<glowClick>();
        if (highlighter == null)
            highlighter = gameObject.AddComponent<glowClick>();
        
        // Pass the hierarchy data to glowClick
        highlighter.Initialize(provinceLookup, regionLookup, countryLookup);
        
        // Initialize pathfinding manager
        PathfindingManager.Initialize(provinceLookup, regionLookup, countryLookup);
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == this.gameObject)
            {
                // Manual UV calculation for Plane
                Vector3 localPoint = transform.InverseTransformPoint(hit.point);
                Vector3 size = GetComponent<Renderer>().bounds.size;
                
                float u = 1f - (localPoint.x + size.x * 0.5f) / size.x;
                float v = 1f - ((localPoint.z + size.z * 0.5f) / size.z);
                
                u = Mathf.Clamp01(u);
                v = Mathf.Clamp01(v);
                
                int x = (int)(u * mapTexture.width);
                int y = (int)(v * mapTexture.height);
                
                Color clickedColor = mapTexture.GetPixel(x, y);
                string clickedHex = ProvinceLoader.ColorToHex(clickedColor);
                
                if (provinceLookup.ContainsKey(clickedHex))
                {
                    ProvinceData clickedProvince = provinceLookup[clickedHex];
                    
                    // GLOW BASED ON INSPECTOR MODE
                    switch (highlighter.glowMode)
                    {
                        case glowClick.GlowMode.Tile:
                            highlighter.GlowRegion(clickedHex);
                            break;
                        case glowClick.GlowMode.Region:
                            string regionID = GetRegionIDForTile(clickedProvince.provinceID);
                            if (regionID != null)
                                highlighter.GlowByRegionID(regionID);
                            break;
                        case glowClick.GlowMode.Country:
                            highlighter.GlowByCountryID(clickedProvince.Country);
                            break;
                    }
                    
                    // Movement logic using pathfinding
                    if (currentProvinceID != null && !isMovingAlongPath)
                    {
                        List<ProvinceData> path = PathfindingManager.GetPath(currentProvinceID.provinceID, clickedProvince.provinceID);
                        
                        if (path.Count > 0)
                        {
                            // Store the full path and start following it
                            currentPath = path;
                            StartCoroutine(FollowPath());
                        }
                        else
                        {
                            Debug.Log($"Cannot move to {clickedProvince.provinceID} - no path found");
                        }
                    }
                    else if (isMovingAlongPath)
                    {
                        Debug.Log("Unit is already moving, please wait");
                    }
                }
                else
                {
                    Debug.Log("Unknown territory");
                }
            }
        }
    }
    
    // Follow the path step by step
    IEnumerator FollowPath()
    {
        isMovingAlongPath = true;
        
        for (int i = 0; i < currentPath.Count; i++)
        {
            ProvinceData nextTile = currentPath[i];
            Vector3 targetPosition = getInfo.GetProvinceWorldPosition(nextTile.hexColor, provinceLookup);
            
            // Move to this tile
            CurrentUnit.MoveToProvince(nextTile.provinceID, targetPosition);
            currentProvinceID = nextTile;
            
            // Wait until unit finishes moving to this tile
            while (CurrentUnit.IsMoving())
            {
                yield return null;
            }
        }
        
        // Path complete
        isMovingAlongPath = false;
        currentPath = null;
        Debug.Log("Unit reached destination");
    }

    void LoadCentersFromFile()
    {
        if (centerDataFile == null)
        {
            Debug.LogWarning("Center data file not assigned!");
            return;
        }
        
        string[] lines = centerDataFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            string[] parts = line.Split('|');
            if (parts.Length == 4)
            {
                string hexColor = parts[0].Trim();
                float x = float.Parse(parts[1]);
                float y = float.Parse(parts[2]);
                float z = float.Parse(parts[3]);
                
                if (provinceLookup.ContainsKey(hexColor))
                {
                    provinceLookup[hexColor].centerPosition = new Vector3(x, y, z);
                }
            }
        }
    }

    void ShowAllCenters()
    {
        foreach (var entry in provinceLookup)
        {
            ProvinceData data = entry.Value;
            
            if (data.centerPosition != Vector3.zero)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.position = data.centerPosition;
                marker.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                marker.transform.SetParent(this.transform);
                
                Renderer rend = marker.GetComponent<Renderer>();
                
                if (data.Country == "France")
                    rend.material.color = Color.blue;
                else if (data.Country == "Germany")
                    rend.material.color = Color.red;
                else
                    rend.material.color = Color.green;
                
                Destroy(marker.GetComponent<Collider>());
            }
        }
    }

    private string GetRegionIDForTile(string tileID)
    {
        foreach (var region in regionLookup.Values)
        {
            if (region.tiles.Contains(tileID))
                return region.regionID;
        }
        return null;
    }
}