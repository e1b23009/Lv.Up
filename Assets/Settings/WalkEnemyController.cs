using System.Drawing;
using UnityEngine;

public class WalkEnemy : MonoBehaviour,IEnemyStatus
{
    [Header("攻撃設定")]
    public int damage = 1;             // プレイヤーに与えるダメージ
    public float moveSpeed = 3f;       // 移動速度

    [Header("体力設定")]
    public int maxHealth = 3;          // 最大体力
    private int currentHealth;         // 現在の体力

    public int point = 10; // 倒したときにもらえるポイント

    public int Damage { get; set; }
    public float MoveSpeed { get; set; }
    public float DetectRadius { get; set; }

    private Rigidbody2D rb;
    private Transform player;
    private bool isGrounded = false;
    private bool isFacingRight = false;
    private int groundContactCount = 0;

    void Start()
    {
        Damage = damage;
        MoveSpeed = moveSpeed;
        DetectRadius = 0;
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
        if (!isGrounded) return;

        // �v���C���[�܂ł̋������v�Z
        float distance = Vector2.Distance(transform.position, player.position);

        // ���a detectRadius �ȓ��Ȃ�v���C���[��x���W�Ɍ������Ĉړ�

        Vector2 targetPos;
        
        if (isFacingRight)
        {
            targetPos = new Vector2(rb.position.x + 1, rb.position.y); // 右向きの場合
        }
        else
        {
            targetPos = new Vector2(rb.position.x - 1, rb.position.y); // 左向きの場合
        }
        Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
        
    }

    // �n�ʂƂ̐ڐG����i�^�O�Ŕ���j
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))
        {
            groundContactCount++;
            isGrounded = true;
            if (groundContactCount == 2) // 壁と床に同時に接触している状態
            {
                if (isFacingRight) // 右を向いている場合
                {
                    isFacingRight = false;
                }
                else // 左向きの場合
                {
                    isFacingRight = true;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Enemy"))
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

        // プレイヤーにポイント加算
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.AddPoint(point);
        }

        Destroy(gameObject);
    }
}
