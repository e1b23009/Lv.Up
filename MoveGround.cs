
// MoveGrondを使用する際には、RigidBody2DにあるFreeze Position:YとFreeze Rotasion:Zにチェックを付けてください

using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float moveSpeed = 2f; // 移動スピード
    private float direction = 1f; // 移動方向（1 = 右, -1 = 左）

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 一定間隔で ReverseDirection を呼び出す
        InvokeRepeating("ReverseDirection", 2f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        // Rigidbody2Dの速度を変更
        rb.linearVelocity = new Vector2(moveSpeed * direction, rb.linearVelocity.y);

    }

    void ReverseDirection()
    {
        direction *= -1f; // 方向を反転
    }
}
