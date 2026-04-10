using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;  

public class MapCoordinates : MonoBehaviour
{
    Dictionary<string, ProvinceData> provinceLookup = new Dictionary<string, ProvinceData>();
    public TextAsset provinceDataFile;
    private Texture2D mapTexture;
    


    
    void Start()
    {
        ProvinceLoader.LoadProvincesFromFile(provinceDataFile, provinceLookup);


        LoadCentersFromFile();
        
        
        Renderer myRenderer = GetComponent<Renderer>();
        mapTexture = (Texture2D)myRenderer.material.mainTexture;
        
        ShowAllCenters();

        Debug.Log($"Texture is {mapTexture.width} x {mapTexture.height}");
        Debug.Log($"Loaded {provinceLookup.Count} provinces");

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
                
                Debug.Log($"UV: ({u:F3}, {v:F3}) -> Pixel: ({x}, {y}) -> Hex: {clickedHex}");
                
                if (provinceLookup.ContainsKey(clickedHex))
                {
                    ProvinceData data = provinceLookup[clickedHex];

                    string neighboorsList = string.Join(", ", data.neighbors);

                    Debug.Log($"Clicked: {data.provinceID}, the nighboors are :  {neighboorsList}");



                }
                else
                {
                    Debug.Log("Unknown territory");
                }
            }
        }
    }


    void LoadCentersFromFile()
    {
    string path = Application.dataPath + "/ProvinceCenters.txt";
    
    if (!File.Exists(path))
    {
        Debug.LogWarning($"Center file not found: {path}");
        return;
    }
    
    string[] lines = File.ReadAllLines(path);
    int loadedCount = 0;
    
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
                loadedCount++;
            }
            else
            {
                Debug.LogWarning($"No province found for hex: {hexColor}");
            }
        }
    }
    
    Debug.Log($"Loaded {loadedCount} center positions");
    }


    void ShowAllCenters()
{
    foreach (var entry in provinceLookup)
    {
        ProvinceData data = entry.Value;
        
        if (data.centerPosition != Vector3.zero)
        {
            // Create a small circle/sphere at the center
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = data.centerPosition;
            marker.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
            marker.transform.SetParent(this.transform); // Attach to map
            
            // Set color based on country or just green
            Renderer rend = marker.GetComponent<Renderer>();
            
            if (data.Country == "France")
                rend.material.color = Color.blue;
            else if (data.Country == "Germany")
                rend.material.color = Color.red;
            else
                rend.material.color = Color.green;
            
            // Remove collider so it doesn't block clicks
            Destroy(marker.GetComponent<Collider>());
            
            Debug.Log($"Created marker for {data.provinceID} at {data.centerPosition}");
        }
        else
        {
            Debug.LogWarning($"No center position for {data.provinceID}");
        }
    }
}
}