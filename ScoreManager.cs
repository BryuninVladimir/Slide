using UnityEngine;
using TMPro; // Для TextMeshProUGUI
using System.Collections.Generic;

// ScoreManager.cs
public class ScoreManager : MonoBehaviour {
    public static ScoreManager Instance { get; private set; }

    [Header("Player Scores")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    private Dictionary<PlayerType, int> playerScores;

    [Header("UI References")]
    public RectTransform floatingTextCanvasTransform; // <-- ДОБАВЬТЕ ЭТО ПУБЛИЧНОЕ ПОЛЕ

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        playerScores = new Dictionary<PlayerType, int>();
        playerScores.Add(PlayerType.Player1, 0);
        playerScores.Add(PlayerType.Player2, 0);

        UpdateScoreUI();
    }

    public void AddScore(PlayerType player, int amount)
    {
        if (playerScores.ContainsKey(player))
        {
            playerScores[player] += amount;
            UpdateScoreUI();
            Debug.Log($"Player {player} scored {amount}. Total: {playerScores[player]}");
        }
        else
        {
            Debug.LogWarning($"Player {player} not found in score dictionary.");
        }
    }

    private void UpdateScoreUI()
    {
        if (player1ScoreText != null)
        {
            player1ScoreText.text = $"Player 1: {playerScores[PlayerType.Player1]}";
        }
        if (player2ScoreText != null)
        {
            player2ScoreText.text = $"Player 2: {playerScores[PlayerType.Player2]}";
        }
    }

    // <-- ДОБАВЬТЕ ЭТОТ МЕТОД -->
    public RectTransform GetFloatingTextCanvasTransform()
    {
        if (floatingTextCanvasTransform == null)
        {
            Debug.LogError("floatingTextCanvasTransform не назначен в ScoreManager. Пожалуйста, назначьте ваш UI Canvas (RectTransform) в Inspector.");
            // Запасной вариант: Попытаться найти Canvas, хотя это не идеально
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas != null)
            {
                floatingTextCanvasTransform = mainCanvas.GetComponent<RectTransform>();
            }
        }
        return floatingTextCanvasTransform;
    }

    // Этот метод уже определен в вашем GameManager (неявно)
    // Убедитесь, что enum PlayerType также определен (например, в TurnManager.cs или в отдельном файле)
    public Vector3 GetPlayerScoreTextWorldPosition(PlayerType player)
    {
        TextMeshProUGUI targetText = null;
        if (player == PlayerType.Player1) targetText = player1ScoreText;
        else if (player == PlayerType.Player2) targetText = player2ScoreText;

        if (targetText != null)
        {
            // Для Screen Space - Overlay, transform.position уже находится в координатах UI/экрана
            return targetText.transform.position;
        }
        Debug.LogWarning($"Score Text UI для {player} не найден. Возвращаем центр экрана.");
        return new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
    }
}