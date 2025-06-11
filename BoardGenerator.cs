using UnityEngine;

public class BoardGenerator : MonoBehaviour {
    public GameObject cellPrefab; // ���� �� ��������� ��� ������ Cell
    public int boardSize = 5;     // ������ ���� (5x5)
    public float cellSize = 1.1f; // ���������� ����� �������� �����

    void Awake() // Awake ����������, ����� ������ �����������, �� Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        // �������� ������ Board, ������� ����� ��������� ��� ���� �����
        // ���� ������ ���������� � Board, �� this.transform - ��� Board
        Transform boardTransform = this.transform;

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                // ��������� ������� ��� ������� ������
                // ���������� ���� ������������ (0,0,0)
                Vector3 position = new Vector3(
                    x * cellSize - (boardSize - 1) * cellSize / 2f,
                    0, // ���� ������ �������, ������� Y=0
                    y * cellSize - (boardSize - 1) * cellSize / 2f
                );

                // ������� (�������������) ������ ������
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);

                // ������ ������ Board ��������� ��� ����� ������
                cell.transform.SetParent(boardTransform);

                // ����� ���� ��� ������ ��� �������� � Hierarchy
                cell.name = $"Cell ({x},{y})";
            }
        }
    }
}