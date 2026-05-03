using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shared prestige row: costs are in <b>prestige points</b> only (never Purr Points / <see cref="GameManager.currency"/>).
/// Purchases use <see cref="PrestigeManager.TrySpendPrestigePoints"/>.
/// </summary>
public abstract class PrestigeUpgrade : MonoBehaviour
{
    public GameManager gameManager;
    public PrestigeManager prestigeManager;
    public TMP_Text costText;

    [Header("Prestige point cost")]
    [Tooltip("First-row price in prestige points.")]
    [FormerlySerializedAs("baseCost")]
    public int basePrestigePointCost = 10;

    [Tooltip("Multiplies the prestige-point price after each purchase of this row.")]
    [FormerlySerializedAs("costMultiplier")]
    public float prestigePointCostMultiplier = 1.1f;

    [Header("Affordability visuals")]
    [Tooltip("Usually the child Image used as the pill fill behind the label.")]
    [SerializeField] Image buttonBackground;

    [Tooltip("Tint when prestige points are below this row's prestige-point cost.")]
    [SerializeField] Color unaffordableTint = new Color(0.72f, 0.76f, 0.84f, 1f);

    [Header("Click color feedback")]
    [SerializeField] float clickColorPulseDuration = 0.18f;
    [Tooltip("Successful buy: flash shifts this far from the normal fill color toward Buy Pulse Dark Color.")]
    [SerializeField] [Range(0f, 1f)] float buyDarkenBlend = 0.38f;
    [SerializeField] Color buyPulseDarkColor = new Color(0.42f, 0.46f, 0.55f, 1f);
    [SerializeField] [Range(0f, 1f)] float denyShiftTowardUnaffordableTint = 0.45f;

    [Tooltip("Global prestige-upgrade index in SaveData.prestigeUpgradeLevels.")]
    [SerializeField] int upgradeLevelIndex;

    public int UpgradeLevelIndex => upgradeLevelIndex;

    int _currentPrestigePointCost;
    Color _affordableColor;
    Button _button;
    float _colorPulseEnd;
    Color _pulsePeak;

    int _level;
    SaveManager _saveManager;

    void Awake()
    {
        CacheButtonReference();
        if (buttonBackground != null)
            _affordableColor = buttonBackground.color;
        if (_button != null && buttonBackground != null && _button.targetGraphic == buttonBackground)
            _button.transition = Selectable.Transition.None;
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
        if (prestigeManager == null)
            prestigeManager = FindObjectOfType<PrestigeManager>();

        _level = (_saveManager != null && _saveManager.CurrentSave != null)
            ? _saveManager.GetPrestigeUpgradeLevel(upgradeLevelIndex)
            : 0;
        _level = Mathf.Max(0, _level);

        _currentPrestigePointCost = ComputeCostForLevel(_level);
        UpdateCostText();
    }

    PrestigeManager ResolvePrestigeManager()
    {
        if (prestigeManager == null)
            prestigeManager = FindObjectOfType<PrestigeManager>();
        return prestigeManager;
    }

    protected SaveManager ResolveSaveManager()
    {
        if (_saveManager == null)
            _saveManager = FindObjectOfType<SaveManager>();
        return _saveManager;
    }

    /// <summary>One successful purchase (after points are spent). Level is still the old value; override typically applies one step.</summary>
    protected abstract void ApplyPurchaseEffect();

    /// <summary>Reapply the cumulative bonus for this upgrade type after a baseline reset (<paramref name="level"/> is the saved prestige level).</summary>
    protected abstract void ApplySavedBonusesForLevel(int level);

    /// <summary>
    /// Called by <see cref="SaveManager.ApplyPrestigeUpgradeBonusesFromSave"/> after run stats are reset.
    /// </summary>
    public void ApplySavedBonusesFromSave()
    {
        SaveManager save = ResolveSaveManager();
        if (gameManager == null || save == null || save.CurrentSave == null)
            return;
        int level = save.GetPrestigeUpgradeLevel(upgradeLevelIndex);
        if (level <= 0)
            return;
        ApplySavedBonusesForLevel(level);
    }

    protected int ComputeCostForLevel(int level)
    {
        int cost = basePrestigePointCost;
        for (int i = 0; i < level; i++)
        {
            int scaled = Mathf.RoundToInt(cost * prestigePointCostMultiplier);
            // At least +1 per tier so tiny multipliers / rounding can't stall the price.
            cost = Mathf.Max(scaled, cost + 1);
        }
        return Mathf.Max(0, cost);
    }

    void Update()
    {
        var pm = ResolvePrestigeManager();
        bool canAfford = pm != null && pm.PrestigePoints >= _currentPrestigePointCost;
        if (_button != null)
            _button.interactable = canAfford;
    }

    void LateUpdate()
    {
        var pm = ResolvePrestigeManager();
        if (pm == null || buttonBackground == null)
            return;
        bool canAfford = pm.PrestigePoints >= _currentPrestigePointCost;
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
        if (gameManager == null)
            return;
        var pm = ResolvePrestigeManager();
        if (pm == null)
            return;

        if (pm.TrySpendPrestigePoints(_currentPrestigePointCost))
        {
            ApplyPurchaseEffect();
            _level++;
            _currentPrestigePointCost = ComputeCostForLevel(_level);
            UpdateCostText();
            StartClickColorPulse(true);

            SaveManager save = ResolveSaveManager();
            if (save != null && save.CurrentSave != null)
            {
                save.SetPrestigeUpgradeLevel(upgradeLevelIndex, _level);
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
        if (costText != null)
            costText.text = CurrencyAmountFormatter.Format(_currentPrestigePointCost);
    }
}
