using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;
    public Button settingsButton;
    public GameObject settingsPopup;

    void Awake()
    {
        playButton.onClick.AddListener(() => SceneLoader.Load(SceneNames.LevelSelect));
        quitButton.onClick.AddListener(Quit);
        settingsButton.onClick.AddListener(() => settingsPopup.SetActive(true));
        settingsPopup.SetActive(false);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}