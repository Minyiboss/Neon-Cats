using UnityEngine;

public class IncreaseClickPowerUpgrade : RunUpgradeBase
{
    public int clickPowerIncrease = 1;

    protected override void ApplyPurchaseEffect()
    {
        if (gameManager == null)
            return;
        gameManager.currencyPerClick += clickPowerIncrease;
    }
}
