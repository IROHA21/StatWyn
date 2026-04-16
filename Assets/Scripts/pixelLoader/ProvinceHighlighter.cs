using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class glowClick : MonoBehaviour
{
    public Color glowColor = Color.yellow;
    [Range(0, 1)] public float glowOpacity = 0.5f;
    public float glowDuration = 2f;

    private Texture2D glowTexture;
    private Texture2D sourceTexture;
    private Coroutine currentGlowCoroutine;
    private Dictionary<string, List<Vector2Int>> provincePixels;

    void Start()
    {
        CreateGlowLayer();
        
        // USE YOUR EXISTING PIXEL DATA
        provincePixels = BorderPixelLoader.provincesPixels;
        Debug.Log($"Loaded {provincePixels.Count} provinces for glow");
    }

    void CreateGlowLayer()
{
    Renderer renderer = GetComponent<Renderer>();
    sourceTexture = (Texture2D)renderer.material.mainTexture;
    
    Debug.Log($"CreateGlowLayer: Texture size = {sourceTexture.width} x {sourceTexture.height}");

    glowTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
    glowTexture.filterMode = FilterMode.Point;

    Color[] transparent = new Color[sourceTexture.width * sourceTexture.height];
    for (int i = 0; i < transparent.Length; i++)
        transparent[i] = Color.clear;

    glowTexture.SetPixels(transparent);
    glowTexture.Apply();

    GameObject glowQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
    glowQuad.transform.SetParent(transform);
    Destroy(glowQuad.GetComponent<Collider>());
    glowQuad.transform.localRotation = Quaternion.Euler(90, 0, 0);
    glowQuad.transform.localPosition = new Vector3(0, 0, 0);  // Much higher
    //glowQuad.transform.localPosition = new Vector3(0, 0.05f, 0);
    glowQuad.transform.localScale = new Vector3(10, 10, 1); 
    



    Material glowMat = new Material(Shader.Find("Unlit/Transparent"));
    glowMat.mainTexture = glowTexture;
    glowMat.mainTexture.filterMode = FilterMode.Point;
    glowQuad.GetComponent<Renderer>().material = glowMat;
    
    Debug.Log($"CreateGlowLayer: Quad created at position {glowQuad.transform.position}");
}

    public void GlowRegion(string hexColor)
    {
        if (currentGlowCoroutine != null)
            StopCoroutine(currentGlowCoroutine);

        ClearGlow();
        currentGlowCoroutine = StartCoroutine(PulseGlow(hexColor));
    }

    IEnumerator PulseGlow(string hexColor)
{
    ClearGlow();

    if (!provincePixels.ContainsKey(hexColor))
    {
        Debug.LogWarning($"No pixel data for {hexColor}");
        yield break;
    }

    List<Vector2Int> pixels = provincePixels[hexColor];
    float elapsed = 0f;
    
    while (elapsed < glowDuration)
    {
        ClearGlow();
        
        float pulse = glowOpacity * (0.7f + Mathf.Sin(elapsed * 10f) * 0.3f);
        
        foreach (Vector2Int pos in pixels)
        {
            int flippedX = sourceTexture.width - 1 - pos.x;
            int flippedY = sourceTexture.height - 1 - pos.y;
            
            if (flippedX >= 0 && flippedX < glowTexture.width && 
                flippedY >= 0 && flippedY < glowTexture.height)
            {
                Color glowy = glowColor;
                glowy.a = pulse;
                glowTexture.SetPixel(flippedX, flippedY, glowy);
            }
        }
        
        glowTexture.Apply();
        elapsed += Time.deltaTime;
        yield return null;
    }

    ClearGlow();
    currentGlowCoroutine = null;
}

    void AddGlowAt(int x, int y)
{
    if (x >= 0 && x < glowTexture.width && y >= 0 && y < glowTexture.height)
    {
        Color glowy = glowColor;
        glowy.a = glowOpacity;
        glowTexture.SetPixel(x, y, glowy);
    }
}

    public void ClearGlow()
    {
        if (glowTexture == null) return;

        Color[] clear = new Color[glowTexture.width * glowTexture.height];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;

        glowTexture.SetPixels(clear);
        glowTexture.Apply();
    }
}