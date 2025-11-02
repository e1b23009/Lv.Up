using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button startButton;   // Startボタン
    [SerializeField] private Button quitButton;    // Quitボタン
    [SerializeField] private Image fadePanel;      // フェード用黒パネル
    [SerializeField] private TextMeshProUGUI bestScoreText;  // Best Score表示

    [Header("UI SE")]
    public AudioSource uiAudio;
    public AudioClip moveSound, decideSound;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource; // タイトルBGM用(Loop On, PlayOnAwake Off, 2D)
    [Range(0f, 1f)][SerializeField] private float bgmVolume = 0.5f; // 目標音量
    [SerializeField] private float bgmFadeSeconds = 1.0f; // 明転/暗転に合わせる

    private void Start()
    {
        // BestScore取得と表示
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (bestScoreText != null) bestScoreText.text = "Best Score: " + bestScore.ToString();

        // ボタン設定
        if (startButton != null) startButton.onClick.AddListener(OnStartGame);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitGame);

        // 起動時フェードインとBGMフェードイン
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            var c = fadePanel.color;
            c.a = 1f;
            fadePanel.color = c;
            StartCoroutine(FadeIn()); // 画面
        }

        // BGM開始(フェードイン)
        if (bgmSource != null)
        {
            bgmSource.volume = 0f;           // 一旦0から
            if (!bgmSource.isPlaying) bgmSource.Play();
            StartCoroutine(FadeAudio(bgmSource, 0f, bgmVolume, bgmFadeSeconds));
        }

        if (startButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
    }

    private void Update()
    {
        if (uiAudio != null)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
                if (moveSound) uiAudio.PlayOneShot(moveSound);

            if (Input.GetKeyDown(KeyCode.Return))
                if (decideSound) uiAudio.PlayOneShot(decideSound);
        }
    }

    /// <summary>Startボタン</summary>
    private void OnStartGame()
    {
        Debug.Log("Start button clicked!");
        StartCoroutine(FadeOutAndLoad());
    }

    /// <summary>Quitボタン</summary>
    private void OnQuitGame()
    {
        Debug.Log("Quit Game!");
        StartCoroutine(FadeOutAndQuit());
    }

    /// <summary>起動時のフェードイン（黒→透明）</summary>
    private IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);

        float fadeTime = 1.2f;
        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            fadePanel.color = c;
            yield return null;
        }
        c.a = 0f;
        fadePanel.color = c;
        fadePanel.gameObject.SetActive(false);
    }

    /// <summary>ゲーム開始時のフェードアウト（透明→黒）+ BGMフェードアウト</summary>
    private IEnumerator FadeOutAndLoad()
    {
        fadePanel.gameObject.SetActive(true);
        Color c = fadePanel.color;
        float t = 0f;

        // BGMを同時にフェードアウト
        if (bgmSource != null) StartCoroutine(FadeAudio(bgmSource, bgmSource.volume, 0f, 1f));

        while (t < 1f)
        {
            t += Time.deltaTime;
            c.a = t;
            fadePanel.color = c;
            yield return null;
        }

        SceneManager.LoadScene("GameScene");
    }

    /// <summary>終了時のフェードアウト処理 + BGMフェードアウト</summary>
    private IEnumerator FadeOutAndQuit()
    {
        fadePanel.gameObject.SetActive(true);
        Color c = fadePanel.color;
        float t = 0f;

        if (bgmSource != null) StartCoroutine(FadeAudio(bgmSource, bgmSource.volume, 0f, 1f));

        while (t < 1f)
        {
            t += Time.deltaTime;
            c.a = t;
            fadePanel.color = c;
            yield return null;
        }

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /// <summary>AudioSourceの音量フェード</summary>
    private IEnumerator FadeAudio(AudioSource src, float from, float to, float seconds)
    {
        if (src == null || seconds <= 0f) { if (src) src.volume = to; yield break; }

        float t = 0f;
        src.volume = from;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime; // ポーズしてもフェードする
            src.volume = Mathf.Lerp(from, to, t / seconds);
            yield return null;
        }
        src.volume = to;
        if (Mathf.Approximately(to, 0f)) src.Pause(); // 必要なら停止
    }
}
