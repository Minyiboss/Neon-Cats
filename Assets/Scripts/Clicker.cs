using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Main cat click: currency, hover/press visuals, and optional floating +gain text.
/// Put on the same GameObject as the Button.
/// </summary>
public class Clicker : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    public GameManager gameManager;

    [Header("Scale (optional)")]
    [Tooltip("Usually the cat art RectTransform. If empty, uses this object's RectTransform.")]
    [SerializeField] RectTransform feedbackTarget;

    [SerializeField] float hoverScale = 1.04f;
    [SerializeField] float pressScale = 0.92f;

    [Header("Tint (optional)")]
    [Tooltip("e.g. cat Image. Leave empty to skip color feedback.")]
    [SerializeField] Graphic tintTarget;

    [SerializeField] Color hoverTint = new Color(0.94f, 0.97f, 1f, 1f);
    [SerializeField] Color pressTint = new Color(0.82f, 0.86f, 0.92f, 1f);

    [Header("Floating gain text")]
    [SerializeField] bool showFloatGain = true;
    [SerializeField] float floatFontSize = 34f;
    [SerializeField] float floatRisePixels = 72f;
    [SerializeField] float floatLifetime = 0.75f;
    [SerializeField] Vector2 floatRandomOffset = new Vector2(18f, 8f);

    Vector3 _baseScale = Vector3.one;
    Color _baseColor = Color.white;
    bool _hovered;
    bool _pressed;

    Canvas _canvas;
    Vector2 _pointerDownScreen;
    //True after pointer down on this control; cleared after a click spawns or on exit/disable
    bool _usePointerSpawnForNextClick;

    void Awake()
    // Initialize the button visuals
    {
        var rt = feedbackTarget != null ? feedbackTarget : transform as RectTransform;
        if (rt != null)
        {
            feedbackTarget = rt;
            _baseScale = rt.localScale;
        }

        if (tintTarget != null)
            _baseColor = tintTarget.color;

        _canvas = GetComponentInParent<Canvas>();
    }

    // Add currency to the game manager and spawn floating gain text
    public void OnClick()
    {
        gameManager.totalManualClicks++;

        int gain = gameManager.currencyPerClick;
        gameManager.currency += gain * gameManager.incomeMultiplier;

        if (showFloatGain)
            SpawnFloatGain(gain);

        _usePointerSpawnForNextClick = false;
    }

    // Apply visuals when the pointer enters the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovered = true;
        ApplyVisuals();
    }

    // Apply visuals when the pointer exits the button
    public void OnPointerExit(PointerEventData eventData)
    {
        _hovered = false;
        _usePointerSpawnForNextClick = false;
        ApplyVisuals();
    }

    // Apply visuals when the pointer is pressed
    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        _pointerDownScreen = eventData.position;
        _usePointerSpawnForNextClick = true;
        ApplyVisuals();
    }

    // Apply visuals when the pointer is released
    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        ApplyVisuals();
    }

    // Apply visuals to the button
    void ApplyVisuals()
    {
        if (feedbackTarget != null)
        {
            float mult = _pressed ? pressScale : (_hovered ? hoverScale : 1f);
            feedbackTarget.localScale = _baseScale * mult;
        }

        if (tintTarget != null)
        {
            if (_pressed)
                tintTarget.color = pressTint;
            else if (_hovered)
                tintTarget.color = hoverTint;
            else
                tintTarget.color = _baseColor;
        }
    }

    // Reset visuals when the button is disabled
    void OnDisable()
    {
        _hovered = false;
        _pressed = false;
        _usePointerSpawnForNextClick = false;
        if (feedbackTarget != null)
            feedbackTarget.localScale = _baseScale;
        if (tintTarget != null)
            tintTarget.color = _baseColor;
    }

    /// <summary>
    /// Spawns a floating gain text object at the given position.
    /// </summary>
    void SpawnFloatGain(int gainAmount)
    {
        // get the canvas to draw on and the font for the text
        if (_canvas == null)
            return;

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;
        if (font == null)
            return;

        // get the root canvas and camera
        RectTransform root = _canvas.rootCanvas.transform as RectTransform;
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        Vector2 screenPoint;

        // if the pointer is down on this control, use where the pointer was last pressed
        if (_usePointerSpawnForNextClick)
            screenPoint = _pointerDownScreen;
        else
        {
            // otherwise, use the position of the button
            RectTransform spawnRt = feedbackTarget != null ? feedbackTarget : transform as RectTransform;
            screenPoint = RectTransformUtility.WorldToScreenPoint(cam, spawnRt.position);
        }

        // convert the screen point to a local point
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPoint, cam, out Vector2 localPoint))
            localPoint = Vector2.zero;

        // add some randomness to the position so numbers dont perfectly overlap
        localPoint += new Vector2(
            Random.Range(-floatRandomOffset.x, floatRandomOffset.x),
            Random.Range(-floatRandomOffset.y, floatRandomOffset.y));

        // create a new game object for the floating text
        var go = new GameObject("ClickGainFloat", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(root, false);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = localPoint;
        rt.sizeDelta = new Vector2(220f, 64f);

        // add text to the game object
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.fontSize = floatFontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = false;
        tmp.raycastTarget = false;
        tmp.text = "+" + CurrencyAmountFormatter.Format(gainAmount);
        tmp.color = Color.white;
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color32(30, 40, 60, 200);

        // start the animation for the floating text
        StartCoroutine(FloatAndFade(tmp, rt, floatRisePixels, floatLifetime));
    }

    static IEnumerator FloatAndFade(TextMeshProUGUI tmp, RectTransform rt, float risePixels, float lifetime)
    {
        // get the starting position and color of the text
        Vector2 start = rt.anchoredPosition;
        Color c = tmp.color;
        float elapsed = 0f;

        // animate the text
        while (elapsed < lifetime)
        {
            // update the elapsed time
            elapsed += Time.unscaledDeltaTime;
            // calculate the progress of the animation
            float u = Mathf.Clamp01(elapsed / lifetime);
            // update the position of the text
            rt.anchoredPosition = start + Vector2.up * (risePixels * Mathf.SmoothStep(0f, 1f, u));
            // update the color of the text
            float a = 1f - Mathf.SmoothStep(0f, 1f, u);
            tmp.color = new Color(c.r, c.g, c.b, c.a * a);
            // wait for the next frame
            yield return null;
        }

        // destroy the game object after animation is complete
        Object.Destroy(rt.gameObject);
    }
}
