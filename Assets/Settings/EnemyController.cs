using UnityEngine;

public class Enemy : MonoBehaviour,IEnemyStatus
{
    public int damage = 1;
    public float moveSpeed = 3f;    // �G�̈ړ����x
    public float detectRadius = 10f; // �v���C���[���m�͈�

    public int Damage { get; set; }
    public float MoveSpeed { get; set; }
    public float DetectRadius { get; set; }

    private Rigidbody2D rb;
    private Transform player;
    private bool isGrounded = false;

    void Start()
    {
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
