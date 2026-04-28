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
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                transform.position = targetPosition;
                isMoving = false;
                Debug.Log($"{unitName} arrived at {currentProvinceID}");
            }
        }
    }

    public void MoveToProvince(string provinceID, Vector3 worldPosition)
    {
        currentProvinceID = provinceID;
        targetPosition = worldPosition;
        isMoving = true;
    }

    public bool IsMoving()
    {
        return isMoving;
    }
}