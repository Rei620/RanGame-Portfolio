using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 追従するキャラクター
    public Vector3 offset = new Vector3(0, 5, -10); // 高さ5、後ろ10の位置
    public float smoothTime = 0.1f; // カメラ追従の滑らかさ

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // 目標位置を計算
        Vector3 targetPosition = target.position + offset;

        // 滑らかに追従
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // キャラクターを常に注視
        transform.LookAt(target);
    }
}
