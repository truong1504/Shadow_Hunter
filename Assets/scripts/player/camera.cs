using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 5f;

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    void LateUpdate()
    {
        if (target == null) return;

        float x = Mathf.Clamp(target.position.x, minX, maxX);
        float y = Mathf.Clamp(target.position.y, minY, maxY);

        Vector3 targetPos = new Vector3(x, y, transform.position.z);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            smoothSpeed * Time.deltaTime
        );
    }
}