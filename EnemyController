using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int damage = 1;
    public float moveSpeed = 3f;    // 敵の移動速度
    public float detectRadius = 10f; // プレイヤー感知範囲

    private Rigidbody2D rb;
    private Transform player;
    private bool isGrounded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // プレイヤーと物理衝突を無視する
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform; // Transformはオブジェクトの位置（position）、回転（rotation）、スケール（scale）を管理
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

        // プレイヤーまでの距離を計算
        float distance = Vector2.Distance(transform.position, player.position);

        // 半径 detectRadius 以内ならプレイヤーのx座標に向かって移動
        if (distance <= detectRadius)
        {
            Vector2 targetPos = new Vector2(player.position.x, rb.position.y); // yは変えない
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }
    }

    // 地面との接触判定（タグで判定）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
