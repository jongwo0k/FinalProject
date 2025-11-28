using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class StartButtonEffect : MonoBehaviour
{
    [Header("Motion")]
    public bool enableBob = true;
    public float bobAmplitude = 2f;      // 픽셀 기준, 너무 크지 않게 기본 2
    public float bobFrequency = 0.8f;    // 초당 왕복 횟수

    [Header("Alpha Pulse (keep original RGB)")]
    public bool enableAlphaPulse = true;
    public float alphaMin = 0.25f;       // 최소 알파
    public float alphaMax = 1.0f;        // 최대 알파
    public float alphaFrequency = 1.2f;  // 초당 왕복 횟수

    [Header("Optional Outline Alpha Pulse")]
    public bool pulseOutlineAlpha = true;
    public float outlineAlphaMin = 0.0f;
    public float outlineAlphaMax = 0.35f;

    RectTransform _rt;
    Vector2 _startAnchoredPos;

    Image _image;
    Color _origImageColor;

    Outline _outline;
    Color _origOutlineColor;

    bool _active = true;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _startAnchoredPos = _rt.anchoredPosition;

        _image = GetComponent<Image>();
        if (_image != null)
            _origImageColor = _image.color;

        _outline = GetComponent<Outline>();
        if (_outline != null)
            _origOutlineColor = _outline.effectColor;
    }

    void OnEnable()
    {
        _active = true;
        if (_rt != null) _startAnchoredPos = _rt.anchoredPosition;

        // 최신 원본 색 재기록(실행 중 변경 대비)
        if (_image != null) _origImageColor = _image.color;
        if (_outline != null) _origOutlineColor = _outline.effectColor;
    }

    void Update()
    {
        if (!_active) return;

        // 부드러운 시간 기준
        float tUnscaled = Time.unscaledTime;

        // 작은 위아래 보빙
        if (enableBob && _rt != null)
        {
            float bob = Mathf.Sin(tUnscaled * (Mathf.PI * 2f) * bobFrequency) * bobAmplitude;
            _rt.anchoredPosition = _startAnchoredPos + new Vector2(0f, bob);
        }

        // 알파만 맥동(기존 RGB 유지)
        float pulse01 = (Mathf.Sin(tUnscaled * (Mathf.PI * 2f) * alphaFrequency) * 0.5f) + 0.5f;

        if (enableAlphaPulse && _image != null)
        {
            float a = Mathf.Lerp(Mathf.Clamp01(alphaMin), Mathf.Clamp01(alphaMax), pulse01);
            Color c = _origImageColor;
            c.a = a;
            _image.color = c;
        }

        // 아웃라인이 있으면 알파만 펄스(선택)
        if (pulseOutlineAlpha && _outline != null)
        {
            float a = Mathf.Lerp(Mathf.Clamp01(outlineAlphaMin), Mathf.Clamp01(outlineAlphaMax), pulse01);
            Color c = _origOutlineColor;
            c.a = a;
            _outline.effectColor = c;
        }
    }

    public void StopAndReset()
    {
        _active = false;

        if (_rt != null)
            _rt.anchoredPosition = _startAnchoredPos;

        if (_image != null)
            _image.color = _origImageColor;

        if (_outline != null)
            _outline.effectColor = _origOutlineColor;
    }
}
