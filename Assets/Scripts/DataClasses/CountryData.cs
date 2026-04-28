using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CountryData
{
    public string countryID;
    public List<string> neighborCountries;
    public List<string> regions;
    
    // ADD THIS LINE
    public Dictionary<string, List<string>> pathsToCountries;
    
    public CountryData(string id)
    {
        countryID = id;
        neighborCountries = new List<string>();
        regions = new List<string>();
        
        // ADD THIS LINE
        pathsToCountries = new Dictionary<string, List<string>>();
    }
}