using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // シーン制御用
using UnityEngine.UI;              // UI表示用
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;                       // 最大体力
    private int currentHealth;                      // 現在の体力
    public float invincibleTime = 2f;               //無敵時間（秒）
    private float invincibleTimer = 0f;
    [SerializeField] private TextMeshProUGUI healthText; // Inspectorで割り当て

    [Header("Move")]
    public float moveSpeed = 5f;                    // 通常の移動速度
    public float dashSpeed = 10f;                   // Shiftキー長押し中の移動速度
    // 空中速度維持用
    private float storedSpeed = 0f;

    [Header("Jump")]
    public float jumpForce = 8f;                    // ジャンプの推進力

    [Header("Crouch")]
    public float crouchSpeed = 2.5f;                // しゃがみ中の移動速度
    [Range(0.2f, 1f)]
    public float crouchHeightRatio = 0.5f;          // しゃがみ中のコライダーの高さ比率 (0.2~1.0)

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private bool isGrounded = false;
    private bool isCrouching = false;

    private Vector2 originalColSize;
    private Vector2 originalColOffset;

    [SerializeField] private Transform visual;       // 見た目用の子(Visual)を割り当てる
    private SpriteRenderer sr;                       // Visual上のSpriteRenderer
    private Vector3 visualOrigScale;                 // Visualの元のスケール
    private Vector3 visualOrigLocalPos;              // Visualの元のローカル位置
    private float visualOrigWorldHeight;             // Visualの元のワールド高さ(足元補正用)

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 0.1f);
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private float moveGroundSpeed = 0f;             //移動床の速度

    private Vector3 spawnPoint;                     // 開始位置

    [Header("UI")]
    public GameObject gameOverUI; // Inspectorで割り当て（Canvas内のGameOverパネル）

    [Header("Game Over")]
    public float fallThreshold = -10f;  // この高さを下回ったらゲームオーバー
    private bool isGameOver = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        currentHealth = maxHealth; // ゲーム開始時は全快
        UpdateHealthUI();
        spawnPoint = transform.position; // 開始位置を記録

        if (gameOverUI != null)
            gameOverUI.SetActive(false); // 開始時は非表示

        // Visualを取得（インスペクタで未設定なら子から探す）
        if (visual == null)
            visual = transform.Find("Visual");

        if (visual != null)
        {
            sr = visual.GetComponent<SpriteRenderer>();
            visualOrigScale = visual.localScale;
            visualOrigLocalPos = visual.localPosition;

            // 元のワールド高さを記録（後で縮小時の足元補正に使用）
            if (sr != null) visualOrigWorldHeight = sr.bounds.size.y;
        }
        else
        {
            Debug.LogWarning("Visual(見た目用の子)が見つかりません。足元補正が無効になります。");
        }

        originalColSize = col.size;
        originalColOffset = col.offset;
    }

    void Update()
    {
        Debug.Log("体力: " + currentHealth);

        // --- 接地判定を毎フレーム更新 ---
        // OverlapBox で足元に床があるか確認
        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,      // 足元中心
            groundCheckSize,           // 判定サイズ (横幅, 高さ)
            0f,                        // 回転なし
            groundLayer
        );

        // デバッグ可視化
        Debug.DrawLine(groundCheck.position + new Vector3(-groundCheckSize.x / 2, 0, 0),
                       groundCheck.position + new Vector3(groundCheckSize.x / 2, 0, 0),
                       Color.green);

        // --- 落下でゲームオーバー判定 ---
        if (!isGameOver && transform.position.y < fallThreshold)
        {
            GameOver();
        }

        // 入力
        float move = Input.GetAxisRaw("Horizontal"); // -1,0,1
        bool holdCrouchKey = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
        bool holdDash = Input.GetKey(KeyCode.LeftShift);

        // --- しゃがみ制御 ---
        // 地上でキーを押したらしゃがみに入る
        if (isGrounded && holdCrouchKey && !isCrouching) EnterCrouch();
        // 立ち上がりは「地上でキーを離したとき」に限定（空中では解除しない＝空中でもしゃがみ姿勢維持）
        if (isGrounded && !holdCrouchKey && isCrouching) ExitCrouch();

        // 速度の選択（しゃがみ中はダッシュ無効）
        float currentSpeed = holdDash ? dashSpeed : moveSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;
        if (isGrounded)
        {
            storedSpeed = currentSpeed;
        }
        else
        {
            // 空中では地上で設定された速度を維持
            currentSpeed = storedSpeed;
        }

        // --- 移動処理 ---
        if (isCrouching && isGrounded)
        {
            // しゃがみ中 & 地面にいる → 移動禁止
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            // 通常移動（空中でしゃがみ中なら低速で移動）
            rb.linearVelocity = new Vector2(move * currentSpeed, rb.linearVelocity.y);
        }

        // ジャンプ入力
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
        {
            float jumpPower = jumpForce;

            if (Input.GetKey(KeyCode.LeftShift) && !isCrouching)
            {
                jumpPower *= 1.2f;
            }
            else if (isCrouching)
            {
                jumpPower *= 0.6f;
            }

                rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }

        //棚橋君の追加コード
        // 無敵時間のカウントダウン
        if (invincibleTimer > 0)
        {
            invincibleTimer -= Time.deltaTime;

            // 半透明にする（alpha = 0.5）
            SetTransparency(0.5f);
        }
        else
        {
            // 通常の表示（alpha = 1）
            SetTransparency(1f);
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (enemyCol != null)
            {
                // OverlapColliderで重なっているかを検知
                ContactFilter2D filter = new ContactFilter2D();
                Collider2D[] results = new Collider2D[1];
                int count = col.Overlap(filter.NoFilter(), results);

                for (int i = 0; i < count; i++)
                {
                    if (results[i] == enemyCol && invincibleTimer <= 0f)
                    {
                        currentHealth -= enemy.GetComponent<Enemy>().damage;
                        Debug.Log("体力: " + currentHealth);
                        UpdateHealthUI();
                        if (currentHealth <= 0)
                        {
                            GameOver();
                        }
                        invincibleTimer = invincibleTime;
                    }
                }
            }
        }
    }

    // --- しゃがみ開始 ---
    private void EnterCrouch()
    {
        if (col == null) return;
        isCrouching = true;

        // 当たり判定は「足を地面に残したまま」縮める
        float newHeight = originalColSize.y * crouchHeightRatio;
        float deltaH = originalColSize.y - newHeight;
        col.size = new Vector2(originalColSize.x, newHeight);
        col.offset = new Vector2(originalColOffset.x, originalColOffset.y - deltaH * 0.5f);

        // 見た目だけ縦に縮小（横幅はそのまま）
        if (visual != null)
        {
            visual.localScale = new Vector3(visualOrigScale.x,
                                             visualOrigScale.y * crouchHeightRatio,
                                             visualOrigScale.z);

            // Pivotが中央などで足が浮く場合に備えて足元を補正
            if (sr != null && visualOrigWorldHeight > 0f)
            {
                float lost = visualOrigWorldHeight * (1f - crouchHeightRatio); // 失われた高さ（ワールド空間）
                // 見た目を半分だけ下げて足元を地面に合わせる（PivotがBottomなら移動量は0になる）
                visual.localPosition = visualOrigLocalPos + new Vector3(0f, -lost * 0.5f, 0f);
            }
        }
    }

    // --- しゃがみ終了 ---
    private void ExitCrouch()
    {
        if (col == null) return;
        isCrouching = false;

        // 当たり判定を元のサイズに戻す
        col.size = originalColSize;
        col.offset = originalColOffset;

        // 見た目も元の状態に戻す
        if (visual != null)
        {
            visual.localScale = visualOrigScale;
            visual.localPosition = visualOrigLocalPos;
        }
    }

    //棚橋君の追加コード
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveGround"))
        {
            Rigidbody2D groundRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (groundRb != null)
            {
                moveGroundSpeed = groundRb.linearVelocity.x;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveGround"))
        {
            moveGroundSpeed = 0f;
        }

    }

    // Spriteの透明度を変更する関数
    private void SetTransparency(float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "HP: " + currentHealth.ToString();
        }
    }

    private void GameOver()
    {
        isGameOver = true;
        Debug.Log("Game Over!");

        // ゲームオーバーUIを表示
        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        // 動きを止める
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false; // Rigidbodyを一時停止

        // ゲーム全体を止める（物理・アニメーション・Update が止まる）
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        // UIを消す必要なし（シーン再ロードで勝手に初期化される）
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // ゲームを再開
        Time.timeScale = 1f;
    }
}
