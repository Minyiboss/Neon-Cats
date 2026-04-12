using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CatFarmerUpgrade : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_Text costText;
    public int incomeIncrease = 1;

    public int baseCost = 50;
    public float costMultiplier = 1.25f;

    [Header("Affordability visuals")]
    [Tooltip("Usually the child Image used as the pill fill behind the label.")]
    [SerializeField] Image buttonBackground;

    [Tooltip("Tint when Purr Points are below the upgrade cost.")]
    [SerializeField] Color unaffordableTint = new Color(0.72f, 0.76f, 0.84f, 1f);

    [Header("Click color feedback")]
    [SerializeField] float clickColorPulseDuration = 0.18f;
    [Tooltip("Successful buy: flash shifts this far from the normal fill color toward Buy Pulse Dark Color.")]
    [SerializeField] [Range(0f, 1f)] float buyDarkenBlend = 0.38f;
    [SerializeField] Color buyPulseDarkColor = new Color(0.42f, 0.46f, 0.55f, 1f);
    [SerializeField] [Range(0f, 1f)] float denyShiftTowardUnaffordableTint = 0.45f;

    [Header("Reveal upgrades")]
    [Tooltip("Starts inactive. Turns on when Purr Points are at least Base Cost for this row, then stays on. " +
             "Leave empty if only the click-power row should reveal a section. Do not assign an object that contains this script.")]
    [SerializeField] GameObject upgradesSectionStartsHidden;

    int currentCost;
    Color _affordableColor;
    Button _button;
    bool _upgradesSectionShown;
    float _colorPulseEnd;
    Color _pulsePeak;

    void Awake()
    {
        CacheButtonReference();
        if (buttonBackground != null)
            _affordableColor = buttonBackground.color;
        if (_button != null && buttonBackground != null && _button.targetGraphic == buttonBackground)
            _button.transition = Selectable.Transition.None;
        if (upgradesSectionStartsHidden != null)
            upgradesSectionStartsHidden.SetActive(false);
    }

    void OnEnable()
    {
        CacheButtonReference();
    }

    void CacheButtonReference()
    {
        if (_button == null)
            _button = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
    }

    void Start()
    {
        currentCost = baseCost;
        UpdateCostText();
    }

    void Update()
    {
        // reveal the extra upgrade if the player can afford the base cost
        if (!_upgradesSectionShown && upgradesSectionStartsHidden != null && gameManager != null &&
            gameManager.currency >= baseCost)
        {
            _upgradesSectionShown = true;
            upgradesSectionStartsHidden.SetActive(true);
            CacheButtonReference();
        }

        bool canAfford = gameManager != null && gameManager.currency >= currentCost;
        if (_button != null)
            _button.interactable = canAfford;
    }

    void LateUpdate()
    {
        // check if the player can afford the upgrade
        if (gameManager == null || buttonBackground == null)
            return;

        bool canAfford = gameManager.currency >= currentCost;
        Color affordTint = canAfford ? _affordableColor : unaffordableTint;
        if (Time.unscaledTime < _colorPulseEnd && clickColorPulseDuration > 0f)
        {
            // calculate the progress of the pulse
            float t = 1f - Mathf.Clamp01((_colorPulseEnd - Time.unscaledTime) / clickColorPulseDuration);
            // update the color of the button background
            buttonBackground.color = Color.Lerp(_pulsePeak, affordTint, t);
        }
        else
            // update the color of the button background
            buttonBackground.color = affordTint;
    }

    public void BuyUpgrade()
    {
        if (gameManager == null)
            return;

        if (gameManager.currency >= currentCost)
        {
            gameManager.currency -= currentCost;
            gameManager.incomePerSecond += incomeIncrease;
            gameManager.NotifyPassiveIncomePurchased();
            currentCost = Mathf.RoundToInt(currentCost * costMultiplier);
            UpdateCostText();
            StartClickColorPulse(true);
        }
        else
            StartClickColorPulse(false);
    }

    // Start the click color pulse
    void StartClickColorPulse(bool purchased)
    {
        // check if the button background is null or the click color pulse duration is less than or equal to 0
        if (buttonBackground == null || clickColorPulseDuration <= 0f)
            return;
        _pulsePeak = purchased
            ? Color.Lerp(_affordableColor, buyPulseDarkColor, buyDarkenBlend)
            : Color.Lerp(_affordableColor, unaffordableTint, denyShiftTowardUnaffordableTint);
        _colorPulseEnd = Time.unscaledTime + clickColorPulseDuration;
        buttonBackground.color = _pulsePeak;
    }

    public void UpdateCostText()
    {
        costText.text = CurrencyAmountFormatter.Format(currentCost);
    }
}
