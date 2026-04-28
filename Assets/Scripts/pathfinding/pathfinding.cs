using System.Collections.Generic;
using UnityEngine;

public static class PathfindingManager
{
    // Reference to data (set once at start)
    private static Dictionary<string, ProvinceData> provinceLookup;      // hex → ProvinceData
    private static Dictionary<string, RegionData> regionLookup;
    private static Dictionary<string, CountryData> countryLookup;
    private static Dictionary<string, string> tileToRegionLookup;        // tileID → regionID
    private static Dictionary<string, ProvinceData> tileIDLookup;        // tileID → ProvinceData
    
    private static bool isInitialized = false;
    
    // Call this once from MapCoordinates.Start()
    public static void Initialize(
        Dictionary<string, ProvinceData> pLookup,
        Dictionary<string, RegionData> rLookup,
        Dictionary<string, CountryData> cLookup)
    {
        provinceLookup = pLookup;
        regionLookup = rLookup;
        countryLookup = cLookup;
        
        // BUILD TILE ID LOOKUP (tileID → ProvinceData)
        tileIDLookup = new Dictionary<string, ProvinceData>();
        foreach (var entry in provinceLookup)
        {
            tileIDLookup[entry.Value.provinceID] = entry.Value;
        }
        
        // Build tile → region lookup cache
        tileToRegionLookup = new Dictionary<string, string>();
        foreach (var region in regionLookup.Values)
        {
            foreach (string tileID in region.tiles)
            {
                tileToRegionLookup[tileID] = region.regionID;
            }
        }
        
        // Pre-calculate all paths
        PreCalculatePaths();
        
        isInitialized = true;
        Debug.Log($"PathfindingManager initialized: {tileIDLookup.Count} tiles, {regionLookup.Count} regions, {countryLookup.Count} countries");
    }
    
    // Main public method: Get path from any tile to any tile
    public static List<ProvinceData> GetPath(string fromTileID, string toTileID)
    {
        if (!isInitialized)
        {
            Debug.LogError("PathfindingManager not initialized!");
            return new List<ProvinceData>();
        }
        
        if (fromTileID == toTileID)
            return new List<ProvinceData>();
        
        // Check if tiles exist
        if (!tileIDLookup.ContainsKey(fromTileID) || !tileIDLookup.ContainsKey(toTileID))
        {
            Debug.LogWarning($"Tile not found: {fromTileID} or {toTileID}");
            return new List<ProvinceData>();
        }
        
        // Step 1: Neighbor?
        if (tileIDLookup[fromTileID].neighbors.Contains(toTileID))
        {
            return new List<ProvinceData> { tileIDLookup[toTileID] };
        }
        
        // Step 2: Same region?
        string fromRegion = tileToRegionLookup[fromTileID];
        string toRegion = tileToRegionLookup[toTileID];
        
        if (fromRegion == toRegion)
        {
            return FindPathInSameRegion(fromTileID, toTileID, fromRegion);
        }
        
        // Step 3: Same country?
        string fromCountry = tileIDLookup[fromTileID].Country;
        string toCountry = tileIDLookup[toTileID].Country;
        
        if (fromCountry == toCountry)
        {
            return FindPathInSameCountry(fromTileID, toTileID, fromRegion, toRegion);
        }
        
        // Step 4: Different countries
        return FindPathDifferentCountries(fromTileID, toTileID, fromCountry, toCountry, fromRegion, toRegion);
    }
    
    // ========== PRIVATE METHODS ==========
    
    private static void PreCalculatePaths()
    {
        Debug.Log("Pre-calculating region and country paths...");
        
        // Pre-calculate region-to-region paths
        foreach (var fromRegion in regionLookup.Values)
        {
            foreach (var toRegion in regionLookup.Values)
            {
                if (fromRegion.regionID == toRegion.regionID) continue;
                
                if (!fromRegion.pathsToRegions.ContainsKey(toRegion.regionID))
                {
                    fromRegion.pathsToRegions[toRegion.regionID] = FindRegionPath(fromRegion.regionID, toRegion.regionID);
                }
            }
        }
        
        // Pre-calculate country-to-country paths
        foreach (var fromCountry in countryLookup.Values)
        {
            foreach (var toCountry in countryLookup.Values)
            {
                if (fromCountry.countryID == toCountry.countryID) continue;
                
                if (!fromCountry.pathsToCountries.ContainsKey(toCountry.countryID))
                {
                    fromCountry.pathsToCountries[toCountry.countryID] = FindCountryPath(fromCountry.countryID, toCountry.countryID);
                }
            }
        }
        
        Debug.Log("Path pre-calculation complete");
    }
    
