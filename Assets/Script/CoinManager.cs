using UnityEngine;
using UnityEngine.UI; // For UI elements
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance; // Singleton pattern for easy access
    
    public int coinCount = 0;
    public int totalCoins = 0;
    public TextMeshProUGUI coinText; // Changed to TextMeshProUGUI for TMP support
    
    void Awake()
    {
        // Implement singleton pattern
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        totalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;
        UpdateCoinText();
    }
    public int GetTotalCoins()
    {
        return totalCoins;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Increase coin count
    public void AddCoin(int amount = 1)
    {
        coinCount += amount;
        UpdateCoinText();
    }
    
    // Get current coin count
    public int GetCoinCount()
    {
        return coinCount;
    }
    
    // Update UI text if available
    void UpdateCoinText()
    {
        if (coinText != null)
            coinText.text = "Coins: " + coinCount.ToString() + " / " + totalCoins.ToString();
    }
}
