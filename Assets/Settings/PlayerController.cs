using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // シーン制御用
using UnityEngine.UI;              // UI表示用

public class PlayerController : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;                       // 最大体力
    private int currentHealth;                      // 現在の体力
    public float invincibleTime = 2f;               // 無敵時間（秒）
    private float invincibleTimer = 0f;

    [Header("Move")]
    public float moveSpeed = 5f;                    // 通常の移動速度
    public float dashSpeed = 10f;                   // ダッシュ速度
    private float storedSpeed = 0f;                 // 空中速度維持用
    private int facingDirection = 1;                // 1=右, -1=左

    [Header("Jump")]
    public float jumpForce = 8f;                    // ジャンプの推進力

    [Header("Crouch")]
    public float crouchSpeed = 2.5f;                // しゃがみ中の移動速度
    [Range(0.2f, 1f)]
    public float crouchHeightRatio = 0.5f;          // しゃがみ高さ比率

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;             // 弾のプレハブ
    public float projectileSpeed = 10f;             // 弾の速度
    public Transform firePoint;                     // 発射位置

    [Header("Score Settings")]
    private int score = 0;
    private int Point = 0;
    private int scoreForTime;
    private int scoreForHealth;
    [SerializeField] private ScoreManager scoreManager;  // 未使用でも可

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private bool isGrounded = false;
    private bool isCrouching = false;
    private bool wasGroundedLastFrame = false;      // 着地SE用

    private Vector2 originalColSize;
    private Vector2 originalColOffset;

    [SerializeField] private Transform visual;       // 見た目(子)
    private SpriteRenderer sr;
    private Vector3 visualOrigScale;
    private Vector3 visualOrigLocalPos;
    private float visualOrigWorldHeight;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    private float moveGroundSpeed = 0f;

    private Vector3 spawnPoint;
    private float currentTime;
    private bool isGameOver = false;
    private bool isGameClear = false;

    [Header("Game Over")]
    public float fallThreshold = -10f;

    [SerializeField] private GameUIManager uiManager;

    // ======== ここから：AudioManagerで鳴らすためのクリップ参照のみ保持する ========
    [Header("SE Clips")]
    [SerializeField] private AudioClip seJump;
    [SerializeField] private AudioClip seLand;
    [SerializeField] private AudioClip seFire;
    [SerializeField] private AudioClip seDamage;
    [SerializeField] private AudioClip seItem;
    [SerializeField] private AudioClip seGameOver;
    [SerializeField] private AudioClip seGameClear;
    [SerializeField] private AudioClip seWallHit;

    [Header("Wall Hit SE Settings")]
    [SerializeField, Range(0f, 20f)] private float wallHitSpeedThreshold = 5f;
    [SerializeField, Range(0f, 1f)] private float wallHitCooldownSeconds = 0.15f;
    [SerializeField] private LayerMask wallLayer;   // 壁レイヤー（Tag派なら不要）
    private float wallSeCooldownTimer = 0f;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip bgmStage;                 // ステージBGM
    [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.5f;
    [SerializeField] private float bgmFadeSeconds = 1.0f;

    // ======== ヘルパ ========
    private void BootstrapAudioManagerIfNeeded()
    {
        if (AudioManager.Instance != null) return;
        var go = new GameObject("AudioManager(Auto)");
        go.AddComponent<AudioManager>(); // Awakeで常設化される
    }

    private void PlaySE(AudioClip clip, float vol01 = -1f)
    {
        if (clip == null || AudioManager.Instance == null) return;
        if (vol01 >= 0f) AudioManager.Instance.PlaySE(clip, vol01);
        else AudioManager.Instance.PlaySE(clip);
    }
    // ======== ヘルパここまで ========

    void Start()
    {
        // AudioManagerが無ければ自動生成する（常設化）
        BootstrapAudioManagerIfNeeded();

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();

        currentHealth = maxHealth;
        spawnPoint = transform.position;
        currentTime = uiManager != null ? uiManager.StartTime : 60f;
        score = 0; scoreForTime = 0; scoreForHealth = 0;

        uiManager?.UpdateHealthUI(currentHealth, maxHealth);
        uiManager?.UpdateTimerUI(currentTime);

        if (visual == null) visual = transform.Find("Visual");
        if (visual != null)
        {
            sr = visual.GetComponent<SpriteRenderer>();
            visualOrigScale = visual.localScale;
            visualOrigLocalPos = visual.localPosition;
            if (sr != null) visualOrigWorldHeight = sr.bounds.size.y;
        }
        else
        {
            Debug.LogWarning("Visualが見つからないため，足元補正は無効である．");
        }

        originalColSize = col.size;
        originalColOffset = col.offset;

        if (uiManager == null)
        {
            Debug.LogError("uiManager が未アサインである．CanvasのGameUIManagerを割り当てること．");
        }

        // --- BGM開始（フェードイン） ---
        if (AudioManager.Instance != null && bgmStage != null)
        {
            AudioManager.Instance.PlayBGM(bgmStage, volume: bgmVolume, fadeSec: bgmFadeSeconds, loop: true);
        }
    }

    void Update()
    {
        if (wallSeCooldownTimer > 0f) wallSeCooldownTimer -= Time.deltaTime;

        // 接地判定
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        if (!wasGroundedLastFrame && isGrounded) PlaySE(seLand);
        wasGroundedLastFrame = isGrounded;

        // タイマー
        currentTime -= Time.deltaTime;
        uiManager?.UpdateTimerUI(currentTime);
        if (currentTime <= 0f) { GameOver(); return; }

        // 落下死
        if (!isGameOver && transform.position.y < fallThreshold) { GameOver(); return; }

        // 入力
        float move = Input.GetAxis("Horizontal");
        float verticalMove = Input.GetAxis("Vertical");
        bool holdCrouchKey = (verticalMove < -0.5f);
        bool holdDash = Input.GetAxis("RT") > 0.5f;

        // しゃがみ制御
        if (isGrounded && holdCrouchKey && !isCrouching) EnterCrouch();
        if (isGrounded && !holdCrouchKey && isCrouching) ExitCrouch();

        // 速度決定
        float currentSpeed = holdDash ? dashSpeed : moveSpeed;
        if (isCrouching) currentSpeed = crouchSpeed;
        if (isGrounded) storedSpeed = currentSpeed; else currentSpeed = storedSpeed;

        // 移動
        if (isCrouching && isGrounded)
            rb.linearVelocity = new Vector2(0f + moveGroundSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(move * currentSpeed + moveGroundSpeed, rb.linearVelocity.y);

        // 向き更新
        if (!isCrouching)
        {
            if (move > 0)
            {
                facingDirection = 1;
                if (visual != null) visual.localScale = new Vector3(Mathf.Abs(visualOrigScale.x), visualOrigScale.y, visualOrigScale.z);
                if (firePoint != null) { var p = firePoint.localPosition; p.x = Mathf.Abs(p.x); firePoint.localPosition = p; }
            }
            else if (move < 0)
            {
                facingDirection = -1;
                if (visual != null) visual.localScale = new Vector3(-Mathf.Abs(visualOrigScale.x), visualOrigScale.y, visualOrigScale.z);
                if (firePoint != null) { var p = firePoint.localPosition; p.x = -Mathf.Abs(p.x); firePoint.localPosition = p; }
            }
        }
        else
        {
            if (move > 0)
            {
                facingDirection = 1;
                if (firePoint != null) { var p = firePoint.localPosition; p.x = Mathf.Abs(p.x); firePoint.localPosition = p; }
            }
            else if (move < 0)
            {
                facingDirection = -1;
                if (firePoint != null) { var p = firePoint.localPosition; p.x = -Mathf.Abs(p.x); firePoint.localPosition = p; }
            }
        }

        // ジャンプ
        if (Input.GetButtonDown("A") && isGrounded)
        {
            float jumpPower = jumpForce;
            if (holdDash && !isCrouching) jumpPower *= 1.2f;
            else if (isCrouching) jumpPower *= 0.8f;

            rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            PlaySE(seJump);
        }

        // 射撃
        if (Input.GetButtonDown("B")) FireProjectile();

        // 無敵時間
        if (invincibleTimer > 0) { invincibleTimer -= Time.deltaTime; SetTransparency(0.5f); }
        else { SetTransparency(1f); }

        // 当たり判定
        HandleEnemyCollision();
    }

    // しゃがみ開始
    private void EnterCrouch()
    {
        if (col == null) return;
        isCrouching = true;

        float newH = originalColSize.y * crouchHeightRatio;
        float deltaH = originalColSize.y - newH;
        col.size = new Vector2(originalColSize.x, newH);
        col.offset = new Vector2(originalColOffset.x, originalColOffset.y - deltaH * 0.5f);

        if (visual != null)
        {
            visual.localScale = new Vector3(visualOrigScale.x, visualOrigScale.y * crouchHeightRatio, visualOrigScale.z);
            if (sr != null && visualOrigWorldHeight > 0f)
            {
                float lost = visualOrigWorldHeight * (1f - crouchHeightRatio);
                visual.localPosition = visualOrigLocalPos + new Vector3(0f, -lost * 0.5f, 0f);
            }
        }
        // しゃがみSEは鳴らさない
    }

    // しゃがみ終了
    private void ExitCrouch()
    {
        if (col == null) return;
        if (CanStandUp())
        {
            isCrouching = false;
            col.size = originalColSize;
            col.offset = originalColOffset;
            if (visual != null) { visual.localScale = visualOrigScale; visual.localPosition = visualOrigLocalPos; }
            // 立ち上がりSEも鳴らさない
        }
        else
        {
            Debug.Log("天井があるため立てない．しゃがみ維持である．");
        }
    }

    private bool CanStandUp()
    {
        Vector2 center = (Vector2)transform.position + originalColOffset;
        Vector2 chk = originalColSize * 0.98f;
        Collider2D hit = Physics2D.OverlapBox(center, chk, 0f, groundLayer);
        return hit == null;
    }

    // 弾発射
    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null) return;

        int currentBullets = GameObject.FindGameObjectsWithTag("Projectile").Length;
        if (currentBullets >= 2) { Debug.Log("弾の上限に達しているため発射しない．"); return; }

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        Projectile p = projectile.GetComponent<Projectile>();
        if (p != null) p.SetShooter(GetComponent<Collider2D>());

        Rigidbody2D rb2 = projectile.GetComponent<Rigidbody2D>();
        if (rb2 != null) rb2.linearVelocity = new Vector2(facingDirection * projectileSpeed, 0f);

        Destroy(projectile, 1.5f);
        PlaySE(seFire);
    }

    // 敵・アイテム接触
    private void HandleEnemyCollision()
    {
        if (isGameOver || isGameClear) return;

        // 敵
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Collider2D enemyCol = enemy.GetComponent<Collider2D>();
            if (enemyCol != null)
            {
                ContactFilter2D filter = new ContactFilter2D();
                Collider2D[] results = new Collider2D[1];
                int count = col.Overlap(filter.NoFilter(), results);
                for (int i = 0; i < count; i++)
                {
                    if (results[i] == enemyCol && invincibleTimer <= 0f)
                    {
                        currentHealth -= enemy.GetComponent<IEnemyStatus>().Damage;
                        PlaySE(seDamage);
                        uiManager?.UpdateHealthUI(currentHealth, maxHealth);
                        if (currentHealth <= 0) { GameOver(); return; }
                        invincibleTimer = invincibleTime;
                    }
                }
            }
        }

        // アイテム
        GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
        foreach (GameObject item in items)
        {
            Collider2D itemCol = item.GetComponent<Collider2D>();
            if (itemCol != null)
            {
                ContactFilter2D filter = new ContactFilter2D();
                Collider2D[] results = new Collider2D[10];
                int count = col.Overlap(filter.NoFilter(), results);
                for (int i = 0; i < count; i++)
                {
                    if (results[i] == itemCol)
                    {
                        AddPoint(item.GetComponent<Item>().point);
                        PlaySE(seItem);
                        Destroy(item);
                    }
                }
            }
        }
    }

    // 見た目透明度
    private void SetTransparency(float alpha)
    {
        if (sr == null) return;
        Color c = sr.color; c.a = alpha; sr.color = c;
    }

    // 壁衝突SE
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 壁レイヤー判定（Tag派は CompareTag("Wall") に置換可）
        bool isWall = (wallLayer.value != 0 && ((1 << collision.gameObject.layer) & wallLayer.value) != 0);
        if (!isWall) return;

        // 横方向の衝突のみ
        bool horizontalHit = false;
        foreach (var cp in collision.contacts)
        {
            Vector2 n = cp.normal;
            if (Mathf.Abs(n.y) < 0.5f) { horizontalHit = true; break; }
        }
        if (!horizontalHit) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < wallHitSpeedThreshold) return;

        if (wallSeCooldownTimer <= 0f)
        {
            float vol = Mathf.Lerp(0.4f, 1.0f,
                Mathf.InverseLerp(wallHitSpeedThreshold, wallHitSpeedThreshold * 2.5f, impactSpeed));
            PlaySE(seWallHit, vol);
            wallSeCooldownTimer = wallHitCooldownSeconds;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveGround"))
        {
            Rigidbody2D groundRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (groundRb != null) moveGroundSpeed = groundRb.linearVelocity.x;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MoveGround")) moveGroundSpeed = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isGameOver || isGameClear) return;
        if (other.CompareTag("Goal")) GameClear();
    }

    private void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        // BGMフェードアウトと効果音
        if (AudioManager.Instance != null) AudioManager.Instance.StopBGM(fadeSec: 1.0f);
        PlaySE(seGameOver);

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Time.timeScale = 0f;

        CalculateFinalScore();
        uiManager?.GameOver();
        UpdateBestScore(score);
    }

    private void GameClear()
    {
        if (isGameClear) return;
        isGameClear = true;

        if (AudioManager.Instance != null) AudioManager.Instance.StopBGM(fadeSec: 1.0f);
        PlaySE(seGameClear);

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        Time.timeScale = 0f;

        CalculateFinalScore();
        uiManager?.GameClear();
        UpdateBestScore(score);
    }

    public void AddPoint(int amount)
    {
        Point += amount;
        Debug.Log("現在のポイント: " + Point);
    }

    public void CalculateFinalScore()
    {
        if (isGameClear)
        {
            scoreForTime = Mathf.FloorToInt(currentTime * 10);
            scoreForHealth = Mathf.FloorToInt(currentHealth * 10);
        }
        score = Point + scoreForTime + scoreForHealth;
        uiManager?.DisplayFinalScore(score);
    }

    private void UpdateBestScore(int currentScore)
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (currentScore > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", currentScore);
            PlayerPrefs.Save();
        }
        Debug.Log("Best Score Updated: " + PlayerPrefs.GetInt("BestScore"));
    }
}
