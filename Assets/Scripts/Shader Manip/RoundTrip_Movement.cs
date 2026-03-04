using UnityEngine;

public class SimpleBackAndForthMovement : MonoBehaviour
{
    public enum MovementAxis
    {
        X,
        Y,
        Z
    }

    [Header("Axe de déplacement")]
    [SerializeField] private MovementAxis movementAxis = MovementAxis.X;

    [Header("Paramètres de déplacement")]
    [SerializeField] private float movementSpeed = 1.0f;
    [SerializeField] private float timeForOneWay = 2.0f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float timer = 0.0f;
    private bool isMovingForward = true;

    private void Start()
    {
        startPosition = transform.position;

        Vector3 direction = GetDirectionFromAxis();
        targetPosition = startPosition + direction * movementSpeed * timeForOneWay;
    }

    private void Update()
    {
        if (timeForOneWay <= 0.0f)
        {
            return;
        }

        timer += Time.deltaTime / timeForOneWay;

        if (isMovingForward)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timer);
        }
        else
        {
            transform.position = Vector3.Lerp(targetPosition, startPosition, timer);
        }

        if (timer >= 1.0f)
        {
            timer = 0.0f;
            isMovingForward = !isMovingForward;
        }
    }

    private Vector3 GetDirectionFromAxis()
    {
        if (movementAxis == MovementAxis.X)
        {
            return Vector3.right;
        }

        if (movementAxis == MovementAxis.Y)
        {
            return Vector3.up;
        }

        return Vector3.forward;
    }
}
