using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class RecordCenter : MonoBehaviour
{
    private Texture2D mapTexture;
    private List<string> savedCenters = new List<string>();
    
    void Start()
    {
        // Get texture once
        Renderer myRenderer = GetComponent<Renderer>();
        mapTexture = (Texture2D)myRenderer.material.mainTexture;
        
        //Debug.Log("=== CENTER RECORDER ===");
        //Debug.Log("Press C to record center at mouse position");
        //Debug.Log("Press S to save all to file");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == this.gameObject)
            {
                // Get HEX color from click position
                Vector3 localPoint = transform.InverseTransformPoint(hit.point);
                Vector3 size = GetComponent<Renderer>().bounds.size;
                
                float u = 1f - (localPoint.x + size.x * 0.5f) / size.x;
                float v = 1f - ((localPoint.z + size.z * 0.5f) / size.z);
                
                u = Mathf.Clamp01(u);
                v = Mathf.Clamp01(v);
                
                int x = (int)(u * mapTexture.width);
                int y = (int)(v * mapTexture.height);
                
                Color clickedColor = mapTexture.GetPixel(x, y);
                string hexColor = ProvinceLoader.ColorToHex(clickedColor);
                
                // Get position
                Vector3 position = hit.point;
                
                // Save to list
                string line = $"{hexColor}|{position.x}|{position.y}|{position.z}";
                savedCenters.Add(line);
                
                // Create visual marker
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.position = position;
                marker.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                marker.GetComponent<Renderer>().material.color = Color.green;
                
                //Debug.Log($"<color=green>RECORDED #{savedCenters.Count}:</color>");
                //Debug.Log($"  Hex: {hexColor}");
                //Debug.Log($"  Position: {position}");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveToFile();
        }
    }
    
    void SaveToFile()
    {
        if (savedCenters.Count == 0)
        {
           // Debug.LogWarning("Nothing to save. Press C to record centers first.");
            return;
        }
        
        string path = Application.dataPath + "/ProvinceCenters.txt";
        File.WriteAllLines(path, savedCenters.ToArray());
        
       // Debug.Log($"<color=green>SAVED {savedCenters.Count} centers to: {path}</color>");
        //Debug.Log("File contents:");
        foreach (string line in savedCenters)
        {
            Debug.Log($"  {line}");
        }
    }
}