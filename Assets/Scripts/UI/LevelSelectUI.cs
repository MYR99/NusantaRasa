using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectUI : MonoBehaviour
{
    public Button[] levelButtons;   // urut: level 1 sampai 7
    public Button backButton;

    void Awake()
    {
        backButton.onClick.AddListener(() => SceneLoader.Load(SceneNames.MainMenu));

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int level = i;   // salin nilai i untuk lambda

            var label = levelButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = (i + 1).ToString();

            bool unlocked = i <= SaveSystem.UnlockedLevel;      // terbuka lewat progres
            bool available = StageCatalog.IsAvailable(i);       // scene-nya ada di build
            levelButtons[i].interactable = unlocked && available;

            levelButtons[i].onClick.AddListener(() =>
            {
                GameSession.SelectedLevel = level;
                SceneLoader.Load(StageCatalog.SceneFor(level));
            });
        }
    }
}