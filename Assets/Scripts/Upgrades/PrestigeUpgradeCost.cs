using UnityEngine;

/// <summary>
/// Prestige upgrade that discounts all run (Purr Points) upgrades by applying a global cost multiplier.
/// </summary>
public class PrestigeUpgradeCost : PrestigeUpgrade
{
    [Tooltip("Multiplier applied to normal upgrade costs per level. Below 1 discounts prices.")]
    public float upgradeCostModifier = 0.9f;

    protected override void ApplyPurchaseEffect()
    {
        if (gameManager == null)
            return;
        gameManager.normalUpgradeCostMultiplier *= upgradeCostModifier;
    }

    protected override void ApplySavedBonusesForLevel(int level)
    {
        if (gameManager == null)
            return;
        gameManager.normalUpgradeCostMultiplier *= Mathf.Pow(upgradeCostModifier, level);
    }
}
