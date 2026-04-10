using System.Collections.Generic;
using UnityEngine;

public static class ProvinceLoader
{
    // CHANGED: Dictionary now uses string (hex) as key, not Color
    //public static void LoadProvincesFromFile(TextAsset provinceDataFile, Dictionary<string, string> targetDictionary)
    public static void LoadProvincesFromFile(TextAsset provinceDataFile, Dictionary<string, ProvinceData> targetDictionary)
    {
        if (provinceDataFile == null)
        {
            Debug.LogError("No file assigned! Drag Provinces.txt into the Inspector");
            return;
        }
        
        string[] lines = provinceDataFile.text.Split('\n');
        
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            string[] parts = line.Split('|');
            
            if (parts.Length == 4)
            {
                string provinceName = parts[0].Trim();
                string hexColor = parts[1].Trim();  // This is already hex like "#FF0000"
                string countryName = parts[2].Trim();
                string neighborsString = parts[3].Trim();



                List<string> neighbors = new List<string>();
                if (!string.IsNullOrEmpty(neighborsString))
                {
                    string[] neighborArray = neighborsString.Split(',');
                    foreach (string neighbor in neighborArray)
                    {
                        neighbors.Add(neighbor.Trim());
                    }
                }
                ProvinceData data = new ProvinceData(provinceName, hexColor,countryName, neighbors);
                // CHANGED: Store with hex as the key
                targetDictionary[hexColor] = data;
                
                Debug.Log($"Loaded: {provinceName} = {hexColor}");
            }
            else
            {
                Debug.LogWarning($"Bad line: {line} (expected 2 parts, got {parts.Length})");
            }
        }
        
        Debug.Log($"Loaded {targetDictionary.Count} provinces");
    }
    
    // NEW: Convert Color to Hex string
    public static string ColorToHex(Color color)
    {
        // Convert float (0-1) to byte (0-255)
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        
        // Format as #RRGGBB
        return $"#{r:X2}{g:X2}{b:X2}";
    }
    
    // Keep this for loading, but now it's private (only used internally)
    private static Color HexToColor(string hex)
    {
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);
        
        float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        
        return new Color(r, g, b);
    }
}