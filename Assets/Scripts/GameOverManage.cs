using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManage : MonoBehaviour
{

    public GameObject gameOverScr;
    public TMP_Text cointCollected;

    public CoinManager coinManager;
    public GameObject player;
    public Vector3 playerStartPosition = Vector3.zero;

    private int coinCount;

    void Start()
    {
        PlayerHealth.OnPlayerDied += GameOverScreen;
        gameOverScr.SetActive(false);
        playerStartPosition = player.transform.position;
    }

    void OnDestroy()
    {
        // Hủy đăng ký event khi destroy
        PlayerHealth.OnPlayerDied -= GameOverScreen;
    }

    void GameOverScreen()
    {
        coinCount = coinManager.GetCoinCount();
        gameOverScr.SetActive(true);
        cointCollected.text = "COIN COLLECT: " + coinCount;

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        gameOverScr.SetActive(false);
        // reset coin count
        // relocate Player 0 0 0
        // playerHealth back to fullhealth

        // Reset coin count
        if (coinManager != null)
        {
            coinManager.coinCount = 0;
            // Update coin display nếu có UI
            if (coinManager.coinText != null)
                coinManager.coinText.text = "Coins: 0";
        }
        // Relocate Player to (0,0,0) or start position
        if (player != null)
        {
            player.transform.position = playerStartPosition;

            // Reset player health
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }

            // Đảm bảo player active
            player.SetActive(true);
        }
        // Resume game
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        StartCoroutine(RestartLevelCoroutine());
    }

    private IEnumerator RestartLevelCoroutine()
    {
        // Hủy event trước
        PlayerHealth.OnPlayerDied -= GameOverScreen;

        // Chờ 1 frame để đảm bảo tất cả events được xử lý xong
        yield return null;

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
