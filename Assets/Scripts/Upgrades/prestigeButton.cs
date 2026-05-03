using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class prestigeButton : MonoBehaviour
{
    public float prestigeGain = 0f;
    public GameManager gameManager;
    public PrestigeManager prestigeManager;
    public float prestigePointsModifier = 1f;
    public TMP_Text gainText;

    [Header("Reveal upgrades")]
    [Tooltip("Starts inactive. Turns on when prestige can be earned or when current currency reaches the reveal threshold.")]
    [SerializeField] GameObject upgradesSectionStartsHidden;
    [SerializeField] float revealAtCurrentCurrency = 50000f;

    bool _upgradesSectionShown;
    Button _button;

    void Awake()
    {
        CacheButtonReference();
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

    public void activePrestige()
    {
        if (prestigeManager == null || prestigeGain <= 0f)
            return;
        prestigeManager.addPrestige(prestigeGain);
    }

    void Update()
    {
        if (gameManager == null)
            return;

        float safeRunCurrency = Mathf.Max(0f, gameManager.currentRunCurrency);
        int gain = Mathf.FloorToInt(Mathf.Sqrt(safeRunCurrency / 50000f));
        prestigeGain = gain * prestigePointsModifier;

        if (!_upgradesSectionShown && upgradesSectionStartsHidden != null &&
            (prestigeGain > 0f || gameManager.currency >= revealAtCurrentCurrency))
        {
            _upgradesSectionShown = true;
            upgradesSectionStartsHidden.SetActive(true);
            CacheButtonReference();
        }
        if (gainText != null)
            gainText.text = "+<b>" + CurrencyAmountFormatter.FormatFromFloat(prestigeGain) + "</b> Prestige Points";

        if (_button != null)
            _button.interactable = prestigeGain > 0f;
    }
}
