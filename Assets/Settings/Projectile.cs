using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 1; // 弾の攻撃力

    private Collider2D shooter; // 弾を撃ったプレイヤーのコライダー

    public void SetShooter(Collider2D shooterCollider)
    {
        shooter = shooterCollider;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 撃った人（プレイヤーなど）とは衝突しない
        if (collision == shooter)
            return;

        // 敵に当たったらダメージ
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

        // それ以外のもの（壁など）に当たったら削除
        Destroy(gameObject);
    }
}
