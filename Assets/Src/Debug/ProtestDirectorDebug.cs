// ProtestDirectorDebug.cs
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ProtestDirectorDebug : MonoBehaviour
{
    [Header("대상")]
    [SerializeField] ProtestDirector director;   // 없으면 자동 탐색
    [SerializeField] bool autoFind = true;

    [Header("핫키")]
    [SerializeField] KeyCode startInstantKey  = KeyCode.F6;
    [SerializeField] KeyCode startGradualKey  = KeyCode.F7;
    [SerializeField] KeyCode stopKey          = KeyCode.F8;

    [Header("UI 버튼")]
    [SerializeField] bool showOverlay = true;
    [SerializeField] Rect overlayRect = new Rect(10, 10, 220, 94);

    void Awake()
    {
        if (!director && autoFind)
            director = FindFirstObjectByType<ProtestDirector>();
    }

    void Update()
    {
        if (!director) return;

        if (Input.GetKeyDown(startInstantKey))  TryStartInstant();
        if (Input.GetKeyDown(startGradualKey))  TryStartGradual();
        if (Input.GetKeyDown(stopKey))          TryStop();
    }

    void OnGUI()
    {
        if (!showOverlay || !director) return;

        GUILayout.BeginArea(overlayRect, GUI.skin.box);
        GUILayout.Label("Protest Debug");
        if (GUILayout.Button("Start (Instant)"))  TryStartInstant();
        if (GUILayout.Button("Start (Gradual)"))  TryStartGradual();
        if (GUILayout.Button("Stop"))             TryStop();
        GUILayout.EndArea();
    }

    // ---- 호출 어댑터 (리플렉션으로 다양한 버전 지원) ----
    void TryStartInstant()
    {
        if (InvokeIfExists("StartProtestInstant")) return;
        if (InvokeIfExists("StartProtest", new object[]{ true })) return; // StartProtest(bool instant)
        if (InvokeIfExists("StartProtest")) { Debug.LogWarning("[ProtestDebug] instant 미지원이라 기본 순차 스폰 실행함."); }
    }

    void TryStartGradual()
    {
        if (InvokeIfExists("StartProtestGradual")) return;
        if (InvokeIfExists("StartProtest", new object[]{ false })) return; // StartProtest(bool instant)
        InvokeIfExists("StartProtest");
    }

    void TryStop()
    {
        if (!InvokeIfExists("StopProtest"))
            Debug.LogWarning("[ProtestDebug] StopProtest 못 찾음. director API 확인해줘.");
    }

    bool InvokeIfExists(string method, object[] args = null)
    {
        var t = director.GetType();
        var m = (args == null)
            ? t.GetMethod(method, System.Type.EmptyTypes)
            : t.GetMethod(method, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null,
                          Array.ConvertAll(args, a => a?.GetType() ?? typeof(object)), null);

        if (m == null)
        {
            // bool 하나 받는 StartProtest 특별 케이스(오브젝트 배열 타입 매칭 보정)
            if (method == "StartProtest" && args != null && args.Length == 1 && args[0] is bool b)
            {
                m = t.GetMethod("StartProtest", new[] { typeof(bool) });
                if (m != null) { m.Invoke(director, new object[] { b }); return true; }
            }
            return false;
        }

        m.Invoke(director, args);
        return true;
    }
}