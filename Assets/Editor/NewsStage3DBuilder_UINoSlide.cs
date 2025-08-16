// Assets/Editor/NewsStage3DBuilder_UINoSlide.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class NewsStage3DBuilder_UINoSlide
{
    [MenuItem("GameObject/GNN/Create News Stage (3D + UI, No Slide)", priority = 0)]
    public static void CreateStage()
    {
        // Root
        var root = new GameObject("GNN_NewsStage_UI_NoSlide");
        Undo.RegisterCreatedObjectUndo(root, "Create GNN News Stage (UI No Slide)");

        // ===== 3D SETUP =====
        var stage = new GameObject("Stage");
        stage.transform.SetParent(root.transform, false);

        // Wall (Quad)
        var wall = GameObject.CreatePrimitive(PrimitiveType.Quad);
        wall.name = "Wall";
        wall.transform.SetParent(stage.transform, false);
        wall.transform.position = new Vector3(0f, 1.6f, 3.5f);
        wall.transform.localScale = new Vector3(7.0f, 3.8f, 1f);
        wall.transform.rotation = Quaternion.Euler(0, 180, 0);
        SafeColor(wall, new Color(0.92f, 0.87f, 0.90f)); // 연보라-핑크

        // Desk (Cube)
        var desk = GameObject.CreatePrimitive(PrimitiveType.Cube);
        desk.name = "Desk";
        desk.transform.SetParent(stage.transform, false);
        desk.transform.position = new Vector3(0f, 0.35f, 1.6f);
        desk.transform.localScale = new Vector3(4.2f, 0.7f, 0.65f);
        SafeColor(desk, new Color(0.86f, 0.80f, 0.92f));

        // Right News Screen (Frame + Screen)
        var screenRoot = new GameObject("NewsScreenRoot");
        screenRoot.transform.SetParent(stage.transform, false);
        screenRoot.transform.position = new Vector3(1.9f, 1.7f, 2.9f);

        // Bezel (frame)
        var bezel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bezel.name = "Bezel";
        bezel.transform.SetParent(screenRoot.transform, false);
        bezel.transform.localScale = new Vector3(1.95f, 1.35f, 0.06f);
        SafeColor(bezel, new Color(1f, 1f, 1f, 1f));

        // Screen (plain unlit texture, no animation/effects)
        var screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "Screen";
        screen.transform.SetParent(screenRoot.transform, false);
        screen.transform.localPosition = new Vector3(0, 0, 0.04f);
        screen.transform.localScale = new Vector3(1.75f, 1.05f, 1f);
        screen.transform.rotation = Quaternion.Euler(0, 180, 0);
        SafeUnlitTexture(screen);

        // ===== Camera & Lights =====
        Camera cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cam = camGO.GetComponent<Camera>();
            cam.tag = "MainCamera";
        }
        cam.transform.position = new Vector3(0f, 1.6f, -6.0f);
        cam.transform.rotation = Quaternion.LookRotation(new Vector3(0f, 1.5f, 2f) - cam.transform.position);
        cam.fieldOfView = 35f;

        // Basic lights (only if none exist)
        if (Object.FindFirstObjectByType<Light>() == null)
        {
            var key = new GameObject("KeyLight", typeof(Light));
            key.transform.SetParent(stage.transform, false);
            key.transform.position = new Vector3(-2.2f, 3.0f, -1.5f);
            key.transform.rotation = Quaternion.Euler(35, 25, 0);
            var keyL = key.GetComponent<Light>();
            keyL.type = LightType.Directional;
            keyL.intensity = 1.2f;

            var fill = new GameObject("FillLight", typeof(Light));
            fill.transform.SetParent(stage.transform, false);
            fill.transform.position = new Vector3(2.3f, 2.5f, -1.8f);
            fill.transform.rotation = Quaternion.Euler(40, -20, 0);
            var fillL = fill.GetComponent<Light>();
            fillL.type = LightType.Directional;
            fillL.intensity = 0.6f;
        }

        // ===== UI OVERLAY (KEEP) =====
        var canvasGO = new GameObject("UI_Overlay", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(root.transform, false);
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Logo
        var logo = new GameObject("Logo", typeof(TextMeshProUGUI));
        logo.transform.SetParent(canvasGO.transform, false);
        var logoRT = logo.GetComponent<RectTransform>();
        logoRT.anchorMin = new Vector2(0.05f, 0.78f);
        logoRT.anchorMax = new Vector2(0.35f, 0.93f);
        logoRT.offsetMin = logoRT.offsetMax = Vector2.zero;
        var logoTMP = logo.GetComponent<TextMeshProUGUI>();
        logoTMP.text = "GNN";
        logoTMP.fontSize = 200;
        logoTMP.alignment = TextAlignmentOptions.Left;
        logoTMP.fontStyle = FontStyles.Bold;
        logoTMP.color = new Color(0.95f, 0.5f, 0.7f);

        // Tagline
        var tag = new GameObject("Tagline", typeof(TextMeshProUGUI));
        tag.transform.SetParent(canvasGO.transform, false);
        var tagRT = tag.GetComponent<RectTransform>();
        tagRT.anchorMin = new Vector2(0.052f, 0.72f);
        tagRT.anchorMax = new Vector2(0.40f, 0.78f);
        tagRT.offsetMin = tagRT.offsetMax = Vector2.zero;
        var tagTMP = tag.GetComponent<TextMeshProUGUI>();
        tagTMP.text = "Global News for Our City";
        tagTMP.fontSize = 40;
        tagTMP.alignment = TextAlignmentOptions.Left;
        tagTMP.color = new Color(0.4f, 0.2f, 0.4f, 0.95f);

        // Scanline overlay (UI 효과는 유지, 뉴스 화면 자체엔 영향 없음)
        var scan = new GameObject("Scanlines", typeof(RawImage));
        scan.transform.SetParent(canvasGO.transform, false);
        var scanRT = scan.GetComponent<RectTransform>();
        scanRT.anchorMin = Vector2.zero; scanRT.anchorMax = Vector2.one;
        scanRT.offsetMin = scanRT.offsetMax = Vector2.zero;
        var scanRaw = scan.GetComponent<RawImage>();
        scanRaw.texture = GenerateScanlineTexture(1920, 1080, 2, 0.08f);
        scanRaw.color = new Color(1,1,1,0.25f);
        scanRaw.raycastTarget = false;

        // ===== Controller (No Slide, Anchor manual) =====
        var ctrl = root.AddComponent<NewsStage3D_UINoSlideController>();
        ctrl.screenRenderer = screen.GetComponent<Renderer>();

        Selection.activeObject = root;
        EditorGUIUtility.PingObject(root);
    }

    // ---------- Helpers (파이프라인 안전) ----------
    static void SafeColor(GameObject go, Color c)
    {
        var r = go.GetComponent<Renderer>();
        if (!r) return;

        Material m;
        if (r.sharedMaterial != null)
            m = new Material(r.sharedMaterial); // 복제
        else
        {
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("HDRP/Lit");
            if (s == null) s = Shader.Find("Unlit/Color");
            m = new Material(s);
        }

        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        else if (m.HasProperty("_Color")) m.SetColor("_Color", c);

        r.sharedMaterial = m;
    }

    static void SafeUnlitTexture(GameObject go)
    {
        var r = go.GetComponent<Renderer>();
        if (!r) return;
        Shader s = Shader.Find("Unlit/Texture");
        if (s == null) s = Shader.Find("Universal Render Pipeline/Unlit");
        if (s == null) s = Shader.Find("Sprites/Default");
        var m = new Material(s);
        r.sharedMaterial = m;
    }

    static Texture2D GenerateScanlineTexture(int w, int h, int everyPx, float darken)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        var cols = new Color[w*h];
        for (int y=0; y<h; y++)
        {
            bool line = (y % everyPx) == 0;
            float m = line ? (1f - darken) : 1f;
            for (int x=0; x<w; x++) cols[y*w+x] = new Color(m,m,m,1f);
        }
        tex.SetPixels(cols); tex.Apply();
        return tex;
    }
}
#endif