using TMPro;
using UnityEngine;

public class PanelController : MonoBehaviour
{
    [SerializeField] GameObject statsPanel;
    [SerializeField] GameObject achievementsPanel;

    [Header("Tab label styling (TMP only)")]
    [SerializeField] TMP_Text statsTabLabel;
    [SerializeField] TMP_Text achievementsTabLabel;
    [SerializeField] Color activeTextColor = new Color(0.1f, 0.1f, 0.2f, 1f);
    [SerializeField] Color inactiveTextColor = new Color(0.35f, 0.38f, 0.45f, 1f);

    void Start()
    {
        SetTab(statsSelected: true);
    }

    public void ShowStatsPanel()
    {
        SetTab(statsSelected: true);
    }

    public void ShowAchievementsPanel()
    {
        SetTab(statsSelected: false);
    }

    void SetTab(bool statsSelected)
    {
        if (statsPanel != null)
            statsPanel.SetActive(statsSelected);
        if (achievementsPanel != null)
            achievementsPanel.SetActive(!statsSelected);

        ApplyTabTextStyles(statsSelected);
    }

    void ApplyTabTextStyles(bool statsSelected)
    {
        if (statsTabLabel != null)
            statsTabLabel.color = statsSelected ? activeTextColor : inactiveTextColor;

        if (achievementsTabLabel != null)
            achievementsTabLabel.color = statsSelected ? inactiveTextColor : activeTextColor;
    }
}
