using UnityEngine;
using System.Collections.Generic; 


[System.Serializable]  
public class ProvinceData
{
    public string provinceID;
    public string hexColor;

    public string Country;

    
    public List<string> neighbors;

    public Vector3 centerPosition;  

    public ProvinceData(string name, string hex,string country , List<string> neighborList)
    {
         
        provinceID = name;
        hexColor = hex;
        Country = country;
        neighbors = neighborList;
        centerPosition = Vector3.zero; 
    

    }


}