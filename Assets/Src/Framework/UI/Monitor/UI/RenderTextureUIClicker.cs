using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// RenderTextureUIClicker.cs 박정민
/// 클릭하면 ray를 모니터 render texture가 씌어진 cube로 쏨, cube에 맞은 좌표를 ui 상의 좌표로 변환 후
/// 그 좌표에 있는 버튼의 이벤트를 실행시킴
/// </summary>
public class RenderTextureUIClicker : MonoBehaviour
{
    [SerializeField] Camera MonitorCam;
    [SerializeField] Camera KioskCam;             // UI를 찍는 카메라
    [SerializeField] RectTransform monitorCanvasRt; // UI Canvas RectTransform
    [SerializeField] Renderer monitorScreen;        // RenderTexture 붙은 Mesh
    [SerializeField] GraphicRaycaster uiRaycaster;
    [SerializeField] EventSystem eventSystem;

    [SerializeField] float clickDelay = 0.15f;

    private float enterTime = -999f;


    void Update()
    {
        if (!Input.GetMouseButtonDown(0) || !MonitorUIModeManager.Instance.getInUIMode())
        {
            return;
        }

        // 진입 후 딜레이 이전이면 클릭 무시
        if (Time.time - enterTime < clickDelay)
        {
            Debug.Log("UI 진입 직후 클릭 무시됨");
            return;
        }

        Ray ray = MonitorCam.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 1f);

        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("No Physics Hit");
            return;
        }

        if (hit.collider.gameObject != monitorScreen.gameObject)
        {
            Debug.Log("Hit something else: " + hit.collider.name);
            return;
        }

        // 1. UV 좌표 얻기 (0~1)
        Vector2 uv = hit.textureCoord;
        Debug.Log("UV: " + uv);

        // 2. UV → Canvas 좌표로 변환
        Vector2 canvasSize = monitorCanvasRt.sizeDelta;
        Vector2 local = new Vector2(
            (uv.x * canvasSize.x) - canvasSize.x * 0.5f,
            (uv.y * canvasSize.y) - canvasSize.y * 0.5f);

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            KioskCam,
            monitorCanvasRt.TransformPoint(local));

        Debug.Log("ScreenPos: " + screenPos);

        // 3. GraphicRaycaster로 클릭 전달
        PointerEventData ped = new PointerEventData(eventSystem) { position = screenPos };
        var results = new List<RaycastResult>();
        uiRaycaster.Raycast(ped, results);
        Debug.Log($"Raycast hit count: {results.Count}");

        if (!MonitorUIModeManager.Instance.getInUIMode()) return;

        foreach (var r in results)
        {
            ExecuteEvents.Execute(r.gameObject, ped, ExecuteEvents.pointerClickHandler);
        }
    }


public void SetEnterTime()
{
    enterTime = Time.time;
}
}
