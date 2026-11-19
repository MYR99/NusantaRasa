using UnityEngine;
using UnityEngine.UI;

public class HowToPlayPanel : MonoBehaviour
{
    public Button openButton;
    public GameObject panel;
    public Button closeButton;
    public bool showOnFirstRun;  

    void Awake()
    {
        if (openButton != null)  openButton.onClick.AddListener(() => panel.SetActive(true));
        if (closeButton != null) closeButton.onClick.AddListener(() => panel.SetActive(false));
        panel.SetActive(false);
    }

    void Start()
    {
        if (showOnFirstRun && PlayerPrefs.GetInt("seenHelp", 0) == 0)
        {
            panel.SetActive(true);
            PlayerPrefs.SetInt("seenHelp", 1);
            PlayerPrefs.Save();
        }
    }

    void Update()
    {
        if (panel.activeSelf && PointerInput.PausePressed) panel.SetActive(false);
    }
}