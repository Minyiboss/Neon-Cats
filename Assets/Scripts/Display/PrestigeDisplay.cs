using TMPro;
using UnityEngine;

public class PrestigeDisplay : MonoBehaviour
{
    public PrestigeManager prestigeManager;

    TMP_Text _text;
    bool _revealed;
    bool _hideWithTextEnabledOnly;

    void Awake()
    {
        if (prestigeManager == null)
            prestigeManager = FindObjectOfType<PrestigeManager>();

        // get the text component
        _text = GetComponent<TMP_Text>() ?? GetComponentInChildren<TMP_Text>(true);
        if (_text == null)
            return;
        // check if the prestige manager is not null or the prestige points is greater than 0
        if (prestigeManager != null && prestigeManager.PrestigePoints > 0)
        {
            // set the revealed flag to true
            _revealed = true;
            return;
        }

        // check if the text game object is the same as the game object
        if (_text.gameObject == gameObject)
        {
            // set the hide with text enabled only flag to true
            _hideWithTextEnabledOnly = true;
            _text.enabled = false;
        }
        else
            // set the text game object to inactive
            _text.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (prestigeManager == null)
            prestigeManager = FindObjectOfType<PrestigeManager>();

        // check if the prestige manager is not null and the prestige points is greater than 0
        if (prestigeManager != null && prestigeManager.PrestigePoints > 0)
            Reveal();
    }

    void Reveal()
    {
        if (_text == null || _revealed)
            return;

        _revealed = true;
        if (_hideWithTextEnabledOnly)
            _text.enabled = true;
        else
            _text.gameObject.SetActive(true);
    }

    void Update()
    {
        if (prestigeManager == null)
            prestigeManager = FindObjectOfType<PrestigeManager>();

        if (_text == null || prestigeManager == null)
            return;

        if (!_revealed && prestigeManager.PrestigePoints > 0)
            Reveal();
        if (!_revealed)
            return;

        _text.text =
            "Prestige Points: " + CurrencyAmountFormatter.Format(prestigeManager.PrestigePoints);
    }
}
