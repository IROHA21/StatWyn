using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCoordinates : MonoBehaviour
{
    // CHANGED: Dictionary key is now string (hex), not Color
    Dictionary<string, string> provinceLookup = new Dictionary<string, string>();

    public TextAsset provinceDataFile;
    
    void Start()
    {
        // Load provinces (now stores hex -> name)
        ProvinceLoader.LoadProvincesFromFile(provinceDataFile, provinceLookup);
        
        Renderer myRenderer = GetComponent<Renderer>();
        Texture2D myTexture = (Texture2D)myRenderer.material.mainTexture;

        Debug.Log($"Texture is {myTexture.width} wide and {myTexture.height} high");

        // CHANGED: Debug shows hex keys
        foreach (KeyValuePair<string, string> entry in provinceLookup)
        {
            Debug.Log($"Hex: {entry.Key} - Province: {entry.Value}");
        }
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.collider.gameObject == this.gameObject)
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();
                Vector3 localPoint = rend.transform.InverseTransformPoint(hit.point);
                Vector3 size = rend.bounds.size;
                
                float u = (localPoint.x + size.x * 0.5f) / size.x;
                float v = (localPoint.y + size.y * 0.5f) / size.y;
                
                u = Mathf.Clamp01(u);
                v = Mathf.Clamp01(v);
                
                Texture2D tex = (Texture2D)rend.material.mainTexture;
                int x = (int)(u * tex.width);
                int y = (int)(v * tex.height);
                Color clickedColor = tex.GetPixel(x, y);
                
                // CHANGED: Convert clicked color to hex
                string clickedHex = ProvinceLoader.ColorToHex(clickedColor);
                
                // CHANGED: Look up by hex string
                string provinceName = GetProvinceName(clickedHex);
                
                // CHANGED: Debug shows hex
                Debug.Log($"Clicked on: {provinceName} - Hex: {clickedHex} - RGB({clickedColor.r:F2}, {clickedColor.g:F2}, {clickedColor.b:F2})");
            }
        }
    }

    // CHANGED: Takes hex string, not Color
    string GetProvinceName(string hexColor)
    {
        if (provinceLookup.ContainsKey(hexColor))
            return provinceLookup[hexColor];
        return "unknown";
    }
}