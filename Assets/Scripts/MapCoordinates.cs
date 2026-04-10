using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCoordinates : MonoBehaviour
{
    Dictionary<string, ProvinceData> provinceLookup = new Dictionary<string, ProvinceData>();
    public TextAsset provinceDataFile;
    private Texture2D mapTexture;
    
    void Start()
    {
        ProvinceLoader.LoadProvincesFromFile(provinceDataFile, provinceLookup);
        
        Renderer myRenderer = GetComponent<Renderer>();
        mapTexture = (Texture2D)myRenderer.material.mainTexture;
        
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
                    Debug.Log($"Clicked: {data.provinceID}");
                }
                else
                {
                    Debug.Log("Unknown territory");
                }
            }
        }
    }
}