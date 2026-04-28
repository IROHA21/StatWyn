using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RegionData
{
    public string regionID;
    public string regionName;
    public string countryID;
    public List<string> neighborRegions;
    public List<string> tiles;
    
    // ADD THIS LINE
    public Dictionary<string, List<string>> pathsToRegions;
    
    public RegionData(string id, string country, string name)
    {
        regionID = id;
        regionName = name;
        countryID = country;
        neighborRegions = new List<string>();
        tiles = new List<string>();
        
        // ADD THIS LINE
        pathsToRegions = new Dictionary<string, List<string>>();
    }
}