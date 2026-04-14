using UnityEngine;
using System.Collections.Generic;




public class provinceHighlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color HighlightColor = Color.green;
    public float glowSize = 0.05f;
    public float glowHeight = 0.1f;

    private Texture2D mapTexture;
    private GameObject currentHighlight;

    void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        mapTexture = (Texture2D)rend.material.mainTexture;
    }

    public void HighlightProvince(string hexColor)
    {
        ClearHighlight();

        // Debug: Show what we're looking for and what's available
        Debug.Log($"HighlightProvince called with: '{hexColor}' (length: {hexColor.Length})");
        Debug.Log($"Available keys in pixelData: {string.Join(", ", BorderPixelLoader.provincesPixels.Keys)}");

        if(!BorderPixelLoader.provincesPixels.ContainsKey(hexColor))
        {
            Debug.LogWarning($"No pixel data for color {hexColor}");
            return;
        }

        List<Vector2Int> pixels = BorderPixelLoader.provincesPixels[hexColor];

        GameObject highlightGroup = new GameObject($"Highlight_{hexColor}");
        highlightGroup.transform.SetParent(this.transform);
        
        // Create a glow dot at each pixel position
        foreach (Vector2Int pixel in pixels)
        {
            // Convert pixel to world position
            Vector3 worldPos = PixelToWorldPosition(pixel.x, pixel.y);
            
            // Create tiny sphere
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.transform.position = worldPos + Vector3.up * glowHeight;
            dot.transform.localScale = Vector3.one * glowSize;
            dot.transform.SetParent(highlightGroup.transform);
            
            // Set color
            dot.GetComponent<Renderer>().material.color = HighlightColor;
            
            // Remove collider so it doesn't block clicks
            Destroy(dot.GetComponent<Collider>());
        }
        
        currentHighlight = highlightGroup;
        Debug.Log($"Highlighted province with {pixels.Count} dots");
    }
    
    public void ClearHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
            currentHighlight = null;
        }
    }
    
    Vector3 PixelToWorldPosition(int x, int y)
{
    // Convert pixel to UV (0-1 range)
    float u = x / (float)mapTexture.width;
    float v = y / (float)mapTexture.height;
    
    // Apply SAME flips as click detection
    u = 1f - u;
    v = 1f - v;
    
    // Convert UV to world position on plane
    Vector3 worldPos = transform.position;
    Vector3 size = GetComponent<Renderer>().bounds.size;
    
    worldPos.x = (u - 0.5f) * size.x;
    worldPos.z = (v - 0.5f) * size.z;
    
    return worldPos;
}
}