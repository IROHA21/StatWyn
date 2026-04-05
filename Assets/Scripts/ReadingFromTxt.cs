using System.Collections.Generic;
using UnityEngine;

public static class ProvinceLoader
{
    public static void LoadProvincesFromFile(TextAsset provinceDataFile, Dictionary<Color, string> targetDictionary)
    {
        // SAFETY CHECK: Did we forget to drag the file?
        if (provinceDataFile == null)
        {
            Debug.LogError("No file assigned! Drag Provinces.txt into the Inspector");
            return;
        }
        
        // STEP 1: Split file into individual lines
        string[] lines = provinceDataFile.text.Split('\n');
        
        // STEP 2: Loop through each line
        foreach (string line in lines)
        {
            // Skip empty lines
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            // STEP 3: Split line by pipe character
            string[] parts = line.Split('|');
            
            // STEP 4: Check we have exactly 2 parts
            if (parts.Length == 2)
            {
                string provinceName = parts[0].Trim();
                string hexColor = parts[1].Trim();
                Color color = HexToColor(hexColor);
                targetDictionary[color] = provinceName;
                Debug.Log($"Loaded: {provinceName} = {hexColor}");
            }
            else
            {
                Debug.LogWarning($"Bad line: {line} (expected 2 parts, got {parts.Length})");
            }
        }
        
        Debug.Log($"Loaded {targetDictionary.Count} provinces");
    }
    
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