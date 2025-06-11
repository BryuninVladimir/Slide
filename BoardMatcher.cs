using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening; // Убедитесь, что DOTween импортирован
using System.Linq;

// BoardData.cs (вспомогательный класс)
public class BoardData {
    public LamaType[,] board;
    public GameObject[,] lamaObjects;
    public int boardSize;
    public float lamaMoveDuration;
    public float cellSize;
    public float lamaOffset; // Для корректного позиционирования по Y

    public BoardData(LamaType[,] board, GameObject[,] lamaObjects, int boardSize, float lamaMoveDuration, float cellSize, float lamaOffset)
    {
        this.board = board;
        this.lamaObjects = lamaObjects;
        this.boardSize = boardSize;
        this.lamaMoveDuration = lamaMoveDuration;
        this.cellSize = cellSize;
        this.lamaOffset = lamaOffset;
    }

    // Вспомогательный метод для получения мировой позиции для координат доски
    public Vector3 GetWorldPosition(int x, int y)
    {
        float startOffset = (boardSize - 1) * cellSize / 2f;
        return new Vector3(
            x * cellSize - startOffset,
            lamaOffset,
            y * cellSize - startOffset
        );
    }
}

// BoardMatcher.cs
public class BoardMatcher : MonoBehaviour {
    // Публичные события, чтобы GameManager мог реагировать на изменения на доске
    public event System.Action<PlayerType, int, Vector3> OnLamasMatched; // PlayerType, scoreAmount, averageMatchWorldPosition
    public event System.Action OnBoardStable;

    [Header("Match Effects")]
    public GameObject lamaPopFXPrefab; // Префаб эффекта исчезновения ламы
    public AudioClip lamaPopSound; // Звук исчезновения ламы
    public AudioClip lamaFallSound; // Звук падения ламы

