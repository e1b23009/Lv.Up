using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // �x�X�g�X�R�A���擾���郁�\�b�h
    public int GetBestScore()
    {
        return PlayerPrefs.GetInt("BestScore", 0);  // �ۑ����ꂽ�x�X�g�X�R�A��Ԃ��i�f�t�H���g�� 0�j
    }

    // �x�X�g�X�R�A��ݒ肷�郁�\�b�h
    public void SetBestScore(int score)
    {
        int currentBestScore = GetBestScore();
        if (score > currentBestScore)
        {
            PlayerPrefs.SetInt("BestScore", score);
            PlayerPrefs.Save();
        }
    }
}