using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

public class IncreaseClickPowerUpgrade : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_Text costText;
    public int clickPowerIncrease = 1;

    public int baseCost = 10;
    public float costMultiplier = 1.1f;

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
    [Tooltip("Starts inactive. Turns on when Purr Points are at least Base Cost (first upgrade price), then stays on. " +
             "Assign a sibling/parent chunk of UI—do not assign an object that contains this script or Update will stop.")]
    [FormerlySerializedAs("upgradesHiddenUntilFirstAffordable")]
    [SerializeField] GameObject upgradesSectionStartsHidden;

    int currentCost;
    Color _affordableColor;
    Button _button;
    bool _upgradesSectionShown;
    float _colorPulseEnd;
    Color _pulsePeak;

    // Initialize the button visuals
    void Awake()
    {
        CacheButtonReference();
        if (buttonBackground != null)
            _affordableColor = buttonBackground.color;
        // ColorTint on the same Image overwrites scripted colors each frame.
        if (_button != null && buttonBackground != null && _button.targetGraphic == buttonBackground)
            _button.transition = Selectable.Transition.None;
        if (upgradesSectionStartsHidden != null)
            upgradesSectionStartsHidden.SetActive(false);
    }

    void OnEnable()
    {
        // Child Button may live under a section that starts inactive; resolve again when hierarchy wakes up.
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

    // Update the button visuals
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

    // Update the button visuals
    void LateUpdate()
    {
        if (gameManager == null || buttonBackground == null)
            return;
        // check if the player can afford the upgrade
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

    // Buy the upgrade
    public void BuyUpgrade()
    {
        if (gameManager == null)
            return;

        if (gameManager.currency >= currentCost)
        {
            gameManager.currency -= currentCost;
            gameManager.currencyPerClick += clickPowerIncrease;
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

    // Update the cost text
    public void UpdateCostText()
    {
        costText.text = CurrencyAmountFormatter.Format(currentCost);
    }
}
