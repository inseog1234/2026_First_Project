using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("플레이어")]
    public Transform target;

    [Header("움직임 변수 세팅")]
    public float smoothSpeed = 10f;
    public Vector3 offset;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + offset;
        Vector3 smoothPos = Vector3.Lerp(
            transform.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );

        transform.position = new Vector3(
            smoothPos.x,
            smoothPos.y,
            target.position.z - 10f > -10 ? -10 : target.position.z - 10f
        );
    }
}
