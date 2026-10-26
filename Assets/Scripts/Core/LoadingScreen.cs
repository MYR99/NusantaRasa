using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    public RectTransform progressFill;      // anak ProgressBar, anchor min (0,0) max (0,1)
    public float minimumSeconds = 1.5f;     // supaya layar tidak berkedip kalau loading terlalu cepat
    public string nextScene = SceneNames.MainMenu;

    IEnumerator Start()
    {
        AudioListener.volume = SaveSystem.Volume;

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float t = 0f;
        while (op.progress < 0.9f || t < minimumSeconds)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Min(op.progress / 0.9f, t / minimumSeconds);
            SetFill(p);
            yield return null;
        }

        SetFill(1f);
        op.allowSceneActivation = true;
    }

    void SetFill(float p)
    {
        if (progressFill != null) progressFill.anchorMax = new Vector2(Mathf.Clamp01(p), 1f);
    }
}