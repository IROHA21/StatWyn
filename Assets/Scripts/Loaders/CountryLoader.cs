using System.Collections.Generic;
using UnityEngine;

public static class CountryLoader
{
    public static void LoadCountriesFromFile(TextAsset countryGroupsFile, Dictionary<string, CountryData> targetDictionary)
    {
        if (countryGroupsFile == null)
        {
            Debug.LogError("No country groups file assigned!");
            return;
        }
        
        string[] lines = countryGroupsFile.text.Split('\n');
        
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;
            
            string[] parts = line.Split('|');
            
            if (parts.Length == 2)
            {
                string countryID = parts[0].Trim();
                string regionIDs = parts[1].Trim();
                
                CountryData country = new CountryData(countryID);
                
                string[] regions = regionIDs.Split(',');
                foreach (string region in regions)
                {
                    country.regions.Add(region.Trim());
                }
                
                targetDictionary[countryID] = country;
            }
            else
            {
                Debug.LogWarning($"Bad line in CountryGroups: {line}");
            }
        }
        
       // Debug.Log($"Loaded {targetDictionary.Count} countries");
    }
}