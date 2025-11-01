using UnityEngine;

public class LastBoss : MonoBehaviour, IEnemyStatus
{
    public int damage = 3;
    public float moveSpeed = 2f;
    public float detectRadius = 12f;
    public float elasticity = 2f; // ジャンプ攻撃時の伸縮性

    [Header("体力設定")]
    public int maxHealth = 10;
    private int currentHealth;

    public int point = 50; // 倒したときにもらえるポイント

    public int Damage { get; set; }
    public float MoveSpeed { get; set; }
    public float DetectRadius { get; set; }

    public float coolTime = 15f; // 攻撃間隔（秒）

    public GameObject enemyPrefab; // 生成する敵のプレハブ
    private float coolTimer = 0f;
    private float attackTimer = 0f; // 攻撃時間
    private int attack_id = 0; // 攻撃方法を指定するid
    bool isJumped = false; // ジャンプしたかを管理するフラグ
    bool isAttacking = false; // 攻撃中かを判断するフラグ
    private int attackCount = 0; // 召喚攻撃の回数

    private Rigidbody2D rb;
    private Transform player;
    private SpriteRenderer sr;
    private Vector3 originalScale;

    private bool isGrounded = false;
    private int groundContactCount = 0;
    private bool isFacingRight = true;

    private GameObject enemy; 

