// NewsStage3D_UINoSlideController.cs
using UnityEngine;

/// <summary>
/// - 오른쪽 NewsScreen에 텍스처만 표시 (애니메이션/슬라이드 없음)
/// - MaterialPropertyBlock 사용 → 에디터/런타임 모두 머티리얼 복제(누수) 없음
/// - 앵커는 씬에 직접 배치
/// </summary>
public class NewsStage3D_UINoSlideController : MonoBehaviour
{
    [Header("Right Screen")]
    public Renderer screenRenderer; // NewsScreenRoot/Screen의 Renderer
    public Texture issueTexture;    // 표시할 이미지

    MaterialPropertyBlock _mpb;
    static readonly int _MainTex = Shader.PropertyToID("_MainTex");
    static readonly int _BaseMap = Shader.PropertyToID("_BaseMap");

    void OnEnable()    => Apply();
    void Start()       => Apply();
    void OnValidate()  { if (isActiveAndEnabled) Apply(); }

    public void SetIssueTexture(Texture tex)
    {
        issueTexture = tex;
        Apply();
    }

    public void ClearIssueTexture()
    {
        issueTexture = null;
        Apply();
    }

    void Apply()
    {
        if (!screenRenderer) return;

        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        screenRenderer.GetPropertyBlock(_mpb);

        // 이전 값 싹 비우고(중요) 새로 세팅
        _mpb.Clear();

        if (issueTexture != null)
        {
            // 파이프라인/셰이더별 서로 다른 속성명을 같이 커버
            _mpb.SetTexture(_MainTex, issueTexture); // Built-in/Standard 등
            _mpb.SetTexture(_BaseMap, issueTexture); // URP Lit/Unlit 등
        }

        screenRenderer.SetPropertyBlock(_mpb);
    }
}