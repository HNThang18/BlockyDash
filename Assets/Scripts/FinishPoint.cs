using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPoint : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;
    private bool playerInRange = false;
    private bool waitingForProceed = false;
    private bool waitingForMainMenu = false;

    private void Update()
    {
        CheckPlayerProximity();

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int lastIndex = SceneManager.sceneCountInBuildSettings - 1;

        if (playerInRange && CoinManager.instance != null)
        {
            int currentCoins = CoinManager.instance.GetCoinCount();
            int totalCoins = CoinManager.instance.GetTotalCoins();
            int requiredCoins = Mathf.CeilToInt(totalCoins / 2f);

            // If on the last level and not already waiting for main menu
            if (currentIndex >= lastIndex && !waitingForMainMenu)
            {
                MessageManager.Instance.ShowMessage("You have finished the game!\nPress any key to get back to main menu.");
                SoundManager.Instance.PlaySound2D("Win");
                waitingForMainMenu = true;
                waitingForProceed = false;
            }
            // If not on the last level and not already waiting to proceed
            else if (!waitingForProceed && !waitingForMainMenu)
            {
                string message = $"You have collected: {currentCoins}/{totalCoins} coins\n";
                if (currentCoins >= requiredCoins)
                {
                    Time.timeScale = 0f;
                    message += "Press any key to proceed to next level!";
                    waitingForProceed = true;
                }
                else
                {
                    message += $"You need at least {requiredCoins} coins to proceed.";
                    waitingForProceed = false;
                }
                MessageManager.Instance.ShowMessage(message);
            }
        }
        else if (!playerInRange)
        {
            waitingForProceed = false;
            waitingForMainMenu = false;
            if (MessageManager.Instance != null && MessageManager.Instance.IsMessageShowing())
                MessageManager.Instance.HideMessage();
        }

        // Wait for any key/button press to proceed
        if (waitingForProceed && playerInRange && CoinManager.instance != null)
        {
            Time.timeScale = 1f;
            if (Input.anyKeyDown)
            {
                MessageManager.Instance.HideMessage();
                ProceedToNextLevelOrMenu();
                waitingForProceed = false;
            }
        }

        // Wait for any key to return to main menu after finishing last level
        if (waitingForMainMenu && MessageManager.Instance != null && MessageManager.Instance.IsMessageShowing())
        {
            Time.timeScale = 1f;
            if (Input.anyKeyDown)
            {
                MessageManager.Instance.HideMessage();
                LevelManager.Instance.LoadScene("Main Menu", "CrossFade");
                MusicManager.Instance.PlayMusic("MainMenu");
                waitingForMainMenu = false;
            }
        }
    }

    private void CheckPlayerProximity()
    {
        Collider2D[] interactionColliders = Physics2D.OverlapCircleAll(transform.position, interactionRadius, playerLayer);
        playerInRange = interactionColliders.Length > 0;
    }

    private void ProceedToNextLevelOrMenu()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int lastIndex = SceneManager.sceneCountInBuildSettings - 1;

        if (currentIndex >= lastIndex)
        {
            MessageManager.Instance.ShowMessage("You have finished the game!\nPress any key to get back to main menu.");
            SoundManager.Instance.PlaySound2D("Win");
            waitingForMainMenu = true;
        }
        else
        {
            UnlockLevel();
            LevelManager.Instance.LoadScene("level " + (currentIndex + 1).ToString(), "CrossFade");
            SoundManager.Instance.PlaySound2D("Next level");
            MusicManager.Instance.PlayMusic("InGame");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
    void UnlockLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex >= PlayerPrefs.GetInt("ReachedIndex"))
        {
            PlayerPrefs.SetInt("ReachedIndex", SceneManager.GetActiveScene().buildIndex + 1);
            PlayerPrefs.SetInt("UnlockedLevels", PlayerPrefs.GetInt("UnlockedLevels", 1) + 1);
            PlayerPrefs.Save();
        }
    }
}