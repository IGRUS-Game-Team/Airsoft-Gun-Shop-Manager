using UnityEngine;

// Cinemachine을 쓰지 않는다면 다음 using은 지워도 됩니다.
using Cinemachine;

public class LookBinding : MonoBehaviour
{
    [Header("Optional references")]
    public Camera playerCamera;                     // 없으면 자동 탐색
    public CinemachineVirtualCamera vcam;           // 없으면 자동 탐색

    [Header("Base speeds for Cinemachine POV")]
    public float baseHorizontalSpeed = 200f;        // 프로젝트에 맞게 조정
    public float baseVerticalSpeed = 200f;

    void Awake()
    {
        if (!playerCamera) playerCamera = Camera.main ?? GetComponentInChildren<Camera>();
        if (!vcam) vcam = FindFirstObjectByType<CinemachineVirtualCamera>();
    }

    public void Apply(float sensitivity, bool invertY)
    {
        // 1) Cinemachine POV 우선
        if (vcam != null)
        {
            var pov = vcam.GetCinemachineComponent<CinemachinePOV>();
            if (pov != null)
            {
                pov.m_HorizontalAxis.m_MaxSpeed = baseHorizontalSpeed * sensitivity;
                pov.m_VerticalAxis.m_MaxSpeed   = baseVerticalSpeed   * sensitivity;
                pov.m_VerticalAxis.m_InvertInput = invertY;
                return;
            }
        }

        // 2) 리플렉션으로 자주 쓰는 필드/프로퍼티 찾기 (Starter Assets 등)
        //   - 후보: "RotationSpeed", "mouseSensitivity", "Sensitivity", "lookSpeed" ...
        var comp = GetComponentInChildren<MonoBehaviour>(); // 플레이어 컨트롤러 스크립트로 교체 가능
        if (comp != null)
        {
            TrySetFloat(comp, new[] { "RotationSpeed", "rotationSpeed", "MouseSensitivity", "mouseSensitivity", "Sensitivity", "sensitivity", "LookSpeed", "lookSpeed" }, sensitivity);
            TrySetBool (comp, new[] { "InvertY", "invertY", "YInverted", "yInverted" }, invertY);
        }
        // 3) 더 이상 할 게 없으면 스킵(메인 메뉴 등)
    }

    private static void TrySetFloat(object target, string[] names, float value)
    {
        var t = target.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(float)) { f.SetValue(target, value); return; }

            var p = t.GetProperty(n, System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            if (p != null && p.CanWrite && p.PropertyType == typeof(float)) { p.SetValue(target, value); return; }
        }
    }

    private static void TrySetBool(object target, string[] names, bool value)
    {
        var t = target.GetType();
        foreach (var n in names)
        {
            var f = t.GetField(n, System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            if (f != null && f.FieldType == typeof(bool)) { f.SetValue(target, value); return; }

            var p = t.GetProperty(n, System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            if (p != null && p.CanWrite && p.PropertyType == typeof(bool)) { p.SetValue(target, value); return; }
        }
    }
}
