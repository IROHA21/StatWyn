using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RegionData
{
    public string regionID;

    public string regionName; // Optional: A human-readable name for the region
    public string countryID;
    public List<string> neighborRegions;  // Which regions border this one
    public List<string> tiles;   
              // All tile IDs in this region
    
    
    public RegionData(string id, string country, string name)
    {
        regionID = id;
        countryID = country;
        neighborRegions = new List<string>();
        tiles = new List<string>();
        regionName = string.Empty;
         regionName = name;
    }
}