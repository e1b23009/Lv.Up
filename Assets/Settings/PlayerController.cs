using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // シーン制御用
using UnityEngine.UI;              // UI表示用

public class PlayerController : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;                       // 最大体力
    private int currentHealth;                      // 現在の体力
    public float invincibleTime = 2f;               //無敵時間（秒）
    private float invincibleTimer = 0f;

    [Header("Move")]
    public float moveSpeed = 5f;                    // 通常の移動速度
    public float dashSpeed = 10f;                   // Shiftキー長押し中の移動速度
    // 空中速度維持用
    private float storedSpeed = 0f;
    private int facingDirection = 1; // 1 = 右向き, -1 = 左向き

    [Header("Jump")]
    public float jumpForce = 8f;                    // ジャンプの推進力

    [Header("Crouch")]
    public float crouchSpeed = 2.5f;                // しゃがみ中の移動速度
    [Range(0.2f, 1f)]
    public float crouchHeightRatio = 0.5f;          // しゃがみ中のコライダーの高さ比率 (0.2~1.0)

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;  // 弾のプレハブ
    public float projectileSpeed = 10f;  // 弾の速度
    public Transform firePoint;          // 弾を発射する位置

    [Header("Score Settings")]
    private int score = 0; // 現在のスコア
    private int Point = 0; // ポイント(アイテムを取ると上昇)
    private int scoreForTime; // 時間で加算するスコア
    private int scoreForHealth;

    [SerializeField] private ScoreManager scoreManager;  // ScoreManagerを参照

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
    [SerializeField] private LayerMask groundLayer;

    private float moveGroundSpeed = 0f;             //移動床の速度

    private Vector3 spawnPoint;                     // 開始位置
    private float currentTime;
    private bool isGameOver = false;
    private bool isGameClear = false;

    [Header("Game Over")]
    public float fallThreshold = -10f;  // この高さを下回ったらゲームオーバー

    // GameUIManager への参照
    [SerializeField] private GameUIManager uiManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        // 体力・時間初期化
        currentHealth = maxHealth;
        spawnPoint = transform.position;
        currentTime = uiManager != null ? uiManager.StartTime : 60f;
        score = 0;
        scoreForTime = 0;
        scoreForHealth = 0;

        // 初期UI表示
        uiManager?.UpdateHealthUI(currentHealth, maxHealth);
        uiManager?.UpdateTimerUI(currentTime);

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

        if (uiManager == null)
        {
            Debug.LogError("uiManager がアサインされていません。CanvasをPlayerのUiManager欄にドラッグしてください。");
        }
        else
        {
            Debug.Log("uiManager が接続されています。");
        }
    }

    void Update()
    {
        // --- 接地判定を毎フレーム更新 ---
        // OverlapBox で足元に床があるか確認
        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,      // 足元中心
            groundCheckSize,           // 判定サイズ (横幅, 高さ)
            0f,                        // 回転なし
            groundLayer
        );

        // --- タイマー処理 ---
        currentTime -= Time.deltaTime;
        uiManager?.UpdateTimerUI(currentTime);
        if (currentTime <= 0f)
        {
            GameOver();
            return;
        }

        // --- 落下によるゲームオーバー判定 ---
        if (!isGameOver && transform.position.y < fallThreshold)
        {
            GameOver();
            return;
        }

        // 入力
        float move = Input.GetAxis("Horizontal");  // 横方向 (左スティック)
        float verticalMove = Input.GetAxis("Vertical");
        bool holdCrouchKey = (verticalMove < -0.5);
        bool holdDash = Input.GetAxis("RT") > 0.5f;

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

        // --- プレイヤーの向きを更新（しゃがみ中はスケール変更しない） ---
        if (!isCrouching)
        {
            if (move > 0)
            {
                facingDirection = 1;
                visual.localScale = new Vector3(Mathf.Abs(visualOrigScale.x), visualOrigScale.y, visualOrigScale.z);

                // FirePointも右側に配置
                if (firePoint != null)
                {
                    Vector3 pos = firePoint.localPosition;
                    pos.x = Mathf.Abs(pos.x);
                    firePoint.localPosition = pos;
                }
            }
            else if (move < 0)
            {
                facingDirection = -1;
                visual.localScale = new Vector3(-Mathf.Abs(visualOrigScale.x), visualOrigScale.y, visualOrigScale.z);

                // FirePointも左側に配置
                if (firePoint != null)
                {
                    Vector3 pos = firePoint.localPosition;
                    pos.x = -Mathf.Abs(pos.x);
                    firePoint.localPosition = pos;
                }
            }
        }
        else
        {
            // しゃがみ中でも向きだけは変えたい場合（見た目は縮めたまま）
            if (move > 0)
            {
                facingDirection = 1;
                if (firePoint != null)
                {
                    Vector3 pos = firePoint.localPosition;
                    pos.x = Mathf.Abs(pos.x);
                    firePoint.localPosition = pos;
                }
            }
            else if (move < 0)
            {
                facingDirection = -1;
                if (firePoint != null)
                {
                    Vector3 pos = firePoint.localPosition;
                    pos.x = -Mathf.Abs(pos.x);
                    firePoint.localPosition = pos;
                }
            }
        }

        // ジャンプ入力
        if (Input.GetButtonDown("A") && isGrounded)
        {
            float jumpPower = jumpForce;

            if (holdDash && !isCrouching)
            {
                jumpPower *= 1.2f;
            }
            else if (isCrouching)
            {
                jumpPower *= 0.8f;
            }

            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }

        // 弾を発射
        if (Input.GetButtonDown("B"))
        {
            FireProjectile();
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

        // --- 敵との接触判定 ---
        HandleEnemyCollision();
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

        // 立ち上がったときに天井に当たるかどうかを確認
        if (CanStandUp())
        {
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
        else
        {
            // 立ち上がれなければしゃがみ続ける
            Debug.Log("天井があるため立てません。しゃがみ姿勢を維持します。");
        }
    }

    // --- 立てるかどうかを確認する関数 ---
    private bool CanStandUp()
    {
        // 立ち上がるために必要なスペースがあるかを判定
        Vector2 center = (Vector2)transform.position + originalColOffset;
        Vector2 checkSize = originalColSize * 0.98f;  // 少し小さくして余裕を持たせる
        Collider2D hit = Physics2D.OverlapBox(center, checkSize, 0f, groundLayer);

        // 重なりがなければ立てる
        return hit == null;
    }

    // 弾を発射する処理
    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        // --- 現在の弾の数を確認 ---
        int currentBullets = GameObject.FindGameObjectsWithTag("Projectile").Length;
        if (currentBullets >= 2)
        {
            Debug.Log("弾が上限に達しています。");
            return; // 弾が2発以上あるなら発射しない
        }

        // 弾を発射する位置から弾のインスタンスを生成
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // 自分との衝突を無視
        Projectile p = projectile.GetComponent<Projectile>();
        if (p != null)
        {
            p.SetShooter(GetComponent<Collider2D>());
        }

        // 弾に速度を与える（Rigidbody2Dを使って発射する）
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // 右向きなら右へ、左向きなら左へ発射
            rb.linearVelocity = new Vector2(facingDirection * projectileSpeed, 0f);
        }

        // 1.5秒後に自動で消す
        Destroy(projectile, 1.5f);
    }

    //棚橋君の追加コード
    private void HandleEnemyCollision()
    {
        if (isGameOver || isGameClear) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>(); if (enemyCol != null)
            { // OverlapColliderで重なっているかを検知
                ContactFilter2D filter = new ContactFilter2D();
                Collider2D[] results = new Collider2D[1];
                int count = col.Overlap(filter.NoFilter(), results);
                for (int i = 0; i < count; i++)
                {
                    if (results[i] == enemyCol && invincibleTimer <= 0f)
                    {
                        currentHealth -= enemy.GetComponent<IEnemyStatus>().Damage;
                        Debug.Log("体力: " + currentHealth);
                        uiManager?.UpdateHealthUI(currentHealth, maxHealth);
                        if (currentHealth <= 0)
                        {
                            GameOver();
                            return;
                        }
                        invincibleTimer = invincibleTime;
                    }
                }
            }
        }

        // アイテムとの接触判定
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Collider2D itemCol = item.GetComponent<Collider2D>();

            if (itemCol != null)
            {
                // OverlapColliderで重なっているかを検知
                ContactFilter2D filter = new ContactFilter2D();
                Collider2D[] results = new Collider2D[10];
                int count = col.Overlap(filter.NoFilter(), results);

                for (int i = 0; i < count; i++)
                {

                    if (results[i] == itemCol)
                    {
                        AddPoint(item.GetComponent<Item>().point);
                        Destroy(item);
                    }
                }
            }
        }
    }

    // Spriteの透明度を変更する関数
    private void SetTransparency(float alpha)
    {
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("何かに衝突しました: " + collision.gameObject.name);
    }

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver || isGameClear) return; // ゲームオーバーやゲームクリア時は何もしない

        // ゴールに触れたらゲームクリア
        if (other.CompareTag("Goal")) // ゴールタグが設定されたオブジェクトに触れる
        {
            GameClear();
        }
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        Time.timeScale = 0f;

        // スコア計算
        CalculateFinalScore();

        // --- UI側に通知 ---
        uiManager?.GameOver();

        // ベストスコアの更新
        UpdateBestScore(score);
    }

    private void GameClear()
    {
        if (isGameClear) return;
        isGameClear = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        Time.timeScale = 0f;

        // スコア計算
        CalculateFinalScore();

        // --- UI側に通知 ---
        uiManager?.GameClear();

        // ベストスコアの更新
        UpdateBestScore(score);
    }


    public void AddPoint(int amount)
    {
        Point += amount;
        Debug.Log("現在のポイント: " + Point);
    }

    // スコア計算
    public void CalculateFinalScore()
    {
        // 残りの時間に応じてスコアを増加
        if (isGameClear)
        {
            scoreForTime = Mathf.FloorToInt(currentTime * 10); // 時間に基づいてスコアを増加（1秒あたり10ポイント）
            scoreForHealth = Mathf.FloorToInt(currentHealth * 10); // 体力に基づいてスコアを増加（体力1あたり10ポイント
        }
        score = Point + scoreForTime + scoreForHealth;

        Debug.Log("最終スコア: " + score);
        uiManager?.DisplayFinalScore(score);
    }

    private void UpdateBestScore(int currentScore)
    {
        // 現在のスコアがベストスコアより大きい場合に更新
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (currentScore > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", currentScore);  // 新しいベストスコアを保存
            PlayerPrefs.Save();  // 保存
        }
        Debug.Log("Best Score Updated: " + PlayerPrefs.GetInt("BestScore"));
    }
}
