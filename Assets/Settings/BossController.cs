using System.Diagnostics;
using UnityEngine;

public class Boss : MonoBehaviour, IEnemyStatus
{
    public int damage = 3;
    public float moveSpeed = 2f;
    public float detectRadius = 12f;

    public int Damage { get; set; }
    public float MoveSpeed { get; set; }
    public float DetectRadius { get; set; }

    public float coolTime = 15f; // 攻撃間隔（秒）
    private float coolTimer = 0f;
    private float attackTimer = 0f; // 攻撃時間
    private int attack_id = 0; // 攻撃方法を指定するid
    bool isJumped = false; // ジャンプしたかを管理するフラグ
    bool isAttacking = false; // 攻撃中かを判断するフラグ

    private Rigidbody2D rb;
    private Transform player;

    private bool isGrounded = false;

    void Start()
    {
        Damage = damage;
        MoveSpeed = moveSpeed;
        DetectRadius = detectRadius;

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
            UnityEngine.Debug.Log(coolTimer);
            //プレイヤーが近くにいるとき、クールタイムが回復する
            if (coolTimer != 0)
            {
                coolTimer -= Time.deltaTime;
            }

            //クールタイムが1以下のときに移動停止(攻撃前に1秒間停止する)
            if (coolTimer <= 1)
            {
                //クールタイムが0かつ、攻撃中でないとき
                if (coolTimer <= 0 && isAttacking == false)
                {
                    attack_id = UnityEngine.Random.Range(0, 1); //攻撃方法の抽選(0しかでない)

                    //ジャンプ攻撃のとき
                    if (attack_id == 0)
                    {
                        //地面の上にいるかつ、ジャンプしてないとき
                        if (isGrounded && !isJumped)
                        {
                            UnityEngine.Vector2 targetPos = new UnityEngine.Vector2(player.position.x, player.position.y + 100f); // プレイヤーの頭上をターゲット設定
                            UnityEngine.Vector2 dir = (targetPos - (UnityEngine.Vector2)transform.position).normalized; // ベクトルを設定
                            dir.x *= 3.5f;
                            rb.AddForce(dir * 10f, ForceMode2D.Impulse); // ジャンプする

                            isAttacking = true; // 攻撃フラグ
                            isJumped = true; // ジャンプフラグを立てる
                        }
                    }
                }
            }
            else if (isGrounded) //クールタイム中、地面の上にいるときに移動可能な状態になる
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
                    attackTimer = 2f; // 攻撃時間を2秒とする
                }
                //着地してから2秒以内の場合
                else if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;

                    // 2秒経過した場合
                    if (attackTimer <= 0)
                    {
                        // 攻撃終了
                        isAttacking = false;
                        //クールタイムリセット
                        coolTimer = coolTime;

                    }
                }
            }

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