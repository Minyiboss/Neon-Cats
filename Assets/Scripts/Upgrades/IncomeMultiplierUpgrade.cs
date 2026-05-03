using UnityEngine;

public class IncomeMultiplierUpgrade : RunUpgradeBase
{
    public float moneyMultiplierIncrease = 0.1f;

    protected override void ApplyPurchaseEffect()
    {
        if (gameManager == null)
            return;
        gameManager.incomeMultiplier += moneyMultiplierIncrease;
        gameManager.NotifyIncomeMultiplierPurchased();
    }
}
