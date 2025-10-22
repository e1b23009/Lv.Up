using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;   // Startボタン
    [SerializeField] private Button quitButton;    // Quitボタン
    [SerializeField] private Image fadePanel;      // フェード用黒パネル

    private void Start()
    {
        // Startボタン設定
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        // Quitボタン設定
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitGame);

        // 起動時フェードイン
        if (fadePanel != null)
        {
            fadePanel.gameObject.SetActive(true);
            Color c = fadePanel.color;
            c.a = 1f;
            fadePanel.color = c;
            StartCoroutine(FadeIn());
        }

        if (startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
    }

    // ボタン移動音を鳴らす
    public AudioSource uiAudio;
    public AudioClip moveSound, decideSound;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            uiAudio.PlayOneShot(moveSound);

        if (Input.GetKeyDown(KeyCode.Return))
            uiAudio.PlayOneShot(decideSound);
    }

    /// <summary>
    /// Startボタンを押したとき
    /// </summary>
    private void OnStartGame()
    {
        Debug.Log("Start button clicked!");
        StartCoroutine(FadeOutAndLoad());
    }

    /// <summary>
    /// Quitボタンを押したとき
    /// </summary>
    private void OnQuitGame()
    {
        Debug.Log("Quit Game!");
        StartCoroutine(FadeOutAndQuit());
    }

    /// <summary>
    /// 起動時のフェードイン（黒→透明）
    /// </summary>
    private IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);

        float fadeTime = 1.2f; // 明転にかける時間（秒）
        float elapsed = 0f;
        Color c = fadePanel.color;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / fadeTime); // αを1→0へ
            fadePanel.color = c;
            yield return null;
        }

        // 最後に確実に透明にして消す
        c.a = 0f;
        fadePanel.color = c;
        fadePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// ゲーム開始時のフェードアウト（透明→黒）
    /// </summary>
    private IEnumerator FadeOutAndLoad()
    {
        fadePanel.gameObject.SetActive(true);
        Color c = fadePanel.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            c.a = t;
            fadePanel.color = c;
            yield return null;
        }
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// 終了時のフェードアウト処理
    /// </summary>
    private IEnumerator FadeOutAndQuit()
    {
        fadePanel.gameObject.SetActive(true);
        Color c = fadePanel.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            c.a = t;
            fadePanel.color = c;
            yield return null;
        }

        // アプリ終了
        Application.Quit();

#if UNITY_EDITOR
        // Unityエディタ上では再生停止
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}