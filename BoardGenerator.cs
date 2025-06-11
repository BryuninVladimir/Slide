using UnityEngine;

public class BoardGenerator : MonoBehaviour {
    public GameObject cellPrefab; // Сюда мы перетащим наш префаб Cell
    public int boardSize = 5;     // Размер поля (5x5)
    public float cellSize = 1.1f; // Расстояние между центрами ячеек

    void Awake() // Awake вызывается, когда скрипт загружается, до Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        // Получаем объект Board, который будет родителем для всех ячеек
        // Если скрипт прикреплен к Board, то this.transform - это Board
        Transform boardTransform = this.transform;

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                // Вычисляем позицию для текущей ячейки
                // Центрируем поле относительно (0,0,0)
                Vector3 position = new Vector3(
                    x * cellSize - (boardSize - 1) * cellSize / 2f,
                    0, // Наша ячейка плоская, поэтому Y=0
                    y * cellSize - (boardSize - 1) * cellSize / 2f
                );

                // Создаем (инстанциируем) префаб ячейки
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity);

                // Делаем объект Board родителем для новой ячейки
                cell.transform.SetParent(boardTransform);

                // Можно дать имя ячейке для удобства в Hierarchy
                cell.name = $"Cell ({x},{y})";
            }
        }
    }
}