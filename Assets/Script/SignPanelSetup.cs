using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SignPanelSetup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;
    
    private void Awake()
    {
        // Get references if not set in the inspector
        if (messagePanel == null)
        {
            messagePanel = transform.Find("MessagePanel")?.gameObject;
        }
        
        if (messageText == null && messagePanel != null)
        {
            messageText = messagePanel.GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Get the SignManager and set it up
        SignManager signManager = GetComponent<SignManager>();
        if (signManager != null)
        {
            // Use reflection to set the private fields (since they're SerializeField)
            var panelField = signManager.GetType().GetField("messagePanel", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (panelField != null)
            {
                panelField.SetValue(signManager, messagePanel);
            }
            
            var textField = signManager.GetType().GetField("messageText", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (textField != null)
            {
                textField.SetValue(signManager, messageText);
            }
        }
        
        // Auto-destroy this script after setup
        Destroy(this);
    }
}
