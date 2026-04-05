using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCoordinates : MonoBehaviour
{

    Dictionary<Color, string> myDictionary = new Dictionary<Color, string>();

    public TextAsset provinceDataFile;
    
    // Start is called before the first frame update
    void Start()
    {

        ProvinceLoader.LoadProvincesFromFile(provinceDataFile, myDictionary);
        
        

        Renderer myRenderer = GetComponent<Renderer>();
        Texture2D myTexture = (Texture2D)myRenderer.material.mainTexture;


        Debug.Log($"the texture is {myTexture.width} wide and {myTexture.height} high");

        Debug.Log($"i want to understand what the hell is {myRenderer} and {myTexture}");
        foreach (KeyValuePair<Color, string> entry in myDictionary)
        {
        Debug.Log($"Color: R={entry.Key.r}, G={entry.Key.g}, B={entry.Key.b} - Value: {entry.Value}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit ;

            if (Physics.Raycast(ray, out hit ) && hit.collider.gameObject == this.gameObject)
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();
                Vector3 localPoint = rend.transform.InverseTransformPoint(hit.point);
                Vector3 size = rend.bounds.size;
                
                // Convert local position to 0-1 UV range using local extents
                float u = (localPoint.x + size.x * 0.5f) / size.x;
                float v = (localPoint.y + size.y * 0.5f) / size.y;
                
                // Clamp to prevent overflow
                u = Mathf.Clamp01(u);
                v = Mathf.Clamp01(v);
                
                Texture2D tex = (Texture2D)rend.material.mainTexture;
                int x = (int)(u * tex.width);
                int y = (int)(v * tex.height);
                Color clickedColor = tex.GetPixel(x, y);

                string colorName = GetColorName(clickedColor);
                Debug.Log($"Clicked on: {colorName} - Color: {clickedColor}");
            
            }



        }
    }


    string GetColorName(Color color) {
    // Loop through dictionary to find matching color
    foreach (var entry in myDictionary)
    {
        if (entry.Key == color)
            return entry.Value;
    }
    return "unknown";
    }

    
}
