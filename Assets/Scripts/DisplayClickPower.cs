using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayClickPower : MonoBehaviour
{
    public GameManager gameManager;
    void Update()
    {
        GetComponent<TMPro.TMP_Text>().text = "Click Power: " + CurrencyAmountFormatter.Format(gameManager.currencyPerClick);
    }
}
