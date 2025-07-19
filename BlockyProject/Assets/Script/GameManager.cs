using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public bool gamePaused = false;
    
    [Header("Player Inventory (Simple)")]
    [SerializeField] private List<string> playerInventory = new List<string>();
    [SerializeField] private Dictionary<string, int> itemQuantities = new Dictionary<string, int>();
    
    [Header("Game Flags")]
    [SerializeField] private List<string> gameFlags = new List<string>();
    
    public static GameManager Instance { get; private set; }
    
    // Events
    public System.Action<string, int> OnItemAdded;
    public System.Action<string> OnFlagSet;
    public System.Action<bool> OnGamePaused;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGameManager();
    }

    private void InitializeGameManager()
    {
        // Initialize item quantities dictionary
        itemQuantities = new Dictionary<string, int>();
        
        Debug.Log("GameManager initialized");
    }

    #region Inventory System
    
    public bool PlayerHasItem(string itemName)
    {
        return playerInventory.Contains(itemName);
    }

    public int GetItemQuantity(string itemName)
    {
        return itemQuantities.ContainsKey(itemName) ? itemQuantities[itemName] : 0;
    }

    public void GiveItemToPlayer(string itemName, int quantity = 1)
    {
        if (!playerInventory.Contains(itemName))
        {
            playerInventory.Add(itemName);
            itemQuantities[itemName] = quantity;
        }
        else
        {
            itemQuantities[itemName] += quantity;
        }
        
        OnItemAdded?.Invoke(itemName, quantity);
        Debug.Log($"Added {quantity}x {itemName} to player inventory");
    }

    public bool RemoveItemFromPlayer(string itemName, int quantity = 1)
    {
        if (!PlayerHasItem(itemName)) return false;
        
        int currentQuantity = GetItemQuantity(itemName);
        if (currentQuantity < quantity) return false;
        
        itemQuantities[itemName] -= quantity;
        
        if (itemQuantities[itemName] <= 0)
        {
            playerInventory.Remove(itemName);
            itemQuantities.Remove(itemName);
        }
        
        Debug.Log($"Removed {quantity}x {itemName} from player inventory");
        return true;
    }

    #endregion

    #region Game Flags System
    
    public bool HasFlag(string flagName)
    {
        return gameFlags.Contains(flagName);
    }

    public void SetFlag(string flagName)
    {
        if (!gameFlags.Contains(flagName))
        {
            gameFlags.Add(flagName);
            OnFlagSet?.Invoke(flagName);
            Debug.Log($"Game flag '{flagName}' set");
        }
    }

    public void RemoveFlag(string flagName)
    {
        if (gameFlags.Contains(flagName))
        {
            gameFlags.Remove(flagName);
            Debug.Log($"Game flag '{flagName}' removed");
        }
    }

    #endregion

    #region Game Control
    
    public void PauseGame(bool pause)
    {
        gamePaused = pause;
        Time.timeScale = pause ? 0f : 1f;
        OnGamePaused?.Invoke(pause);
    }

    public void SetPlayerMovementEnabled(bool enabled)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var playerMovement = player.GetComponent<playerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = enabled;
            }
        }
    }

    #endregion
    #region Debug Methods
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugInventory()
    {
        Debug.Log("=== PLAYER INVENTORY ===");
        if (playerInventory.Count == 0)
        {
            Debug.Log("Inventory is empty");
            return;
        }
        
        foreach (string item in playerInventory)
        {
            int quantity = GetItemQuantity(item);
            Debug.Log($"{item}: {quantity}");
        }
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugFlags()
    {
        Debug.Log("=== GAME FLAGS ===");
        if (gameFlags.Count == 0)
        {
            Debug.Log("No flags set");
            return;
        }
        
        foreach (string flag in gameFlags)
        {
            Debug.Log($"Flag: {flag}");
        }
    }

    #endregion
}
