using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CountryData
{
    public string countryID;
    public List<string> neighborCountries;  // Which countries border this one
    public List<string> regions;            // All region IDs in this country
    
    public CountryData(string id)
    {
        countryID = id;
        neighborCountries = new List<string>();
        regions = new List<string>();
    }
}