    private AudioSource audioSource;
    private BoardData _boardData; // Ссылка на данные доски

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
        }
    }

    // Инициализация BoardMatcher с данными доски
    public void Initialize(BoardData data)
    {
        _boardData = data;
    }

    // Процесс стабилизации доски: удаление совпадений и падение лам до тех пор, пока доска не станет стабильной
    public async Task ProcessBoardStability(PlayerType currentPlayer)
    {
        int maxIterations = 50;
        int currentIteration = 0;

        bool boardChangedInThisCycle;

        do
        {
            boardChangedInThisCycle = false;
            currentIteration++;
            Debug.Log($"--- Starting Board Stability Iteration {currentIteration} ---");

            // Шаг 1: Сначала заставляем все ламы упасть на свои места
            bool lamasFell = await FillEmptySpaces();
            if (lamasFell)
            {
                Debug.Log($"Lamas fell in iteration {currentIteration}.");
                boardChangedInThisCycle = true;
            }
            else
            {
                Debug.Log($"No lamas fell in iteration {currentIteration}.");
            }

            await Task.Delay(150); // Небольшая задержка, чтобы анимации падения завершились


            // Шаг 2: Теперь проверяем на совпадения, только когда все ламы "осели"
            HashSet<Vector2Int> matches = FindMatches();

            if (matches.Count > 0)
            {
                Debug.Log($"Matches found in iteration {currentIteration}. Count: {matches.Count}");
                await RemoveMatches(matches, currentPlayer); // Передаем текущего игрока
                boardChangedInThisCycle = true;
            }
            else
            {
                Debug.Log($"No matches found in iteration {currentIteration}.");
            }

            if (!boardChangedInThisCycle)
            {
                Debug.Log($"Board stable after {currentIteration} iterations.");
                break;
            }
            else
            {
                Debug.Log($"Board changed in iteration {currentIteration}. Repeating stability check.");
            }

        } while (currentIteration < maxIterations);

        if (currentIteration >= maxIterations)
        {
            Debug.LogWarning("Max iterations reached in ProcessBoardStability. Possible infinite loop or highly complex cascade.");
        }

        OnBoardStable?.Invoke(); // Уведомляем GameManager, что доска стабильна
        Debug.Log("--- Board Stability Process Finished ---");
    }

    // Поиск совпадений на доске (Match-3)
    private HashSet<Vector2Int> FindMatches()
    {
        HashSet<Vector2Int> matches = new HashSet<Vector2Int>();

        // Проверка по горизонтали
        for (int y = 0; y < _boardData.boardSize; y++)
        {
            for (int x = 0; x < _boardData.boardSize - 2; x++)
            {
                LamaType type1 = _boardData.board[x, y];
                LamaType type2 = _boardData.board[x + 1, y];
                LamaType type3 = _boardData.board[x + 2, y];

                if (type1 != LamaType.None && type1 == type2 && type2 == type3)
                {
                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x + 1, y));
                    matches.Add(new Vector2Int(x + 2, y));

                    // Расширяем совпадения, если есть больше 3 подряд
                    for (int i = x + 3; i < _boardData.boardSize; i++)
                    {
                        if (_boardData.board[i, y] == type1)
                        {
                            matches.Add(new Vector2Int(i, y));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        // Проверка по вертикали
        for (int x = 0; x < _boardData.boardSize; x++)
        {
            for (int y = 0; y < _boardData.boardSize - 2; y++)
            {
                LamaType type1 = _boardData.board[x, y];
                LamaType type2 = _boardData.board[x, y + 1];
                LamaType type3 = _boardData.board[x, y + 2];

                if (type1 != LamaType.None && type1 == type2 && type2 == type3)
                {
                    matches.Add(new Vector2Int(x, y));
                    matches.Add(new Vector2Int(x, y + 1));
                    matches.Add(new Vector2Int(x, y + 2));

                    // Расширяем совпадения, если есть больше 3 подряд
                    for (int i = y + 3; i < _boardData.boardSize; i++)
                    {
                        if (_boardData.board[x, i] == type1)
                        {
                            matches.Add(new Vector2Int(x, i));
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        return matches;
    }

    // Удаление найденных совпадений с анимацией и уведомлением очков
    private async Task RemoveMatches(HashSet<Vector2Int> matches, PlayerType currentPlayer)
    {
        List<Tween> removeAnimations = new List<Tween>();
        int scoreGainedThisTurn = 0;

        Vector3 averageMatchWorldPosition = Vector3.zero;
        int matchCount = 0;

        foreach (Vector2Int pos in matches)
        {
            GameObject lamaToRemove = _boardData.lamaObjects[pos.x, pos.y];
            if (lamaToRemove != null)
            {
                _boardData.board[pos.x, pos.y] = LamaType.None; // Удаляем из логической доски
                _boardData.lamaObjects[pos.x, pos.y] = null; // Удаляем ссылку на объект

                averageMatchWorldPosition += lamaToRemove.transform.position;
                matchCount++;

                if (lamaPopFXPrefab != null)
                {
                    GameObject popFX = Instantiate(lamaPopFXPrefab, lamaToRemove.transform.position, Quaternion.identity);
                    Destroy(popFX, 1f);
                }
                PlaySFX(lamaPopSound);

                scoreGainedThisTurn += 10; // Каждая лама дает 10 очков

                Tween anim = lamaToRemove.transform.DOScale(Vector3.zero, _boardData.lamaMoveDuration * 0.7f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() => {
                        if (lamaToRemove != null)
                        {
                            Destroy(lamaToRemove);
                        }
                        Debug.Log($"Lama at ({pos.x},{pos.y}) removed."); // Добавлено логирование
                    });

                removeAnimations.Add(anim);
            }
            else
            {
                Debug.LogWarning($"Attempted to remove null lama at ({pos.x},{pos.y}) in RemoveMatches. Setting board to None.");
                _boardData.board[pos.x, pos.y] = LamaType.None;
                _boardData.lamaObjects[pos.x, pos.y] = null;
            }
        }

        if (removeAnimations.Count > 0)
        {
            await Task.WhenAll(removeAnimations.Select(t => t.AsyncWaitForCompletion()));
        }
        await Task.Delay(100);

        if (scoreGainedThisTurn > 0)
        {
            OnLamasMatched?.Invoke(currentPlayer, scoreGainedThisTurn, averageMatchWorldPosition / matchCount);
        }
    }

    // Заполнение пустых мест путем падения лам
    private async Task<bool> FillEmptySpaces()
    {


        bool lamasFell = false;
        List<Tween> fallAnimations = new List<Tween>();

        for (int x = 0; x < _boardData.boardSize; x++)
        {
            for (int y = 0; y < _boardData.boardSize; y++)
            {
                if (_boardData.board[x, y] == LamaType.None)
                {
                    // Найдена пустая ячейка. Ищем ламу сверху, чтобы она упала
                    for (int i = y + 1; i < _boardData.boardSize; i++)
                    {
                        if (_boardData.board[x, i] != LamaType.None)
                        {
                            // Нашли ламу, которая может упасть
                            _boardData.board[x, y] = _boardData.board[x, i]; // Перемещаем тип в пустую ячейку
                            _boardData.lamaObjects[x, y] = _boardData.lamaObjects[x, i]; // Перемещаем объект
                            _boardData.board[x, i] = LamaType.None; // Освобождаем старую позицию
                            _boardData.lamaObjects[x, i] = null; // Удаляем ссылку со старой позиции

                            if (_boardData.lamaObjects[x, y] != null) // Проверка на null
                            {
                                Lama lamaComponent = _boardData.lamaObjects[x, y].GetComponent<Lama>();
                                if (lamaComponent != null)
                                {
                                    lamaComponent.boardPosition = new Vector2Int(x, y); // Обновляем boardPosition ламы
                                    fallAnimations.Add(lamaComponent.transform.DOMove(_boardData.GetWorldPosition(x, y), _boardData.lamaMoveDuration).SetEase(Ease.OutBounce)
                                        .OnComplete(() => Debug.Log($"Lama at ({x},{y}) fell. Its final boardPosition is {lamaComponent.boardPosition}. Actual World Pos: {lamaComponent.transform.position}"))
                                    );
                                    lamasFell = true;
                                }
                                else
                                {
                                    Debug.LogError($"Lama component not found on lama object at ({x},{i}) for fall!");
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"Null object found at ({x},{i}) when trying to fall it to ({x},{y}). Logical error?");
                            }
                            break; // Нашли ламу для этой пустой ячейки, переходим к следующей пустой ячейке
                        }
                    }
                }
            }
        }

        if (lamasFell)
        {
            PlaySFX(lamaFallSound);
            if (fallAnimations.Any())
            {
                await Task.WhenAll(fallAnimations.Select(t => t.AsyncWaitForCompletion()));
            }
            await Task.Delay(100); // Небольшая задержка после падения
        }
        return lamasFell;
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}