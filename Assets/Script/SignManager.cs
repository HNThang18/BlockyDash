using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SignManager : MonoBehaviour
{
    public static SignManager instance;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayTime = 2.0f;  // Keeping this for backward compatibility
    
    private bool isDisplayingMessage = false;
    private Coroutine currentMessageCoroutine;
    
    // Track the sign that is currently active
    private Sign currentActiveSign = null;
    
    private void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Make sure the message panel is hidden at start
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Display a message on the UI panel
    /// </summary>
    /// <param name="message">The text to display</param>
    public void DisplayMessage(string message, Sign sourceSign = null)
    {
        // If a message is already displayed from another sign, close it first
        if (currentActiveSign != null && currentActiveSign != sourceSign)
        {
            HideMessage();
        }
        
        // Stop any current message display
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = null;
        }
        
        // If we're showing a new message or toggling the current one off
        if (!isDisplayingMessage || (isDisplayingMessage && sourceSign == currentActiveSign))
        {
            // Toggle message display
            if (isDisplayingMessage)
            {
                HideMessage();
            }
            else
            {
                ShowMessage(message);
                currentActiveSign = sourceSign;
            }
        }
    }
    
    /// <summary>
    /// Show the message panel
    /// </summary>
    private void ShowMessage(string message)
    {
        if (messagePanel != null && messageText != null)
        {
            messageText.text = message;
            messagePanel.SetActive(true);
            isDisplayingMessage = true;
        }
    }
    
    /// <summary>
    /// Hide the message panel immediately
    /// </summary>
    public void HideMessage()
    {
        if (currentMessageCoroutine != null)
        {
            StopCoroutine(currentMessageCoroutine);
            currentMessageCoroutine = null;
        }
        
        if (messagePanel != null)
        {
            messagePanel.SetActive(false);
        }
        
        isDisplayingMessage = false;
        currentActiveSign = null;
    }
    
    /// <summary>
    /// Check if a message is currently being displayed
    /// </summary>
    public bool IsDisplayingMessage()
    {
        return isDisplayingMessage;
    }
    
    /// <summary>
    /// Check if this specific sign's message is being displayed
    /// </summary>
    public bool IsSignActive(Sign sign)
    {
        return isDisplayingMessage && currentActiveSign == sign;
    }
}