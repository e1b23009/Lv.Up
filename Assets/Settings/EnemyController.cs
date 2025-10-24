using UnityEngine;

public class Enemy : MonoBehaviour,IEnemyStatus
{
    [Header("攻撃・AI設定")]
    public int damage = 1;             // プレイヤーに与えるダメージ
    public float moveSpeed = 3f;       // 移動速度
    public float detectRadius = 10f;   // プレイヤーを検知する距離

    [Header("体力設定")]
    public int maxHealth = 3;          // 最大体力
    private int currentHealth;         // 現在の体力

    public int Damage { get; set; }
    public float MoveSpeed { get; set; }
    public float DetectRadius { get; set; }

    private Rigidbody2D rb;
    private Transform player;
    private bool isGrounded = false;
    private int groundContactCount = 0;

    void Start()
    {
        // ステータス初期化
        Damage = damage;
        MoveSpeed = moveSpeed;
        DetectRadius = detectRadius;
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();

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
            Vector2 targetPos = new Vector2(player.position.x, rb.position.y); // y�͕ς��Ȃ�
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
    }

    // �n�ʂƂ̐ڐG����i�^�O�Ŕ���j
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContactCount++;
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            groundContactCount--;
            if (groundContactCount <= 0)
            {
                groundContactCount = 0;
                isGrounded = false;
            }
        }
    }

    // === ここから追加部分 ===
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} が {amount} ダメージを受けた！（残りHP: {currentHealth}）");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} が倒れた！");
        Destroy(gameObject);
    }
}
