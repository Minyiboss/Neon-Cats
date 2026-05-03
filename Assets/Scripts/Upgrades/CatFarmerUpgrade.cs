using UnityEngine;

public class CatFarmerUpgrade : RunUpgradeBase
{
    public int incomeIncrease = 1;

    protected override void ApplyPurchaseEffect()
    {
        if (gameManager == null)
            return;
        gameManager.incomePerSecond += incomeIncrease;
        gameManager.NotifyPassiveIncomePurchased();
    }
}
