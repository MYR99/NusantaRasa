using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;

    void Awake()
    {
        AudioListener.volume = SaveSystem.Volume;
        playButton.onClick.AddListener(() => SceneLoader.Load(SceneNames.LevelSelect));
        quitButton.onClick.AddListener(Quit);
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