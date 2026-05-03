using UnityEngine;
using TMPro;

public class TabManager : MonoBehaviour
{
    [SerializeField] TMP_Text originalHeader;
    [SerializeField] GameObject prestigeHeader;
    [SerializeField] GameObject upgradesHeader;
    [SerializeField] GameObject verticalDivider;
    [SerializeField] GameObject prestigeBox;
    [SerializeField] GameObject upgradesBox;
    
    SaveManager saveManager;

    void Awake()
    {
        saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
            saveManager.EnsureLoaded();

        bool hasPrestiged = saveManager != null &&
                            saveManager.CurrentSave != null &&
                            saveManager.CurrentSave.totalRuns > 0;

        if (originalHeader != null)
            originalHeader.gameObject.SetActive(!hasPrestiged);
        if (prestigeHeader != null)
            prestigeHeader.gameObject.SetActive(hasPrestiged);
        if (upgradesHeader != null)
            upgradesHeader.gameObject.SetActive(hasPrestiged);
        if (verticalDivider != null)
            verticalDivider.gameObject.SetActive(hasPrestiged);

        if (hasPrestiged)
            showUpgradesBox();
        else
        {
            if (upgradesBox != null)
                upgradesBox.SetActive(true);
            if (prestigeBox != null)
                prestigeBox.SetActive(false);
        }
    }

    public void showPrestigeBox()
    {
        if (prestigeBox != null)
            prestigeBox.SetActive(true);
        if (upgradesBox != null)
            upgradesBox.SetActive(false);
    }

    public void showUpgradesBox()
    {
        if (prestigeBox != null)
            prestigeBox.SetActive(false);
        if (upgradesBox != null)
            upgradesBox.SetActive(true);
    }
}
