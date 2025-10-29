using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject gameClearUI;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button titleButton;

    [Header("Timer")]
    [SerializeField] private float startTime = 60f;
    public float StartTime => startTime; // Player側から取得できるように

    [Header("Color")]
    [SerializeField] private Color normalColor = Color.white;     // 通常色
    [SerializeField] private Color selectedColor = Color.yellow;  // 選択中の色

    private bool isGameOver = false;
    private bool isGameClear = false;
    private int currentSelected = 0; // 0:リスタート, 1:タイトル
    private int bestScore = 0; // 最高スコア

    private void Start()
    {
        gameOverUI?.SetActive(false);
        gameClearUI?.SetActive(false);

        // 保存されたスコアを読み込む
        bestScore = PlayerPrefs.GetInt("bestScore", 0); // "bestScore"がない場合は0に設定

        // ボタンイベント登録
        restartButton?.onClick.AddListener(Restart);
        titleButton?.onClick.AddListener(ReturnToTitle);
    }

    private void Update()
    {
        if (isGameOver)
        {
            HandleGameOverInput();
        }
        else if (isGameClear)
        {
            // クリア時は Enter でタイトルに戻る
            if (Input.GetButtonDown("A"))
            {
                ReturnToTitle();
            }
        }
    }

    // --- HP表示更新 ---
    public void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"HP: {currentHealth}/{maxHealth}";
        }
    }

    // --- タイマー更新 ---
    public void UpdateTimerUI(float currentTime)
    {
        if (timerText != null)
        {
            timerText.text = $"TIME: {currentTime:00}s";
        }
    }

    // --- ゲームクリア処理 ---
    public void GameClear()
    {
        if (isGameClear) return;

        isGameClear = true;

        if (gameClearUI != null)
            gameClearUI.SetActive(true);
    }

    // --- ゲームオーバー処理 ---
    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            SelectRestartButton(); // 初期選択状態
        }
    }

    // 最終スコアを表示するメソッド
    public void DisplayFinalScore(int finalScore)
    {
        // ゲームオーバー画面やクリア画面で最終スコアを表示
        if (gameOverUI != null || gameClearUI != null)
        {
            scoreText.text = $"最終スコア: {finalScore}";
            
            // 最終スコアを保存する処理
            UpdateBestScore(finalScore);
            Debug.Log($"bestScore:{bestScore}");
        }
    }

    //最高スコアの更新
    public void UpdateBestScore(int finalScore)
    {
        if(finalScore > bestScore)
        {
            bestScore = finalScore;
            PlayerPrefs.SetInt("bestScore", bestScore); // bestScoreを保存
            PlayerPrefs.Save(); // 保存
        }
    }

    // --- リスタート ---
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- タイトルに戻る ---
    public void ReturnToTitle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // ← タイトルシーン名に変更
    }

    // --- 選択操作処理 ---
    private void HandleGameOverInput()
    {
        float verticalMove = Input.GetAxis("Vertical");
        if (verticalMove > 0)
        {
            SelectRestartButton();
        }
        else if (verticalMove < 0)
        {
            SelectTitleButton();
        }

        if (Input.GetButtonDown("A"))
        {
            if (currentSelected == 0) Restart();
            else ReturnToTitle();
        }
    }

    // --- ボタン選択処理 ---
    private void SelectRestartButton()
    {
        currentSelected = 0;
        restartButton.Select();

        // 色変更
        var restartImage = restartButton.GetComponent<Image>();
        var titleImage = titleButton.GetComponent<Image>();
        if (restartImage != null) restartImage.color = selectedColor;
        if (titleImage != null) titleImage.color = normalColor;
    }

    private void SelectTitleButton()
    {
        currentSelected = 1;
        titleButton.Select();

        // 色変更
        var restartImage = restartButton.GetComponent<Image>();
        var titleImage = titleButton.GetComponent<Image>();
        if (restartImage != null) restartImage.color = normalColor;
        if (titleImage != null) titleImage.color = selectedColor;
    }
}
