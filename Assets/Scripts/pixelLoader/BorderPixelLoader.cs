using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class BorderPixelLoader
{
    public static Dictionary<string, List<Vector2Int>> provincesPixels = new Dictionary<string, List<Vector2Int>>();

    public static void LoadPixelsFromFile(TextAsset pixelFile)
    {
        Debug.Log($"LoadPixelsFromFile called with file: {pixelFile?.name ?? "NULL"}");
        
        if (pixelFile == null)
        {
            Debug.LogError("Pixel file is null.");
            return;
        }
        provincesPixels.Clear();

        string[] lines = pixelFile.text.Split('\n');
        Debug.Log($"Total lines in file: {lines.Length}");

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            string[] parts = line.Split(new char[] { '|', ':' });
            if (parts.Length == 2)
            {
                
           

            string hexColor = parts[0].Trim();
            string pixelStrings = parts[1].Trim(';');

            List<Vector2Int> pixels = new List<Vector2Int>();
            string[] pixelPairs = pixelStrings.Split(';');

            foreach (string pixelPair in pixelPairs)
                {
                    string[] coordinates = pixelPair.Split(',');
                    if (coordinates.Length == 2)
                    {
                        int x = int.Parse(coordinates[0]);
                        int y = int.Parse(coordinates[1]);
                        pixels.Add(new Vector2Int(x, y));

                    }
                }

                provincesPixels[hexColor] = pixels;
                Debug.Log($"Loaded {pixels.Count} pixels for province with color {hexColor}");

            }
         
        }
        Debug.Log($"Total provinces loaded: {provincesPixels.Count}");


    }
}