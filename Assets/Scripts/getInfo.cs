using System.Collections.Generic;
using UnityEngine;


public class getInfo
{
    public static ProvinceData GetProvinceData(string id, Dictionary<string, ProvinceData> provinceLookup)
    {
        foreach (var entry in provinceLookup)
        {
            if (entry.Value.provinceID == id)
            {
                return entry.Value;
            }
        }
        return null;
    }


    public static Vector3 GetProvinceWorldPosition(string hexColor, Dictionary<string, ProvinceData> provinceLookup)
    {
        if (provinceLookup.ContainsKey(hexColor))
        {
            ProvinceData data = provinceLookup[hexColor];
            if (data.centerPosition != Vector3.zero)
            {
                return data.centerPosition;
            }
            else
            {
                Debug.LogWarning($"Province {data.provinceID} has no center position set.");
            }
       }
       return Vector3.zero;

    }
}