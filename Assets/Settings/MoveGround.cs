
// MoveGrond���g�p����ۂɂ́ARigidBody2D�ɂ���Freeze Position:Y��Freeze Rotasion:Z�Ƀ`�F�b�N��t���Ă�������

using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float moveSpeed = 2f; // �ړ��X�s�[�h
    private float direction = 1f; // �ړ������i1 = �E, -1 = ���j

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // ���Ԋu�� ReverseDirection ���Ăяo��
        InvokeRepeating("ReverseDirection", 2f, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        // Rigidbody2D�̑��x��ύX
        rb.linearVelocity = new Vector2(moveSpeed * direction, rb.linearVelocity.y);

    }

    void ReverseDirection()
    {
        direction *= -1f; // �����𔽓]
    }
}
