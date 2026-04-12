using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public float currency = 0f;
    public int currencyPerClick = 1;
    /// <summary>Manual clicks on the main cat button only (used for milestones).</summary>
    public int totalManualClicks;
    public int incomePerSecond = 0;
    public int autoClickerCount = 0;
    public float incomeMultiplier = 1f;

    // events to display respective stats after upgrade for that stat has been purchased
    public event Action AutoClickerPurchased;
    public event Action PassiveIncomePurchased;
    public event Action IncomeMultiplierPurchased;


    public void NotifyAutoClickerPurchased() => AutoClickerPurchased?.Invoke();

    public void NotifyPassiveIncomePurchased() => PassiveIncomePurchased?.Invoke();
    public void NotifyIncomeMultiplierPurchased() => IncomeMultiplierPurchased?.Invoke();

    public void NotifyWin(){
        SceneManager.LoadScene("WinScreen");
    }

    void Update()
    {
        currency += incomePerSecond * Time.deltaTime * incomeMultiplier;
        currency += autoClickerCount * currencyPerClick * Time.deltaTime * incomeMultiplier;
    }
}
