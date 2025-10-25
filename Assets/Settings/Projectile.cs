using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1; // �e�̍U����

    private Collider2D shooter; // �e���������v���C���[�̃R���C�_�[

    public void SetShooter(Collider2D shooterCollider)
    {
        shooter = shooterCollider;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �������l�i�v���C���[�Ȃǁj�Ƃ͏Փ˂��Ȃ�
        if (collision == shooter)
            return;

        // �G�ɓ���������_���[�W
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Boss boss = collision.GetComponent<Boss>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // ����ȊO�̂��́i�ǂȂǁj�ɓ���������폜
        Destroy(gameObject);
    }
}
