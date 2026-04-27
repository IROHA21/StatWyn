using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class glowClick : MonoBehaviour
{
    // ========== INSPECTOR SETTINGS ==========
    [Header("Glow Mode")]
    public GlowMode glowMode = GlowMode.Tile;
    
    [Header("Glow Settings")]
    public Color glowColor = Color.yellow;
    [Range(0, 1)] public float glowOpacity = 0.5f;
    public float glowDuration = 2f;

    public enum GlowMode
    {
        Tile,
        Region,
        Country
    }

    // ========== PRIVATE VARIABLES ==========
    private Texture2D glowTexture;
    private Texture2D sourceTexture;
    private Coroutine currentGlowCoroutine;
    private Dictionary<string, List<Vector2Int>> provincePixels;
    
    private Dictionary<string, ProvinceData> provinceLookup;
    private Dictionary<string, RegionData> regionLookup;
    private Dictionary<string, CountryData> countryLookup;
    private Dictionary<string, string> tileToHexLookup;

    // OPTIMIZATION: Cache texture dimensions
    private int texWidth;
    private int texHeight;

    public void Initialize(
        Dictionary<string, ProvinceData> pLookup,
        Dictionary<string, RegionData> rLookup,
        Dictionary<string, CountryData> cLookup)
    {
        provinceLookup = pLookup;
        regionLookup = rLookup;
        countryLookup = cLookup;
        
        tileToHexLookup = new Dictionary<string, string>();
        foreach (var entry in provinceLookup)
        {
            tileToHexLookup[entry.Value.provinceID] = entry.Key;
        }
    }

    void Start()
    {
        CreateGlowLayer();
        provincePixels = BorderPixelLoader.provincesPixels;
        
        // Cache dimensions
        texWidth = sourceTexture.width;
        texHeight = sourceTexture.height;
    }

    void CreateGlowLayer()
    {
        Renderer renderer = GetComponent<Renderer>();
        sourceTexture = (Texture2D)renderer.material.mainTexture;
        
        texWidth = sourceTexture.width;
        texHeight = sourceTexture.height;
        
        glowTexture = new Texture2D(texWidth, texHeight);
        glowTexture.filterMode = FilterMode.Point;

        Color[] transparent = new Color[texWidth * texHeight];
        for (int i = 0; i < transparent.Length; i++)
            transparent[i] = Color.clear;

        glowTexture.SetPixels(transparent);
        glowTexture.Apply();

        GameObject glowQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glowQuad.transform.SetParent(transform);
        Destroy(glowQuad.GetComponent<Collider>());
        glowQuad.transform.localRotation = Quaternion.Euler(90, 0, 0);
        glowQuad.transform.localPosition = new Vector3(0, 0.1f, 0);
        glowQuad.transform.localScale = new Vector3(10, 10, 1);

        Material glowMat = new Material(Shader.Find("Unlit/Transparent"));
        glowMat.mainTexture = glowTexture;
        glowMat.mainTexture.filterMode = FilterMode.Point;
        glowQuad.GetComponent<Renderer>().material = glowMat;
    }

    public void GlowRegion(string hexColor)
    {
        if (currentGlowCoroutine != null)
            StopCoroutine(currentGlowCoroutine);

        ClearGlow();
        currentGlowCoroutine = StartCoroutine(PulseGlow(hexColor));
    }

    public void GlowByRegionID(string regionID)
    {
        if (regionLookup == null || !regionLookup.ContainsKey(regionID)) return;
        
        List<string> hexColors = new List<string>();
        foreach (string tileID in regionLookup[regionID].tiles)
        {
            if (tileToHexLookup.ContainsKey(tileID))
                hexColors.Add(tileToHexLookup[tileID]);
        }
        StartCoroutine(GlowMultipleHexes(hexColors));
    }

    public void GlowByCountryID(string countryID)
    {
        if (countryLookup == null || !countryLookup.ContainsKey(countryID)) return;
        
        List<string> hexColors = new List<string>();
        foreach (string regionID in countryLookup[countryID].regions)
        {
            if (regionLookup.ContainsKey(regionID))
            {
                foreach (string tileID in regionLookup[regionID].tiles)
                {
                    if (tileToHexLookup.ContainsKey(tileID))
                        hexColors.Add(tileToHexLookup[tileID]);
                }
            }
        }
        StartCoroutine(GlowMultipleHexes(hexColors));
    }

    // OPTIMIZED: Pulse glow with cached dimensions
    IEnumerator PulseGlow(string hexColor)
    {
        ClearGlow();

        if (!provincePixels.ContainsKey(hexColor))
        {
            yield break;
        }

        List<Vector2Int> pixels = provincePixels[hexColor];
        float elapsed = 0f;
        
        // Pre-calculate flip once per province
        List<Vector2Int> flippedPixels = new List<Vector2Int>(pixels.Count);
        foreach (Vector2Int pos in pixels)
        {
            flippedPixels.Add(new Vector2Int(texWidth - 1 - pos.x, texHeight - 1 - pos.y));
        }
        
        while (elapsed < glowDuration)
        {
            ClearGlow();
            float pulse = glowOpacity * (0.7f + Mathf.Sin(elapsed * 10f) * 0.3f);
            
            Color glowy = glowColor;
            glowy.a = pulse;
            
            foreach (Vector2Int pos in flippedPixels)
            {
                glowTexture.SetPixel(pos.x, pos.y, glowy);
            }
            
            glowTexture.Apply();
            elapsed += Time.deltaTime;
            yield return null;
        }

        ClearGlow();
        currentGlowCoroutine = null;
    }

    // OPTIMIZED: Multiple hexes with cached flips
    IEnumerator GlowMultipleHexes(List<string> hexColors)
    {
        if (currentGlowCoroutine != null)
            StopCoroutine(currentGlowCoroutine);
        
        ClearGlow();
        
        // Pre-collect and pre-flip all pixels once
        List<Vector2Int> allFlippedPixels = new List<Vector2Int>();
        foreach (string hex in hexColors)
        {
            if (provincePixels.ContainsKey(hex))
            {
                foreach (Vector2Int pos in provincePixels[hex])
                {
                    allFlippedPixels.Add(new Vector2Int(texWidth - 1 - pos.x, texHeight - 1 - pos.y));
                }
            }
        }
        
        float elapsed = 0f;
        while (elapsed < glowDuration)
        {
            ClearGlow();
            float pulse = glowOpacity * (0.7f + Mathf.Sin(elapsed * 10f) * 0.3f);
            
            Color glowy = glowColor;
            glowy.a = pulse;
            
            foreach (Vector2Int pos in allFlippedPixels)
            {
                glowTexture.SetPixel(pos.x, pos.y, glowy);
            }
            
            glowTexture.Apply();
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        ClearGlow();
        currentGlowCoroutine = null;
    }

    public void ClearGlow()
    {
        if (glowTexture == null) return;

        Color[] clear = new Color[texWidth * texHeight];
        for (int i = 0; i < clear.Length; i++)
            clear[i] = Color.clear;

        glowTexture.SetPixels(clear);
        glowTexture.Apply();
    }
}