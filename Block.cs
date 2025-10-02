using UnityEngine;

public class Block : MonoBehaviour
{
    private bool playerNearby = false;

    // プレイヤーが衝突したら true
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNearby = true;
            Debug.Log("Playerが近くにいる");
        }
    }

    // プレイヤーが離れたら false
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("Playerが離れた");
        }
    }

    private void Update()
    {
        // プレイヤーが近くにいて、シフトキーが押されたら破壊
        if (playerNearby && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Destroy(gameObject);
            Debug.Log("Blockを破壊した");
        }
    }
}
