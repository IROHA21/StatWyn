using System.ComponentModel;
using UnityEngine;


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
                

        }
    }
    }



    public void MoveToProvince(string provinceID, Vector3 worldPosition)
    {
        currentProvinceID = provinceID;
        targetPosition = worldPosition;
        isMoving = true;
        //Debug.Log($"{unitName} moving to {provinceID} at {worldPosition}");

    }

    public bool IsMoving()
    {
        return isMoving;
    }




}