using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    const string FallbackSaveKey = "NeonPaws_Save_v1";

    public void StartNewGame()
    {
        SaveManager saveManager = FindObjectOfType<SaveManager>();
        string saveKey = saveManager != null ? saveManager.SaveKey : FallbackSaveKey;
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameScene");
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("GameScene");
    }
}
