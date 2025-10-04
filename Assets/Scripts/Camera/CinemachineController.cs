// Assets/Scripts/Camera/CinemachineCameraController.cs
using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CinemachineCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform player; // 拖 Player 到这里（或通过代码设置）

    [Header("Lookahead")]
    public float lookAheadDistance = 1.5f;    // 玩家移动时摄像机前瞻距离（X）
    public float lookSmoothTime = 0.15f;      // 平滑时间
    public float verticalOffset = 0.5f;       // 摄像机Y偏移（让角色略低一点）

    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer transposer;
    private Rigidbody2D playerRb;
    private float smoothVelX;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        transposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (player) playerRb = player.GetComponent<Rigidbody2D>();
        if (transposer == null) Debug.LogWarning("vcam 没有 FramingTransposer（2D）。请确认 cinema vcam 的 Body 使用 Framing Transposer。");
    }

    void LateUpdate()
    {
        if (player == null || transposer == null) return;

        float vx = (playerRb != null) ? playerRb.velocity.x : 0f;
        // 仅在速度足够大时才前瞻，否则回归 0
        float desiredX = Mathf.Abs(vx) > 0.1f ? Mathf.Sign(vx) * lookAheadDistance : 0f;
        float curX = transposer.m_TrackedObjectOffset.x;
        float newX = Mathf.SmoothDamp(curX, desiredX, ref smoothVelX, lookSmoothTime);

        Vector3 offset = transposer.m_TrackedObjectOffset;
        offset.x = newX;
        offset.y = verticalOffset;
        transposer.m_TrackedObjectOffset = offset;
    }
}
