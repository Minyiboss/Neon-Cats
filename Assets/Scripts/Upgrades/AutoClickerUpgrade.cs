using UnityEngine;

public class AutoClickerUpgrade : RunUpgradeBase
{
    public int autoClickerCountIncrease = 1;

    protected override void ApplyPurchaseEffect()
    {
        if (gameManager == null)
            return;
        gameManager.autoClickerCount += autoClickerCountIncrease;
        gameManager.NotifyAutoClickerPurchased();
    }
}
