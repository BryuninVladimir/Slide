using UnityEngine;
using System;
using System.Threading.Tasks;
using TMPro;

public class TurnManager : MonoBehaviour {
    public static TurnManager Instance { get; private set; }

    public PlayerType currentPlayer { get; private set; } = PlayerType.Player1;

    [Header("UI References")]
    public TextMeshProUGUI currentPlayerText;

    public event Action<PlayerType> OnTurnChanged;

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
    }

    void Start()
    {
        UpdateCurrentPlayerUI();
        // Debug.Log($"Game starts! Current player: {currentPlayer}"); // Удаляем
    }

    public void NextTurn()
    {
        currentPlayer = (currentPlayer == PlayerType.Player1) ? PlayerType.Player2 : PlayerType.Player1;
        UpdateCurrentPlayerUI();
        OnTurnChanged?.Invoke(currentPlayer);
        // Debug.Log($"Turn changed to: {currentPlayer}"); // Удаляем
    }

    private void UpdateCurrentPlayerUI()
    {
        if (currentPlayerText != null)
        {
            currentPlayerText.text = $"Current Player: {currentPlayer}";
        }
    }

    public void EndGame()
    {
         Debug.Log("Game Over!"); // Удаляем, т.к. GameManager.HandleGameOver() уже выведет сообщение
        //GameManager.Instance.HandleGameOver();
    }
}