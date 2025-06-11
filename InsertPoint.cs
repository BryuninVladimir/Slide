using UnityEngine;

public class InsertPoint : MonoBehaviour {
    public Direction direction;
    public int index;

    private Color defaultColor;
    public Color hoverColor = Color.yellow;
    public Color disabledColor = Color.gray; // ����� ���� ��� ������������ ���������

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
        // ������������� �� ������� ����� ����, ����� ��������� ��������� InsertPoint
        TurnManager.Instance.OnTurnChanged += OnTurnChanged;
        // ������������� ��������� ���������
        UpdatePointState(TurnManager.Instance.currentPlayer);
    }

    void OnDestroy()
    {
        // ������������, ����� �������� ������ ������
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnTurnChanged -= OnTurnChanged;
        }
    }

    void OnMouseEnter()
    {
        if (CanInsert()) // ������ ���� ����� ��������
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
        if (CanInsert()) // ������ ���� ����� ��������
        {
            GameManager.Instance.PerformLamaInsert(direction, index);
            // ��������� ���������� ���� ����� �����, ��� ��� ��� ����� ���������
            if (pointRenderer != null)
            {
                pointRenderer.material.color = defaultColor;
            }
        }
    }

    // ����� ����� ��� ��������, ����� �� �������� ����
    private bool CanInsert()
    {
        // ���������, ��������� �� ��� �����������
        if (direction == Direction.Down)
        {
            return false;
        }

        // ���������, �� ���� �� ��� ��������� ����
        // GameManager.Instance.isProcessingTurn (����� ��������� ������ ��� ����� �����)
        // ��� ����� ����� ������� isProcessingTurn ��������� ��������� � GameManager
        // public bool IsProcessingTurn => isProcessingTurn;
        // ��� �������� ���� ����� TurnManager.Instance.CanMakeMove (�����)

        // ��� �������� ���� ���������� ������ ����� � GameManager.Instance.isProcessingTurn.
        // ���� ���� isProcessingTurn ���������, �� ����� �������� ��������� �������� � GameManager:
        // public bool IsProcessingTurn { get { return isProcessingTurn; } }
        if (GameManager.Instance.IsProcessingTurn) // ������������, ��� IsProcessingTurn ������ ��������� ������
        {
            return false;
        }

        // ���� ��� �������� ��������, �� ����� ��������
        return true;
    }

    // �����, ������� ����� ���������� ��� ����� ����
    private void OnTurnChanged(PlayerType player)
    {
        UpdatePointState(player);
    }

    // ��������� ���������� ��������� InsertPoint (��������/����������)
    private void UpdatePointState(PlayerType player)
    {
        // ������ ���������/���������� ��� ������
        // ���� ������ �� �����, �� ����� �������� ����� ������� ������
        if (CanInsert()) // ���� ����� ������� ��������� � ���� �� � �������� ����
        {
            if (pointRenderer != null)
            {
                pointRenderer.material.color = defaultColor; // ��� �����-�� ����, ��������������� ����������
            }
        }
        else // ���� ������� ��������� (��-�� ����������� ��� ��������� ����)
        {
            if (pointRenderer != null)
            {
                pointRenderer.material.color = disabledColor;
            }
        }
    }
}