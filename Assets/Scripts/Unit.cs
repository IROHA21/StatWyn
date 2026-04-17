using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Unit class represents a movable unit on the map that can travel between neighboring provinces.
/// The unit moves from its current position to a target province using linear interpolation.
/// Movement is restricted to neighboring provinces only, as defined in the province data.
/// 
/// Key features:
/// - Smooth movement animation using Vector3.MoveTowards
/// - Province tracking to maintain current location
/// - Movement speed configuration
/// - Event logging for movement actions
/// 
/// Usage:
/// 1. Assign a unit name in the inspector or at runtime
/// 2. Call MoveToProvince() with a valid neighboring province ID and world position
/// 3. The unit will automatically move and update its current province
/// 
/// Note: Movement validation (checking if target is a neighbor) should be handled by the calling class.
/// </summary>
public class Unit : MonoBehaviour
{
    [Header("Unit information")]
    public string unitName;
    public string currentProvinceID;

    [Header("movement settings")]
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;



    void Update()
    {
        if (isMoving)
        {

            // MOVE towards the target position
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            //check if we reached the target
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition; // snap to exact position
                isMoving = false;
                Debug.Log($"{unitName} reached {currentProvinceID}");
            }

        }
    }



    /// <summary>
    /// Initiates movement to a target province.
    /// </summary>
    /// <param name="provinceID">The ID of the target province</param>
    /// <param name="worldPosition">The world coordinates of the target province center</param>
    public void MoveToProvince(string provinceID, Vector3 worldPosition)
    {
        currentProvinceID = provinceID;
        targetPosition = worldPosition;
        isMoving = true;
        Debug.Log($"{unitName} moving to {provinceID} at {worldPosition}");

    }

    /// <summary>
    /// Checks if the unit is currently moving.
    /// </summary>
    /// <returns>True if the unit is in motion, false otherwise</returns>
    public bool IsMoving()
    {
        return isMoving;
    }




}