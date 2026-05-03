using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shared Purr-Points upgrade row: cost curve, save index, reveal section, button afford UI.
/// Subclasses implement <see cref="ApplyPurchaseEffect"/> for the stat change after currency is deducted.
/// </summary>
public abstract class RunUpgradeBase : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_Text costText;

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

    [Tooltip("Global upgrade index in SaveData.upgradeLevels.")]
    [SerializeField] int upgradeLevelIndex;

    public int UpgradeLevelIndex => upgradeLevelIndex;

    int currentCost;
    Color _affordableColor;
    Button _button;
    bool _upgradesSectionShown;
    float _colorPulseEnd;
    Color _pulsePeak;
    int _lastDisplayedCost = -1;

    int _level;
    SaveManager _saveManager;

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
        _saveManager = FindObjectOfType<SaveManager>();
        if (_saveManager != null)
            _saveManager.EnsureLoaded();

        _level = (_saveManager != null && _saveManager.CurrentSave != null)
            ? _saveManager.GetUpgradeLevel(upgradeLevelIndex)
            : 0;
        _level = Mathf.Max(0, _level);

        currentCost = ComputeCostForLevel(_level);
        UpdateCostText();

        _upgradesSectionShown = _level > 0;
        if (_upgradesSectionShown && upgradesSectionStartsHidden != null)
        {
            upgradesSectionStartsHidden.SetActive(true);
            CacheButtonReference();
        }
    }

    protected GameManager ResolveGameManager()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();
        return gameManager;
    }

    protected SaveManager ResolveSaveManager()
    {
        if (_saveManager == null)
            _saveManager = FindObjectOfType<SaveManager>();
        return _saveManager;
    }

    /// <summary>Called after Purr Points are deducted for one purchase; apply the upgrade’s effect here.</summary>
    protected abstract void ApplyPurchaseEffect();

    protected int ComputeCostForLevel(int level)
    {
        int cost = baseCost;
        for (int i = 0; i < level; i++)
            cost = Mathf.RoundToInt(cost * costMultiplier);
        return Mathf.Max(0, cost);
    }

    int GetDiscountedCost(int baseUpgradeCost)
    {
        var gm = ResolveGameManager();
        float costMultiplierFromBonuses = 1f;
        if (gm != null)
            costMultiplierFromBonuses = Mathf.Max(0.01f, gm.normalUpgradeCostMultiplier);

        int discountedCost = Mathf.RoundToInt(baseUpgradeCost * costMultiplierFromBonuses);
        if (baseUpgradeCost > 0)
            discountedCost = Mathf.Max(1, discountedCost);
        return Mathf.Max(0, discountedCost);
    }

    void Update()
    {
        var gm = ResolveGameManager();
        if (!_upgradesSectionShown && upgradesSectionStartsHidden != null && gm != null &&
            (_level > 0 || gm.currency >= baseCost))
        {
            _upgradesSectionShown = true;
            upgradesSectionStartsHidden.SetActive(true);
            CacheButtonReference();
        }

        int currentDiscountedCost = GetDiscountedCost(currentCost);
        if (currentDiscountedCost != _lastDisplayedCost)
            UpdateCostText();
        bool canAfford = gm != null && gm.currency >= currentDiscountedCost;
        if (_button != null)
            _button.interactable = canAfford;
    }

    void LateUpdate()
    {
        var gm = ResolveGameManager();
        if (gm == null || buttonBackground == null)
            return;
        int currentDiscountedCost = GetDiscountedCost(currentCost);
        bool canAfford = gm.currency >= currentDiscountedCost;
        Color affordTint = canAfford ? _affordableColor : unaffordableTint;
        if (Time.unscaledTime < _colorPulseEnd && clickColorPulseDuration > 0f)
        {
            float t = 1f - Mathf.Clamp01((_colorPulseEnd - Time.unscaledTime) / clickColorPulseDuration);
            buttonBackground.color = Color.Lerp(_pulsePeak, affordTint, t);
        }
        else
            buttonBackground.color = affordTint;
    }

    public void BuyUpgrade()
    {
        var gm = ResolveGameManager();
        if (gm == null)
            return;

        int currentDiscountedCost = GetDiscountedCost(currentCost);
        if (gm.currency >= currentDiscountedCost)
        {
            gm.currency -= currentDiscountedCost;
            ApplyPurchaseEffect();
            _level++;
            currentCost = ComputeCostForLevel(_level);
            UpdateCostText();
            StartClickColorPulse(true);

            if (!_upgradesSectionShown)
            {
                _upgradesSectionShown = true;
                if (upgradesSectionStartsHidden != null)
                    upgradesSectionStartsHidden.SetActive(true);
                CacheButtonReference();
            }

            SaveManager save = ResolveSaveManager();
            if (save != null && save.CurrentSave != null)
            {
                save.SetUpgradeLevel(upgradeLevelIndex, _level);
                save.SaveNow();
            }
        }
        else
            StartClickColorPulse(false);
    }

    void StartClickColorPulse(bool purchased)
    {
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
        int displayedCost = GetDiscountedCost(currentCost);
        _lastDisplayedCost = displayedCost;
        if (costText != null)
            costText.text = CurrencyAmountFormatter.Format(displayedCost);
    }
}
