using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class borders : MonoBehaviour
{
    // ========== INSPECTOR SETTINGS ==========
    [Header("anchor settings")]
    public Color pointsColor = Color.black;      // Color of the glow (yellow, green, red, etc.)

   // public GameObject anchor = GameObject.CreatePrimitive(PrimitiveType.Sphere); // The GameObject to which this script is attached (the map)

    // ========== PRIVATE VARIABLES ==========
     
    private Dictionary<string, List<Vector2Int>> provincePixels;  // YOUR pixel data: hex → list of pixel coordinates

}
