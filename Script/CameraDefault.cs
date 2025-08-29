using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // �Ǐ]����L�����N�^�[
    public Vector3 offset = new Vector3(0, 5, -10); // ����5�A���10�̈ʒu
    public float smoothTime = 0.1f; // �J�����Ǐ]�̊��炩��

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // �ڕW�ʒu���v�Z
        Vector3 targetPosition = target.position + offset;

        // ���炩�ɒǏ]
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // �L�����N�^�[����ɒ���
        transform.LookAt(target);
    }
}
