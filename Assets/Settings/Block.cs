using UnityEngine;

public class Block : MonoBehaviour
{
    private bool playerNearby = false;
    // 出現させるアイテムのPrefabをインスペクタから指定
    public GameObject itemPrefab;

    // �v���C���[���Փ˂����� true
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Player���߂��ɂ���");
        }
    }

    // �v���C���[�����ꂽ�� false
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("Player�����ꂽ");
        }
    }

    private void Update()
    {
        // プレイヤーが近くにいて、シフトキーが押されたら破壊
        if (playerNearby && Input.GetKeyDown(KeyCode.LeftShift))
        {
            // アイテムを生成（ブロックの位置に出現）
            if (itemPrefab != null)
            {
                Instantiate(itemPrefab, transform.position, Quaternion.identity);
                Debug.Log("アイテムを出現させた");
            }
            
            Destroy(gameObject);
            Debug.Log("Blockを破壊した");
        }
    }
}