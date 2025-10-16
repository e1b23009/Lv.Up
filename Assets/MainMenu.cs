using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;   // Startボタン
    [SerializeField] private Image fadePanel;      // フェード用黒パネル

    private void Start()
    {
        // Startボタンにクリックイベントを登録
        if (startButton != null)
            startButton.onClick.AddListener(OnStartGame);

        // フェードパネルが設定されている場合
        if (fadePanel != null)
        {
            // 起動時は黒パネルを表示しておきフェードイン開始
            fadePanel.gameObject.SetActive(true);
            Color c = fadePanel.color;
            c.a = 1f;
            fadePanel.color = c;

            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// ゲーム開始時の処理（フェードアウト→シーン遷移）
    /// </summary>
    private void OnStartGame()
    {
        Debug.Log("Start button clicked!");
        if (fadePanel != null)
            StartCoroutine(FadeOutAndLoad());
        else
            SceneManager.LoadScene("GameScene"); // フェードパネル未設定でも遷移可能
    }

    /// <summary>
    /// タイトル起動時のフェードイン（黒→透明）
    /// </summary>
    private IEnumerator FadeIn()
    {
        float t = 1f;
        Color c = fadePanel.color;
        while (t > 0f)
        {
            t -= Time.deltaTime;       // フェード時間は1秒
            c.a = Mathf.Clamp01(t);
            fadePanel.color = c;
            yield return null;
        }

        fadePanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// ゲーム開始時のフェードアウト（透明→黒）→GameSceneへ
    /// </summary>
    private IEnumerator FadeOutAndLoad()
    {
        fadePanel.gameObject.SetActive(true);
        float t = 0f;
        Color c = fadePanel.color;

        while (t < 1f)
        {
            t += Time.deltaTime;       // フェード時間は1秒
            c.a = Mathf.Clamp01(t);
            fadePanel.color = c;
            yield return null;
        }

        SceneManager.LoadScene("GameScene");
    }
}