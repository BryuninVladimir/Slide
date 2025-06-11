using UnityEngine;

public class InsertPoint : MonoBehaviour {
    public Direction direction;
    public int index;

    private Color defaultColor;
    public Color hoverColor = Color.yellow;
    public Color disabledColor = Color.gray; // Новый цвет для отключенного состояния

    private Renderer pointRenderer;

    void Awake()
    {
        pointRenderer = GetComponent<Renderer>();
        if (pointRenderer != null)
        {
            defaultColor = pointRenderer.material.color;
        }
    }

    void Start()
    {
        // Подписываемся на событие смены хода, чтобы обновлять состояние InsertPoint
        TurnManager.Instance.OnTurnChanged += OnTurnChanged;
        // Устанавливаем начальное состояние
        UpdatePointState(TurnManager.Instance.currentPlayer);
    }

    void OnDestroy()
    {
        // Отписываемся, чтобы избежать утечек памяти
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged -= OnTurnChanged;
        }
    }

    void OnMouseEnter()
    {
        if (CanInsert()) // Только если можно вставить
        {
            if (pointRenderer != null)
            {
                pointRenderer.material.color = hoverColor;
            }
        }
    }

    void OnMouseExit()
    {
        if (pointRenderer != null)
        {
            pointRenderer.material.color = defaultColor;
        }
    }

    void OnMouseDown()
    {
        if (CanInsert()) // Только если можно вставить
        {
            GameManager.Instance.PerformLamaInsert(direction, index);
            // Визуально сбрасываем цвет после клика, так как ход будет обработан
            if (pointRenderer != null)
            {
                pointRenderer.material.color = defaultColor;
            }
        }
    }

    // Новый метод для проверки, можно ли вставить ламу
    private bool CanInsert()
    {
        // Проверяем, разрешено ли это направление
        if (direction == Direction.Down)
        {
            return false;
        }

        // Проверяем, не идет ли уже обработка хода
        // GameManager.Instance.isProcessingTurn (нужен публичный геттер для этого флага)
        // Для этого нужно сделать isProcessingTurn публичным свойством в GameManager
        // public bool IsProcessingTurn => isProcessingTurn;
        // Или передать флаг через TurnManager.Instance.CanMakeMove (лучше)

        // Для простоты пока используем прямой вызов к GameManager.Instance.isProcessingTurn.
        // Если флаг isProcessingTurn приватный, то нужно добавить публичное свойство в GameManager:
        // public bool IsProcessingTurn { get { return isProcessingTurn; } }
        if (GameManager.Instance.IsProcessingTurn) // Предполагаем, что IsProcessingTurn теперь публичный геттер
        {
            return false;
        }

        // Если все проверки пройдены, то можно вставить
        return true;
    }

    // Метод, который будет вызываться при смене хода
    private void OnTurnChanged(PlayerType player)
    {
        UpdatePointState(player);
    }

    // Обновляет визуальное состояние InsertPoint (активный/неактивный)
    private void UpdatePointState(PlayerType player)
    {
        // Логика подсветки/отключения для игрока
        // Пока просто по цвету, но можно добавить более сложную логику
        if (CanInsert()) // Если точка вставки разрешена и игра не в процессе хода
        {
            if (pointRenderer != null)
            {
                pointRenderer.material.color = defaultColor; // Или какой-то цвет, сигнализирующий готовность
            }
        }
        else // Если вставка запрещена (из-за направления или обработки хода)
        {
            if (pointRenderer != null)
            {
                pointRenderer.material.color = disabledColor;
            }
        }
    }
}