    private static List<string> FindRegionPath(string startRegionID, string targetRegionID)
    {
        Queue<string> toVisit = new Queue<string>();
        Dictionary<string, string> cameFrom = new Dictionary<string, string>();
        HashSet<string> visited = new HashSet<string>();
        
        toVisit.Enqueue(startRegionID);
        visited.Add(startRegionID);
        cameFrom[startRegionID] = null;
        
        while (toVisit.Count > 0)
        {
            string current = toVisit.Dequeue();
            if (current == targetRegionID)
                return ReconstructPath(cameFrom, current);
            
            if (!regionLookup.ContainsKey(current)) continue;
            
            foreach (string neighbor in regionLookup[current].neighborRegions)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    toVisit.Enqueue(neighbor);
                }
            }
        }
        return new List<string>();
    }
    
    private static List<string> FindCountryPath(string startCountryID, string targetCountryID)
    {
        Queue<string> toVisit = new Queue<string>();
        Dictionary<string, string> cameFrom = new Dictionary<string, string>();
        HashSet<string> visited = new HashSet<string>();
        
        toVisit.Enqueue(startCountryID);
        visited.Add(startCountryID);
        cameFrom[startCountryID] = null;
        
        while (toVisit.Count > 0)
        {
            string current = toVisit.Dequeue();
            if (current == targetCountryID)
                return ReconstructPath(cameFrom, current);
            
            if (!countryLookup.ContainsKey(current)) continue;
            
            foreach (string neighbor in countryLookup[current].neighborCountries)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                    toVisit.Enqueue(neighbor);
                }
            }
        }
        return new List<string>();
    }
    
    private static List<string> ReconstructPath(Dictionary<string, string> cameFrom, string current)
    {
        List<string> path = new List<string>();
        while (current != null)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }
    
    private static List<ProvinceData> FindPathInSameRegion(string fromTileID, string toTileID, string regionID)
    {
        HashSet<string> allowedRegions = new HashSet<string> { regionID };
        return FindTilePath(fromTileID, toTileID, allowedRegions);
    }
    
    private static List<ProvinceData> FindPathInSameCountry(string fromTileID, string toTileID, string fromRegion, string toRegion)
    {
        if (!regionLookup.ContainsKey(fromRegion) || !regionLookup[fromRegion].pathsToRegions.ContainsKey(toRegion))
            return new List<ProvinceData>();
        
        List<string> regionPath = regionLookup[fromRegion].pathsToRegions[toRegion];
        
        // Collect all regions in the path
        HashSet<string> allowedRegions = new HashSet<string>(regionPath);
        
        return FindTilePath(fromTileID, toTileID, allowedRegions);
    }
    
    private static List<ProvinceData> FindPathDifferentCountries(string fromTileID, string toTileID, string fromCountry, string toCountry, string fromRegion, string toRegion)
    {
        if (!countryLookup.ContainsKey(fromCountry) || !countryLookup[fromCountry].pathsToCountries.ContainsKey(toCountry))
            return new List<ProvinceData>();
        
        List<string> countryPath = countryLookup[fromCountry].pathsToCountries[toCountry];
        
        // Collect all regions in all countries along the path
        HashSet<string> allowedRegions = new HashSet<string>();
        foreach (string countryID in countryPath)
        {
            if (countryLookup.ContainsKey(countryID))
            {
                foreach (string regionID in countryLookup[countryID].regions)
                {
                    allowedRegions.Add(regionID);
                }
            }
        }
        
        return FindTilePath(fromTileID, toTileID, allowedRegions);
    }
    
    private static List<ProvinceData> FindTilePath(string startTileID, string targetTileID, HashSet<string> allowedRegionIDs)
    {
        Queue<string> toVisit = new Queue<string>();
        Dictionary<string, string> cameFrom = new Dictionary<string, string>();
        HashSet<string> visited = new HashSet<string>();
        
        toVisit.Enqueue(startTileID);
        visited.Add(startTileID);
        cameFrom[startTileID] = null;
        
        while (toVisit.Count > 0)
        {
            string current = toVisit.Dequeue();
            
            if (current == targetTileID)
            {
                List<ProvinceData> path = new List<ProvinceData>();
                string step = current;
                while (step != null)
                {
                    if (tileIDLookup.ContainsKey(step))
                        path.Add(tileIDLookup[step]);
                    step = cameFrom[step];
                }
                path.Reverse();
                return path;
            }
            
            if (!tileIDLookup.ContainsKey(current)) continue;
            
            foreach (string neighbor in tileIDLookup[current].neighbors)
            {
                if (visited.Contains(neighbor)) continue;
                
                if (allowedRegionIDs != null && allowedRegionIDs.Count > 0)
                {
                    if (!tileToRegionLookup.ContainsKey(neighbor)) continue;
                    string neighborRegion = tileToRegionLookup[neighbor];
                    if (!allowedRegionIDs.Contains(neighborRegion))
                        continue;
                }
                
                visited.Add(neighbor);
                cameFrom[neighbor] = current;
                toVisit.Enqueue(neighbor);
            }
        }
        
        return new List<ProvinceData>();
    }
}