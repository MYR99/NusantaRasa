using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    public Button[] levelButtons;   // urut: level 1 sampai 6
    public Button backButton;
    public Button settingsButton;
    public GameObject settingsPopup;

    void Awake()
    {
        backButton.onClick.AddListener(() => SceneLoader.Load(SceneNames.MainMenu));
        settingsButton.onClick.AddListener(() => settingsPopup.SetActive(true));
        settingsPopup.SetActive(false);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int level = i;

            var label = levelButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = (i + 1).ToString();

            bool unlocked = i <= SaveSystem.UnlockedLevel;
            bool available = StageCatalog.IsAvailable(i);
            levelButtons[i].interactable = unlocked && available;

            levelButtons[i].onClick.AddListener(() =>
            {
                GameSession.SelectedLevel = level;
                SceneLoader.Load(StageCatalog.SceneFor(level));
            });
        }
    }
}