    void Start()
    {
        Damage = damage;
        MoveSpeed = moveSpeed;
        DetectRadius = detectRadius;

        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();

        sr = GetComponent<SpriteRenderer>(); // 追加
        originalScale = transform.localScale; // 追加

        // �v���C���[�ƕ����Փ˂𖳎�����
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform; // Transform�̓I�u�W�F�N�g�̈ʒu�iposition�j�A��]�irotation�j�A�X�P�[���iscale�j���Ǘ�
            Collider2D playerCol = playerObj.GetComponent<Collider2D>();
            Collider2D myCol = GetComponent<Collider2D>();
            if (playerCol != null && myCol != null)
            {
                Physics2D.IgnoreCollision(myCol, playerCol, true);
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null || !isGrounded) return;

        // �v���C���[�܂ł̋������v�Z
        float distance = Vector2.Distance(transform.position, player.position);

        // ���a detectRadius �ȓ��Ȃ�v���C���[��x���W�Ɍ������Ĉړ�
        if (distance <= detectRadius)
        {
            //プレイヤーが近くにいるとき、クールタイムが回復する
            if (coolTimer != 0)
            {
                coolTimer -= Time.deltaTime;
            }

            //クールタイムが0.25以下のときに移動停止(攻撃前に0.25秒間停止する)
            if (coolTimer <= 0.25)
            {
                //クールタイムが0かつ、攻撃中でないとき
                if (coolTimer <= 0 && isAttacking == false)
                {
                    attack_id = UnityEngine.Random.Range(0, 2); //攻撃方法の抽選

                    //ジャンプ攻撃のとき
                    if (attack_id == 0)
                    {
                        //地面の上にいるかつ、ジャンプしてないとき
                        if (isGrounded && !isJumped)
                        {

                            UnityEngine.Vector2 targetPos = new UnityEngine.Vector2(player.position.x, player.position.y + 100f);
                            UnityEngine.Vector2 dir = (targetPos - (UnityEngine.Vector2)transform.position).normalized;
                            dir.x *= 3.5f;
                            rb.AddForce(dir * 5f, ForceMode2D.Impulse);

                            isAttacking = true; // 攻撃フラグ
                            isJumped = true; // ジャンプフラグを立てる
                            isGrounded = false;
                        }
                    }
                    //召喚攻撃のとき
                    else if (attack_id == 1)
                    {
                        if (player.position.x <= rb.position.x)
                        {
                            isFacingRight = true;
                        }
                        else
                        {
                            isFacingRight = false;
                        }
                        attackTimer = 2f;
                        isAttacking = true;
                        attackCount = 0;
                    }
                }
            }
            else if (isGrounded) //クールタイム中、地面の上にいるときに移動する
            {
                Vector2 targetPos = new Vector2(player.position.x, rb.position.y); // y�͕ς��Ȃ�
                Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
            }

        }
        //攻撃中
        if (isAttacking == true)
        {
            // ジャンプ攻撃時、地面に触れているとき
            if (attack_id == 0 && isGrounded)
            {
                //ジャンプ中の場合
                if (isJumped)
                {
                    isJumped = false;
                    attackTimer = 0.75f; // 攻撃時間を0.5秒とする
                }
                //着地してから1.5秒以内の場合
                else if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;

                    float scaleFactor = 1.0f + Mathf.Sin((0.75f - attackTimer)) * elasticity;
                    transform.localScale = new Vector3(originalScale.x * scaleFactor, originalScale.y / scaleFactor, originalScale.z);
                    transform.position = new Vector2(rb.position.x, rb.position.y - (originalScale.y / scaleFactor) / 5.0f);

                    // attackTimer秒過ぎた場合
                    if (attackTimer <= 0)
                    {
                        transform.localScale = originalScale; // 元に戻す
                        isAttacking = false; // 攻撃終了
                        //クールタイムリセット
                        coolTimer = coolTime;

                    }
                }
            } else if (attack_id == 1)
            {
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                    
                    if(enemyPrefab != null)
                    {
                        if (attackTimer <= 2.0f)
                        {
                            if (attackTimer <= 1.6f)
                            {
                                if (attackTimer <= 1.2f)
                                {
                                    if (attackTimer <= 0.8f)
                                    {
                                        if (attackTimer <= 0.4f)
                                        {
                                            if (attackTimer > 0f)
                                            {
                                                if (attackCount == 4)
                                                {
                                                    if (isFacingRight)
                                                    {
                                                        enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x - 2f, rb.position.y + 1f, 0f), Quaternion.identity);
                                                    }
                                                    else
                                                    {
                                                        enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x + 2f, rb.position.y + 1f, 0f), Quaternion.identity);
                                                        FlyEnemy fe = enemy.GetComponent<FlyEnemy>();
                                                        fe.SetFacing(true);
                                                    }
                                                    attackCount = 5;
                                                }
                                            }


                                        }
                                        if (attackCount == 3)
                                        {
                                            if (isFacingRight)
                                            {
                                                enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x - 2f, rb.position.y + 1f, 0f), Quaternion.identity);
                                            }
                                            else
                                            {
                                                enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x + 2f, rb.position.y + 1f, 0f), Quaternion.identity);
                                                FlyEnemy fe = enemy.GetComponent<FlyEnemy>();
                                                fe.SetFacing(true);
                                            }
                                            attackCount = 4;

                                        }
                                    }
                                    if (attackCount == 2)
                                    {
                                        if (isFacingRight)
                                        {
                                            enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x - 2f, rb.position.y, 0f), Quaternion.identity);
                                        }
                                        else
                                        {
                                            enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x + 2f, rb.position.y, 0f), Quaternion.identity);
                                            FlyEnemy fe = enemy.GetComponent<FlyEnemy>();
                                            fe.SetFacing(true);
                                        }
                                        attackCount = 3;
                                    }
                                }
                                if (attackCount == 1)
                                {
                                    if (isFacingRight)
                                    {
                                        enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x - 2f, rb.position.y - 1f, 0f), Quaternion.identity);
                                    }
                                    else
                                    {
                                        enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x + 2f, rb.position.y - 1f, 0f), Quaternion.identity);
                                        FlyEnemy fe = enemy.GetComponent<FlyEnemy>();
                                        fe.SetFacing(true);
                                    }
                                    attackCount = 2;
                                }
                            }
                                if (attackCount == 0)
                                {
                                    if (isFacingRight)
                                    {
                                        enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x - 2f, rb.position.y - 1f, 0f), Quaternion.identity);
                                    }
                                    else
                                    {
                                        enemy = Instantiate(enemyPrefab, new Vector3(rb.position.x + 2f, rb.position.y - 1f, 0f), Quaternion.identity);
                                        FlyEnemy fe = enemy.GetComponent<FlyEnemy>();
                                        fe.SetFacing(true);
                                    }
                                    attackCount = 1;
                                }
                        }
                    }
                }
                else
                {
                    // 攻撃終了
                    isAttacking = false; 
                    //クールタイムリセット
                    coolTimer = coolTime;
                }
            }

        }

    }

    // �n�ʂƂ̐ڐG����i�^�O�Ŕ���j
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MoveGround"))
        {
            groundContactCount++;
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("MoveGround"))
        {
            groundContactCount--;
            if (groundContactCount <= 0)
            {
                groundContactCount = 0;
                isGrounded = false;
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        UnityEngine.Debug.Log($"Bossが{amount}ダメージを受けた！残りHP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        UnityEngine.Debug.Log("Bossを倒した！");

        // プレイヤーにポイント加算
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.AddPoint(point);
        }

        Destroy(gameObject);
    }

}