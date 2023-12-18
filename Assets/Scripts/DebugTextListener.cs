using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugTextListener : MonoBehaviour
{
    
    private string msgString = "hello world";
    void OnEnable() {
        Application.logMessageReceived += LogMessage;
    }

    void OnDisable() {
        Application.logMessageReceived -= LogMessage;
    }

    public void LogMessage(string message, string stackTrace, LogType type) {
        GetComponent<TMP_Text>().SetText(message);
    }
}
