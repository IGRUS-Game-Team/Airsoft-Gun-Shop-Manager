using UnityEngine;
using Cinemachine;

public class LookBinding : MonoBehaviour
{
    [Header("Optional references")]
    public Camera playerCamera;                     // 없으면 자동 탐색
    public CinemachineVirtualCamera vcam;           // 없으면 자동 탐색

    [Header("Base speeds for Cinemachine POV")]
    public float baseHorizontalSpeed = 100f;
    public float baseVerticalSpeed = 100f;

    void Awake()
    {
        if (!playerCamera) playerCamera = Camera.main ?? GetComponentInChildren<Camera>();
        if (!vcam) vcam = FindFirstObjectByType<CinemachineVirtualCamera>();
    }

    public void Apply(float sensitivity, bool invertY)
    {
        // vcam이 아직 없으면 다시 탐색 (씬 로드 타이밍 이슈 방지)
        if (!vcam) vcam = FindFirstObjectByType<CinemachineVirtualCamera>();

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
        var comp = GetComponentInChildren<MonoBehaviour>();
        if (comp != null)
        {
            TrySetFloat(comp, new[] { "RotationSpeed", "rotationSpeed", "MouseSensitivity", "mouseSensitivity", "Sensitivity", "sensitivity", "LookSpeed", "lookSpeed" }, sensitivity);
            TrySetBool (comp, new[] { "InvertY", "invertY", "YInverted", "yInverted" }, invertY);
        }
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
