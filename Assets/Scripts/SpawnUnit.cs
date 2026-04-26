using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class SpawnUnit 
{
    public Unit spawnUnit(string provinceHex, string unitName, Dictionary<string, ProvinceData> provinceLookup, GameObject unitPrefab)
    {
        if (unitPrefab == null)
        {
            //Debug.LogError("Unit prefab not assigned in the Inspector!");
            return null;
        }

        if(!provinceLookup.ContainsKey(provinceHex))
        {
            //Debug.LogError($"Province with hex {provinceHex} not found in lookup!");
            return null;
        }

        ProvinceData Data = provinceLookup[provinceHex];
        if (Data.centerPosition == Vector3.zero)
        {
           // Debug.LogError($"Province {Data.provinceID} has no center position set!");
            return null;
        }


        GameObject newUnit = GameObject.Instantiate(unitPrefab, Data.centerPosition, Quaternion.identity);
        Unit currentUnit = newUnit.GetComponent<Unit>();
        currentUnit.unitName = unitName;
        currentUnit.currentProvinceID = Data.provinceID;
        currentUnit.moveSpeed = 5f; // Example speed, can be set as needed

        //Debug.Log($"Spawned unit {unitName} in province {Data.provinceID} at position {Data.centerPosition}");

        return currentUnit;
    }

}