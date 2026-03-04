using UnityEngine;

public class CircularMovement : MonoBehaviour
{
    public float radius = 2f;
    public float speed = 1f;
    private float _angle;

    void Update()
    {
        _angle += speed * Time.deltaTime;

        float x = Mathf.Cos(_angle) * radius;
        float z = Mathf.Sin(_angle) * radius;

        transform.position = new Vector3(x, transform.position.y, z);
    }
}
