using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishPoint : MonoBehaviour
{
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask playerLayer;
    private bool playerInRange = false;
    private bool waitingForProceed = false;

    private void Update()
    {
        CheckPlayerProximity();

        if (playerInRange && !waitingForProceed && SignManager.instance != null && CoinManager.instance != null)
        {
            int currentCoins = CoinManager.instance.GetCoinCount();
            int totalCoins = CoinManager.instance.GetTotalCoins();
            int requiredCoins = Mathf.CeilToInt(totalCoins / 2f);

            string message = $"You have collected: {currentCoins}/{totalCoins} coins\n";

            if (currentCoins >= requiredCoins)
            {
                message += "Press any key to proceed to next level!";
                waitingForProceed = true;
            }
            else
            {
                message += $"You need at least {requiredCoins} coins to proceed.";
                waitingForProceed = false;
            }
            SignManager.instance.DisplayMessage(message);
        }
        else if (!playerInRange)
        {
            waitingForProceed = false;
            if (SignManager.instance != null && SignManager.instance.IsDisplayingMessage())
                SignManager.instance.HideMessage();
        }

        // Wait for any key/button press to proceed
        if (waitingForProceed && playerInRange && SignManager.instance != null && CoinManager.instance != null)
        {
            if (Input.anyKeyDown)
            {
                ProceedToNextLevelOrMenu();
                waitingForProceed = false;
                SignManager.instance.HideMessage();
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
            LevelManager.Instance.LoadScene("Main Menu", "CrossFade");
            SoundManager.Instance.PlaySound2D("Win");
            MusicManager.Instance.PlayMusic("MainMenu");
        }
        else
        {
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
}