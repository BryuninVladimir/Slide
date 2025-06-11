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

    public bool CanInsert()
    {
        // Точка вставки разрешена, если GameManager не находится в процессе обработки хода.
        // TurnManager не проверяем напрямую здесь, так как InputManager
        // будет реагировать на клик по ЛЮБОЙ InsertPoint,
        // а GameManager уже в PerformLamaInsert решит, чей сейчас ход.
        // Однако, чтобы визуально отключить InsertPoint во время чужого хода (если надо),
        // или если это просто заблокировано общим состоянием игры, оставляем проверку на IsProcessingTurn.

        if (GameManager.Instance.IsProcessingTurn)
        {
            return false;
        }

        // Если игра находится в состоянии, когда можно делать ход,
        // и GameManager не обрабатывает предыдущий ход, то эта точка активна.
        return true;
    }

    // Метод, который будет вызываться при смене хода
    private void OnTurnChanged(PlayerType player)
    {
        UpdatePointState(player);
    }

    // Обновляет визуальное состояние InsertPoint (активный/неактивный)
    private void UpdatePointState(PlayerType player) // player - это просто текущий игрок, его ход
    {
        // Логика подсветки/отключения InsertPoint.
        // Теперь InsertPoint активен, если *любой* игрок может сделать ход, и GameManager не обрабатывает предыдущий ход.
        // Если вам нужно ВИЗУАЛЬНО показывать, что insert pointы доступны только для ТЕКУЩЕГО ИГРОКА,
        // то вам нужно привязать insert points к игрокам (как в предыдущем предложении).
        // Если все insert points активны для обоих игроков, то достаточно CanInsert().
        if (CanInsert()) // Если можно вставить (т.е. игра не обрабатывает ход)
        {
            if (pointRenderer != null)
            {
                pointRenderer.material.color = defaultColor; // Активный цвет
            }
        }
        else // Если вставка запрещена (игра обрабатывает ход)
        {
            if (pointRenderer != null)
            {
                pointRenderer.material.color = disabledColor; // Отключенный цвет
            }
        }
    }
}
