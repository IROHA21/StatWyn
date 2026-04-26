using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class glowClick : MonoBehaviour
{
    // ========== INSPECTOR SETTINGS ==========
    [Header("Glow Settings")]
    public Color glowColor = Color.yellow;      // Color of the glow (yellow, green, red, etc.)
    [Range(0, 1)] public float glowOpacity = 0.5f;  // How transparent (0 = invisible, 1 = solid)
    public float glowDuration = 2f;             // How many seconds the glow lasts

    // ========== PRIVATE VARIABLES ==========
    private Texture2D glowTexture;              // The texture we draw glow on (transparent layer)
    private Texture2D sourceTexture;            // The original map texture (to get dimensions)
    private Coroutine currentGlowCoroutine;     // Reference to the pulsing animation
    private Dictionary<string, List<Vector2Int>> provincePixels;  // YOUR pixel data: hex → list of pixel coordinates

    // ========== START ==========
    void Start()
    {
        // Step 1: Create the transparent layer where glow will be drawn
        CreateGlowLayer();
        
        // Step 2: Get the pixel data you already loaded in BorderPixelLoader
        provincePixels = BorderPixelLoader.provincesPixels;
        
        // Step 3: Confirm it loaded
       // Debug.Log($"Loaded {provincePixels.Count} provinces for glow");
    }

    // ========== CREATE THE GLOW LAYER (TRANSPARENT QUAD OVER MAP) ==========
    void CreateGlowLayer()
    {
        // Get the map's current texture to know width/height
        Renderer renderer = GetComponent<Renderer>();
        sourceTexture = (Texture2D)renderer.material.mainTexture;
        
        //Debug.Log($"CreateGlowLayer: Texture size = {sourceTexture.width} x {sourceTexture.height}");

        // Create a new empty texture (same size as map) - this will hold the glow
        glowTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        glowTexture.filterMode = FilterMode.Point;  // Sharp pixels, no blurring

        // Fill it with completely transparent pixels (invisible)
        Color[] transparent = new Color[sourceTexture.width * sourceTexture.height];
        for (int i = 0; i < transparent.Length; i++)
            transparent[i] = Color.clear;

        glowTexture.SetPixels(transparent);
        glowTexture.Apply();  // Saves the changes to the texture

        // Create a physical Quad (flat square) to display the glow texture
        GameObject glowQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glowQuad.transform.SetParent(transform);  // Attach to map
        Destroy(glowQuad.GetComponent<Collider>());  // Remove collider so clicks pass through
        
        // Rotate quad to lie flat on the ground (default Quad faces camera)
        glowQuad.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        // Position exactly at map center, slightly above to avoid z-fighting
        glowQuad.transform.localPosition = new Vector3(0, 0.2f, 0);
        
        // Scale to match map size (adjust 10 to your map scale)
        glowQuad.transform.localScale = new Vector3(10, 10, 1);
        
        // Create material that can show transparency
        Material glowMat = new Material(Shader.Find("Unlit/Transparent"));
        glowMat.mainTexture = glowTexture;
        glowMat.mainTexture.filterMode = FilterMode.Point;
        glowQuad.GetComponent<Renderer>().material = glowMat;
        
       // Debug.Log($"CreateGlowLayer: Quad created at position {glowQuad.transform.position}");
    }

    // ========== PUBLIC FUNCTION: START GLOWING A PROVINCE ==========
    public void GlowRegion(string hexColor)
    {
        // Stop any existing glow animation
        if (currentGlowCoroutine != null)
            StopCoroutine(currentGlowCoroutine);

        // Clear any existing glow pixels
        ClearGlow();
        
        // Start the pulsing animation
        currentGlowCoroutine = StartCoroutine(PulseGlow(hexColor));
    }

    // ========== THE PULSING ANIMATION ==========
    IEnumerator PulseGlow(string hexColor)
    {
        // Clear any leftover glow
        ClearGlow();

        // Check if we have pixel data for this province
        if (!provincePixels.ContainsKey(hexColor))
        {
            Debug.LogWarning($"No pixel data for {hexColor}");
            yield break;
        }

        // Get all pixels that belong to this province (THE BUCKET FILL)
        List<Vector2Int> pixels = provincePixels[hexColor];
        float elapsed = 0f;
        
        // Loop while the glow should be visible
        while (elapsed < glowDuration)
        {
            // Clear previous frame's glow (to create pulsing effect)
            ClearGlow();
            
            // Calculate current opacity (pulses between 0.7 and 1.0 of original opacity)
            // Mathf.Sin goes from -1 to 1, so we convert to 0.7-1.3 range, then multiply by glowOpacity
            float pulse = glowOpacity * (0.7f + Mathf.Sin(elapsed * 10f) * 0.3f);
            
            // Draw glow on EVERY pixel of this province
            foreach (Vector2Int pos in pixels)
            {
                // FLIP COORDINATES to match click detection
                // Your click detection uses (1f - u) and (1f - v), so we flip both axes
                int flippedX = sourceTexture.width - 1 - pos.x;
                int flippedY = sourceTexture.height - 1 - pos.y;
                
                // Make sure coordinates are inside texture bounds
                if (flippedX >= 0 && flippedX < glowTexture.width && 
                    flippedY >= 0 && flippedY < glowTexture.height)
                {
                    // Create the glow color with current pulse opacity
                    Color glowy = glowColor;
                    glowy.a = pulse;
                    
                    // Draw ONE pixel (no expansion, exact province shape)
                    glowTexture.SetPixel(flippedX, flippedY, glowy);
                }
            }
            
            // Apply all pixel changes to the texture
            glowTexture.Apply();
            
            // Wait for next frame
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Glow duration finished - clear it
        ClearGlow();
        currentGlowCoroutine = null;
    }

    // ========== DRAW GLOW AT A SINGLE PIXEL (UNUSED, KEPT FOR REFERENCE) ==========
    void AddGlowAt(int x, int y)
    {
        if (x >= 0 && x < glowTexture.width && y >= 0 && y < glowTexture.height)
        {
            Color glowy = glowColor;
            glowy.a = glowOpacity;
            glowTexture.SetPixel(x, y, glowy);
        }
    }

    // ========== CLEAR ALL GLOW FROM THE TEXTURE ==========
    public void ClearGlow()
    {
        if (glowTexture == null) return;

        // Create an array of completely transparent pixels
        Color[] clear = new Color[glowTexture.width * glowTexture.height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;

        // Apply to the texture
        glowTexture.SetPixels(clear);
        glowTexture.Apply();
    }
}