using UnityEngine;

public class Block : MonoBehaviour
{
    private bool playerNearby = false;

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
        // �v���C���[���߂��ɂ��āA�V�t�g�L�[�������ꂽ��j��
        if (playerNearby && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Destroy(gameObject);
            Debug.Log("Block��j�󂵂�");
        }
    }
}