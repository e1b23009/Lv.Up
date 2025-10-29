using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // ベストスコアを取得するメソッド
    public int GetBestScore()
    {
        return PlayerPrefs.GetInt("BestScore", 0);  // 保存されたベストスコアを返す（デフォルトは 0）
    }

    // ベストスコアを設定するメソッド
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