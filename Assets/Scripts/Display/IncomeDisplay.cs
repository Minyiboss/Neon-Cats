using TMPro;
using UnityEngine;

public class IncomeDisplay : MonoBehaviour
{
    public GameManager gameManager;

    TMP_Text _text;
    bool _revealed;
    bool _hideWithTextEnabledOnly;

    void Awake()
    {
        // get the text component
        _text = GetComponent<TMP_Text>() ?? GetComponentInChildren<TMP_Text>(true);
        if (_text == null)
            return;
        // check if the game manager is null or the income per second is greater than 0
        if (gameManager != null && gameManager.incomePerSecond > 0)
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
        // check if the game manager is not null
        if (gameManager != null)
            gameManager.PassiveIncomePurchased += OnPassiveIncomePurchased;
        // check if the game manager is not null and the income per second is greater than 0
        if (gameManager != null && gameManager.incomePerSecond > 0)
            Reveal();
    }

    void OnDisable()
    {
        if (gameManager != null)
            gameManager.PassiveIncomePurchased -= OnPassiveIncomePurchased;
    }

    void OnPassiveIncomePurchased() => Reveal();

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
        if (_text == null || !_revealed || gameManager == null)
            return;

        _text.text =
            "Passive Income: " + CurrencyAmountFormatter.Format(gameManager.incomePerSecond) + " /s";
    }
}
