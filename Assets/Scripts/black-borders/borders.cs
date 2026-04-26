using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderDrawer : MonoBehaviour
{
    [Header("Border Settings")]
    public Color borderColor = Color.black;
    [Range(0, 1)] public float borderOpacity = 1f;
    [Range(0, 3)] public float borderThickness = 1f;  // Float now!
    public GameObject targetVisualObject;
    
    private Texture2D borderTexture;
    private Texture2D sourceTexture;
    private GameObject borderQuad;
    
    void Start()
    {
        StartCoroutine(DrawPermanentBorders());
    }
    
    IEnumerator DrawPermanentBorders()
    {
        while (BorderPixelLoader.provincesPixels == null || 
               BorderPixelLoader.provincesPixels.Count == 0)
        {
            yield return null;
        }
        
        Renderer renderer = GetComponent<Renderer>();
        sourceTexture = (Texture2D)renderer.material.mainTexture;
        
        borderTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        borderTexture.filterMode = FilterMode.Point;
        
        Color[] transparent = new Color[sourceTexture.width * sourceTexture.height];
        for (int i = 0; i < transparent.Length; i++)
            transparent[i] = Color.clear;
        borderTexture.SetPixels(transparent);
        
        DrawAllBorders();
        borderTexture.Apply();
        
        borderQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        
        if (targetVisualObject != null)
            borderQuad.transform.SetParent(targetVisualObject.transform);
        else
            borderQuad.transform.SetParent(transform);
        
        Destroy(borderQuad.GetComponent<Collider>());
        borderQuad.transform.localRotation = Quaternion.Euler(90, 0, 0);
        borderQuad.transform.localPosition = new Vector3(0, 0.1f, 0);
        borderQuad.transform.localScale = new Vector3(10, 10, 1);
        
        Material borderMat = new Material(Shader.Find("Unlit/Transparent"));
        borderMat.mainTexture = borderTexture;
        borderMat.mainTexture.filterMode = FilterMode.Point;
        borderQuad.GetComponent<Renderer>().material = borderMat;
        
        Debug.Log($"Borders drawn: Color={borderColor}, Opacity={borderOpacity}, Thickness={borderThickness}");
    }
    
    void DrawAllBorders()
    {
        int borderCount = 0;
        
        // Create color with opacity
        Color finalColor = new Color(borderColor.r, borderColor.g, borderColor.b, borderOpacity);
        
        // Calculate radius based on float thickness
        float radius = borderThickness / 2f;
        int minRadius = Mathf.FloorToInt(radius);
        int maxRadius = Mathf.CeilToInt(radius);
        
        foreach (var entry in BorderPixelLoader.provincesPixels)
        {
            List<Vector2Int> pixels = entry.Value;
            
            foreach (Vector2Int pixel in pixels)
            {
                if (IsBorderPixel(pixel.x, pixel.y))
                {
                    int centerX = sourceTexture.width - 1 - pixel.x;
                    int centerY = sourceTexture.height - 1 - pixel.y;
                    
                    // Draw a circle of pixels for smooth thickness
                    for (int dx = -maxRadius; dx <= maxRadius; dx++)
                    {
                        for (int dy = -maxRadius; dy <= maxRadius; dy++)
                        {
                            float distance = Mathf.Sqrt(dx * dx + dy * dy);
                            
                            // Only draw pixels within the radius
                            if (distance <= radius + 0.5f)
                            {
                                int nx = centerX + dx;
                                int ny = centerY + dy;
                                
                                if (nx >= 0 && nx < sourceTexture.width && 
                                    ny >= 0 && ny < sourceTexture.height)
                                {
                                    borderTexture.SetPixel(nx, ny, finalColor);
                                    borderCount++;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log($"Drew {borderCount} border pixels");
    }
    
    bool IsBorderPixel(int x, int y)
    {
        Color currentColor = sourceTexture.GetPixel(x, y);
        
        if (x + 1 < sourceTexture.width)
        {
            Color rightColor = sourceTexture.GetPixel(x + 1, y);
            if (!ColorsMatch(currentColor, rightColor))
                return true;
        }
        
        if (x - 1 >= 0)
        {
            Color leftColor = sourceTexture.GetPixel(x - 1, y);
            if (!ColorsMatch(currentColor, leftColor))
                return true;
        }
        
        if (y + 1 < sourceTexture.height)
        {
            Color upColor = sourceTexture.GetPixel(x, y + 1);
            if (!ColorsMatch(currentColor, upColor))
                return true;
        }
        
        if (y - 1 >= 0)
        {
            Color downColor = sourceTexture.GetPixel(x, y - 1);
            if (!ColorsMatch(currentColor, downColor))
                return true;
        }
        
        return false;
    }
    
    bool ColorsMatch(Color a, Color b)
    {
        float tolerance = 0.01f;
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}