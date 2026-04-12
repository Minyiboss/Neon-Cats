using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CurrencyDisplay : MonoBehaviour
{
    public TMP_Text currencyText;
    public GameManager gameManager;

    void Awake()
    {
        currencyText = GetComponent<TMP_Text>();
    }
    void Update()
    {
        currencyText.text = "<color=#FFFFFFCC>Purr Points: </color><voffset=-3px><size=125%><b>" + CurrencyAmountFormatter.FormatFromFloat(gameManager.currency) + "</b></size></voffset>";
    }
}
