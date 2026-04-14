using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class ProvincePixelScanner : MonoBehaviour
{

    [Header("Settings")]
    public Texture2D provinceMap;
    public bool ScanOnStart = true;

    [Header("Output")]
    public String outputfileName = "ProvincePixelData.txt";

    // Start is called before the first frame update
    void Start()
    {
        if (ScanOnStart)
        {
            ScanAndSave();
        }
    }

    [ContextMenu("Scan and Save")]
    public void ScanAndSave()
    {
        if (provinceMap == null)
        {
            Debug.LogError("Province Map is not assigned.");
            return;
        }

        Debug.Log("Starting scan of province map of width " + provinceMap.width + " and height " + provinceMap.height);

        Dictionary<string, List<string>> pixelMap = new Dictionary<string, List<string>>();

        for (int x = 0; x < provinceMap.width; x++)
        {
            for (int y = 0; y < provinceMap.height; y++)
            {
                Color pixelColor = provinceMap.GetPixel(x, y);
                string hexColor = ColorToHex(pixelColor);


                if (hexColor == "#000000" || hexColor == "#FFFFFF")
                {
                   continue; // Skip black and white pixels
                }

                if (!pixelMap.ContainsKey(hexColor))
                {
                    pixelMap[hexColor] = new List<string>();
                }

                pixelMap[hexColor].Add($"{x},{y}");
            }
        }
        SaveToFile(pixelMap);

        Debug.Log("Scan complete. Data saved to " + outputfileName);

    }

    void SaveToFile(Dictionary<string, List<string>> pixelMap)
    {
        string filePath = Application.dataPath + "/" + outputfileName;

        List<string> lines = new List<string>();

        foreach(var entry in pixelMap)
        {
            string hexcolor = entry.Key;
            string allpixels = string.Join(";", entry.Value);
            lines.Add($"{hexcolor}:{allpixels}");
        }
        File.WriteAllLines(filePath, lines);
        Debug.Log("Data saved to " + filePath);
        Debug.Log("Total unique colors (provinces) found: " + pixelMap.Count);
    }

    string ColorToHex(Color color)
    {
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
