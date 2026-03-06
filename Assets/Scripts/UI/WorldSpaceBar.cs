using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceBar : MonoBehaviour
{
    [Header("설정")]
    public Color barColor = Color.red;
    public float barWidth = 0.8f;
    public float barHeight = 0.08f;
    public float yOffset = 1.0f;

    public float CurrentValue { get; private set; }
    public bool HasFill => _fill != null;

    private RectTransform _fill;
    private Canvas _canvas;
    private static Camera _cachedCamera;

    public void InitializeForTest()
    {
        EnsureCreated();
    }

    private void EnsureCreated()
    {
        if (_canvas != null) return;

        // Canvas
        var canvasObj = new GameObject("BarCanvas");
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0f, yOffset, 0f);

        _canvas = canvasObj.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;

        var rt = canvasObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(barWidth, barHeight);
        rt.localScale = Vector3.one;

        // Background
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        var bgRt = bgObj.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        // Fill
        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform, false);
        _fill = fillObj.AddComponent<RectTransform>();
        _fill.anchorMin = Vector2.zero;
        _fill.anchorMax = Vector2.one;
        _fill.offsetMin = Vector2.zero;
        _fill.offsetMax = Vector2.zero;
        _fill.pivot = new Vector2(0f, 0.5f);
        var fillImg = fillObj.AddComponent<Image>();
        fillImg.color = barColor;
    }

    public void SetValue(float normalized)
    {
        EnsureCreated();
        CurrentValue = Mathf.Clamp01(normalized);
        if (_fill != null)
        {
            _fill.anchorMax = new Vector2(CurrentValue, 1f);
        }
    }

    private void LateUpdate()
    {
        if (_canvas == null) return;
        if (_cachedCamera == null) _cachedCamera = Camera.main;
        if (_cachedCamera != null)
        {
            _canvas.transform.rotation = _cachedCamera.transform.rotation;
        }
    }

    private void OnEnable()
    {
        if (_canvas != null) SetValue(1f);
    }
}
