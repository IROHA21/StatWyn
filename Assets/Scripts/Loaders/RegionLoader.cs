using System.Collections.Generic;
using UnityEngine;

public static class RegionLoader
{
    public static void LoadRegionsFromFile(TextAsset regionGroupsFile, Dictionary<string, RegionData> targetDictionary)
    {
        if (regionGroupsFile == null)
        {
            Debug.LogError("No region groups file assigned!");
            return;
        }
        
        string[] lines = regionGroupsFile.text.Split('\n');
        
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            string[] parts = line.Split('|');
            
            if (parts.Length == 4)
            {
                string regionID = parts[0].Trim();
                string regionName = parts[1].Trim();
                string countryID = parts[2].Trim();
                string tileIDs = parts[3].Trim();
                
                RegionData region = new RegionData(regionID, countryID, regionName);
                
                string[] tiles = tileIDs.Split(',');
                foreach (string tile in tiles)
                {
                    region.tiles.Add(tile.Trim());
                }
                
                targetDictionary[regionID] = region;
            }
            else
            {
                Debug.LogWarning($"Bad line in RegionGroups: {line}");
            }
        }
        
        Debug.Log($"Loaded {targetDictionary.Count} regions");
    }
}