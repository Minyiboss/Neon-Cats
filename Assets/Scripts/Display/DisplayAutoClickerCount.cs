using TMPro;
using UnityEngine;

public class DisplayAutoClickerCount : MonoBehaviour
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

        // check if the game manager is not null and the auto clicker count is greater than 0
        if (gameManager != null && gameManager.autoClickerCount > 0)
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
            // set the text component to disabled
            _text.enabled = false;
        }
        else
            _text.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (gameManager != null)
            gameManager.AutoClickerPurchased += OnAutoClickerPurchased;

        if (gameManager != null && gameManager.autoClickerCount > 0)
            Reveal();
    }

    void OnDisable()
    {
        if (gameManager != null)
            gameManager.AutoClickerPurchased -= OnAutoClickerPurchased;
    }

    void OnAutoClickerPurchased() => Reveal();

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

        _text.text = "AutoClicker Count: " + CurrencyAmountFormatter.Format(gameManager.autoClickerCount);
    }
}
