using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinishLine : MonoBehaviour
{
    [Header("Win Panels")]
    public GameObject player1GoldPanel;   // Top screen gold medal
    public GameObject player1SilverPanel; // Top screen silver medal
    public GameObject player2GoldPanel;   // Bottom screen gold medal
    public GameObject player2SilverPanel; // Bottom screen silver medal

    private bool car1Finished = false;
    private bool car2Finished = false;
    private bool hasFirstPlace = false;

    void Start()
    {
        // Hide all panels at start
        HideAllPanels();
    }

    private void HideAllPanels()
    {
        if (player1GoldPanel) player1GoldPanel.SetActive(false);
        if (player1SilverPanel) player1SilverPanel.SetActive(false);
        if (player2GoldPanel) player2GoldPanel.SetActive(false);
        if (player2SilverPanel) player2SilverPanel.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"FinishLine: Something entered trigger - {other.gameObject.name}");

        // Check for car components
        var backupController = other.GetComponent<Backup>();
        var aiController = other.GetComponent<AICarController>();

        // If no components found on the collider, check the parent
        if (backupController == null && aiController == null && other.transform.parent != null)
        {
            backupController = other.transform.parent.GetComponent<Backup>();
            aiController = other.transform.parent.GetComponent<AICarController>();
        }

        // Process finish line crossing if we found a car
        if (backupController != null || aiController != null)
        {
            Debug.Log($"FinishLine: Car crossed line - {other.gameObject.name}");

            // Determine if it's player 1 or player 2/AI
            bool isPlayer1 = other.gameObject.name.Contains("Player1") || 
                             (other.transform.parent != null && other.transform.parent.name.Contains("Player1"));

            // Handle Player 1 finish
            if (isPlayer1 && !car1Finished)
            {
                Debug.Log("FinishLine: Player 1 finished!");
                car1Finished = true;
                if (!hasFirstPlace)
                {
                    // First place!
                    hasFirstPlace = true;
                    ShowWinPanel(true, true);  // Show gold for player 1
                    ShowWinPanel(false, false); // Show silver for player 2/AI
                }
                else
                {
                    // Second place
                    ShowWinPanel(true, false);  // Show silver for player 1
                }
            }
            // Handle Player 2/AI finish
            else if (!isPlayer1 && !car2Finished)
            {
                Debug.Log($"FinishLine: {(aiController != null ? "AI" : "Player 2")} finished!");
                car2Finished = true;
                if (!hasFirstPlace)
                {
                    // First place!
                    hasFirstPlace = true;
                    ShowWinPanel(false, true);  // Show gold for player 2/AI
                    ShowWinPanel(true, false);  // Show silver for player 1
                }
                else
                {
                    // Second place
                    ShowWinPanel(false, false);  // Show silver for player 2/AI
                }
            }

            // Log race completion when both cars finish
            if (car1Finished && car2Finished)
            {
                Debug.Log("Race Complete - Both cars finished!");
            }
        }
        else
        {
            Debug.Log($"FinishLine: Object {other.gameObject.name} is not a car");
        }
    }

    private void ShowWinPanel(bool isPlayer1, bool isGold)
    {
        if (isPlayer1)
        {
            if (isGold && player1GoldPanel) player1GoldPanel.SetActive(true);
            else if (!isGold && player1SilverPanel) player1SilverPanel.SetActive(true);
        }
        else
        {
            if (isGold && player2GoldPanel) player2GoldPanel.SetActive(true);
            else if (!isGold && player2SilverPanel) player2SilverPanel.SetActive(true);
        }
    }
}
