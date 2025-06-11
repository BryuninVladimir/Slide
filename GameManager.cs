using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using System.Threading.Tasks;
using System.Linq;

// GameManager.cs
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    public GameObject cellPrefab;
    public GameObject[] lamaPrefabs; // ������ �������� ���
    public int boardSize = 5;
    public float cellSize = 1.1f;
    public float lamaOffset = 0.5f;
    public GameObject insertPointPrefab;
    public float insertPointOffset = 0.7f;
    public GameObject currentLamaDisplayPrefab; // ���� �� ������������, �� ������� �� �������
    private GameObject currentLamaObject;
    private LamaType currentLamaType; // ������� ����, ������� ����� ��������� ������� �����
    public float lamaMoveDuration = 0.2f;

    public Vector3 displayLamaPosition = new Vector3(6f, 0.5f, 0f);
    public float pushedOutLamaFlyDistance = 3f;

    [Header("Visual Effects")]
    public GameObject lamaPopFXPrefab; // ������ ��� �������� ���
    public GameObject floatingScoreTextPrefab; // ������ ��� ����������� �����

    [Header("Audio Effects")]
    public AudioClip lamaInsertSound; // ���� ������� ����

    private AudioSource audioSource;

    private LamaType[,] board; // ������ ��� �������� ����� ���
    private GameObject[,] lamaObjects; // ������ ��� �������� �������� ���

    private BoardMatcher boardMatcher;

    public bool IsProcessingTurn { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        board = new LamaType[boardSize, boardSize];
        lamaObjects = new GameObject[boardSize, boardSize];

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0;
        }

        boardMatcher = GetComponent<BoardMatcher>();
        if (boardMatcher == null)
        {
            boardMatcher = gameObject.AddComponent<BoardMatcher>();
        }
        BoardData data = new BoardData(board, lamaObjects, boardSize, lamaMoveDuration, cellSize, lamaOffset);
        boardMatcher.Initialize(data);

        boardMatcher.lamaPopFXPrefab = lamaPopFXPrefab;
        // ����� ��� BoardMatcher ������ ���� ��������� ����� ��������� BoardMatcher!
        // boardMatcher.lamaPopSound = ...;
        // boardMatcher.lamaFallSound = ...;

        boardMatcher.OnLamasMatched += HandleLamasMatched;
        boardMatcher.OnBoardStable += HandleBoardStable;

        GenerateInitialBoard();
        GenerateNewCurrentLama();
    }

    void OnDestroy()
    {
        if (boardMatcher != null)
        {
            boardMatcher.OnLamasMatched -= HandleLamasMatched;
            boardMatcher.OnBoardStable -= HandleBoardStable;
        }
    }

    // ��� �������: ������������ ����� � ���
    void OnDrawGizmos()
    {
        if (board != null && boardSize > 0 && cellSize > 0)
        {
            float startOffset = (boardSize - 1) * cellSize / 2f;
            for (int x = 0; x < boardSize; x++)
            {
                for (int y = 0; y < boardSize; y++)
                {
                    Vector3 worldPos = GetWorldPosition(x, y);
                    Gizmos.color = (board[x, y] == LamaType.None) ? Color.red : Color.green; // ������� - ������, ������� - ������
                    Gizmos.DrawWireSphere(worldPos, 0.2f); // ������ ����� ��� ������ ������

                    // ���� ���� ������ ����, �������� ��� ��� ��� �������
                    if (lamaObjects[x, y] != null)
                    {
                        Gizmos.color = Color.blue; // ����� - ������� ������� ����
                        Gizmos.DrawWireCube(lamaObjects[x, y].transform.position, Vector3.one * 0.5f);
                    }
                }
            }
        }
    }

    void GenerateInitialBoard()
    {
        Transform boardParent = new GameObject("Board").transform;
        boardParent.position = Vector3.zero;

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                Vector3 cellPosition = new Vector3(
                    x * cellSize - (boardSize - 1) * cellSize / 2f,
                    0,
                    y * cellSize - (boardSize - 1) * cellSize / 2f
                );
                GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                cell.transform.SetParent(boardParent);
                cell.name = $"Cell ({x},{y})";

                GenerateRandomLama(x, y, boardParent);
            }
        }

        GenerateInsertPoints(boardParent);
    }

    private GameObject GetLamaPrefab(LamaType type)
    {
        foreach (GameObject prefab in lamaPrefabs)
        {
            Lama lamaComponent = prefab.GetComponent<Lama>();
            if (lamaComponent != null && lamaComponent.type == type)
            {
                return prefab;
            }
        }
        return null;
    }

    void GenerateRandomLama(int x, int y, Transform parentTransform)
    {
        LamaType randomType = (LamaType)Random.Range(1, System.Enum.GetValues(typeof(LamaType)).Length);

        GameObject lamaPrefabToInstantiate = GetLamaPrefab(randomType);

        if (lamaPrefabToInstantiate == null)
        {
            Debug.LogError("Lama Prefab not found for type: " + randomType);
            return;
        }

        Vector3 lamaPosition = new Vector3(
            x * cellSize - (boardSize - 1) * cellSize / 2f,
            lamaOffset,
            y * cellSize - (boardSize - 1) * cellSize / 2f
        );

        GameObject newLama = Instantiate(lamaPrefabToInstantiate, lamaPosition, Quaternion.identity);
        newLama.transform.SetParent(parentTransform);

        board[x, y] = randomType;
        lamaObjects[x, y] = newLama;

        Lama lamaComponent = newLama.GetComponent<Lama>();
        if (lamaComponent != null)
        {
            lamaComponent.type = randomType;
            lamaComponent.boardPosition = new Vector2Int(x, y);
        }
        else
        {
            Debug.LogError($"Lama component missing on prefab {lamaPrefabToInstantiate.name}");
        }
    }

    public LamaType GetLamaType(GameObject lamaObject)
    {
        Lama lamaComponent = lamaObject.GetComponent<Lama>();
        if (lamaComponent != null)
        {
            return lamaComponent.type;
        }
        return LamaType.None;
    }

    public GameObject GetLamaObject(int x, int y)
    {
        if (x >= 0 && x < boardSize && y >= 0 && y < boardSize)
        {
            return lamaObjects[x, y];
        }
        return null;
    }

    public void SetLama(int x, int y, LamaType type, GameObject lamaObject)
    {
        if (x >= 0 && x < boardSize && y >= 0 && y < boardSize) // ����� �������� �� �������
        {
            board[x, y] = type;
            lamaObjects[x, y] = lamaObject;
            if (lamaObject != null)
            {
                Lama lamaComponent = lamaObject.GetComponent<Lama>();
                if (lamaComponent != null)
                {
                    lamaComponent.type = type;
                    lamaComponent.boardPosition = new Vector2Int(x, y); // ��������� boardPosition
                }
                else
                {
                    Debug.LogError($"Lama component missing on object being set at ({x},{y})!");
                }
            }
        }
        else
        {
            Debug.LogError($"Attempted to set lama at out-of-bounds position: ({x},{y})! Board Size: {boardSize}"); // ������ ���� ���
        }
    }


    void GenerateInsertPoints(Transform parentTransform)
    {
        float startOffset = (boardSize - 1) * cellSize / 2f;

        for (int i = 0; i < boardSize; i++)
        {
            // Top (������)
            Vector3 topPos = new Vector3(
                i * cellSize - startOffset,
                lamaOffset,
                startOffset + insertPointOffset
            );
            GameObject topPoint = Instantiate(insertPointPrefab, topPos, Quaternion.identity);
            topPoint.transform.SetParent(parentTransform);
            topPoint.name = $"InsertPoint_Top_{i}";
            InsertPoint topInsert = topPoint.GetComponent<InsertPoint>();
            if (topInsert != null) topInsert.direction = Direction.Up;
            topInsert.index = i;

            // Left (�����)
            Vector3 leftPos = new Vector3(
                -startOffset - insertPointOffset,
                lamaOffset,
                i * cellSize - startOffset
            );
            GameObject leftPoint = Instantiate(insertPointPrefab, leftPos, Quaternion.identity);
            leftPoint.transform.SetParent(parentTransform);
            leftPoint.name = $"InsertPoint_Left_{i}";
            InsertPoint leftInsert = leftPoint.GetComponent<InsertPoint>();
            if (leftInsert != null) leftInsert.direction = Direction.Left;
            leftInsert.index = i;

            // Right (������)
            Vector3 rightPos = new Vector3(
                startOffset + insertPointOffset,
                lamaOffset,
                i * cellSize - startOffset
            );
            GameObject rightPoint = Instantiate(insertPointPrefab, rightPos, Quaternion.identity);
            rightPoint.transform.SetParent(parentTransform);
            rightPoint.name = $"InsertPoint_Right_{i}";
            InsertPoint rightInsert = rightPoint.GetComponent<InsertPoint>();
            if (rightInsert != null) rightInsert.direction = Direction.Right;
            rightInsert.index = i;
        }
    }

    void GenerateNewCurrentLama()
    {
        currentLamaType = (LamaType)Random.Range(1, System.Enum.GetValues(typeof(LamaType)).Length);

        if (currentLamaObject != null)
        {
            Destroy(currentLamaObject);
        }

        GameObject lamaPrefabToInstantiate = GetLamaPrefab(currentLamaType);

        if (lamaPrefabToInstantiate == null)
        {
            Debug.LogError("Lama Prefab not found for current type: " + currentLamaType);
            return;
        }

        currentLamaObject = Instantiate(lamaPrefabToInstantiate, displayLamaPosition, Quaternion.identity);
        currentLamaObject.name = "CurrentPlayerLama";

        if (currentLamaObject.GetComponent<Collider>() != null)
        {
            currentLamaObject.GetComponent<Collider>().enabled = false;
        }
    }

    public async void PerformLamaInsert(Direction direction, int index)
    {
        if (IsProcessingTurn)
        {
            return;
        }

        if (direction == Direction.Down)
        {
            Debug.LogWarning("Inserting from Bottom is not allowed in this game mode.");
            return;
        }

        IsProcessingTurn = true;
        Debug.Log($"PerformLamaInsert: Direction {direction}, Index {index}. IsProcessingTurn = {IsProcessingTurn}");


        LamaType pushedOutLamaType = LamaType.None;
        GameObject pushedOutLamaObject = null;

        List<Tween> insertMoveTweens = new List<Tween>();
        GameObject newLamaGameObject = null;

        Vector2Int insertBoardPos = Vector2Int.zero;
        Vector3 insertWorldPos = Vector3.zero;
        Vector3 startSpawnPos = Vector3.zero;

        // ���������� ��������� ������� ������ ����� ����
        switch (direction)
        {
            case Direction.Up:
                // ������� ���� ���� �����
                startSpawnPos = GetWorldPosition(index, boardSize - 1) + Vector3.forward * insertPointOffset;
                break;
            case Direction.Left:
                // ������� ���� ����� �����
                startSpawnPos = GetWorldPosition(0, index) + Vector3.left * insertPointOffset;
                break;
            case Direction.Right:
                // ������� ���� ������ �����
                startSpawnPos = GetWorldPosition(boardSize - 1, index) + Vector3.right * insertPointOffset;
                break;
        }

        GameObject lamaPrefabToInstantiate = GetLamaPrefab(currentLamaType);
        if (lamaPrefabToInstantiate == null)
        {
            Debug.LogError("Lama Prefab not found for new lama type: " + currentLamaType);
            IsProcessingTurn = false;
            return;
        }
        newLamaGameObject = Instantiate(lamaPrefabToInstantiate, startSpawnPos, Quaternion.identity);
        newLamaGameObject.transform.SetParent(GameObject.Find("Board").transform);
        Lama newLamaComponent = newLamaGameObject.GetComponent<Lama>();
        if (newLamaComponent == null)
        {
            Debug.LogError("New Lama object is missing Lama component!");
            Destroy(newLamaGameObject);
            IsProcessingTurn = false;
            return;
        }
        newLamaComponent.type = currentLamaType;


        bool isFull = IsRowOrColumnFull(direction, index);
        Debug.Log($"Is row/column full: {isFull}");

        // ������ ��� ������� ����/������� (������������ ����)
        if (isFull)
        {
            switch (direction)
            {
                case Direction.Up:
                    pushedOutLamaObject = lamaObjects[index, 0];
                    pushedOutLamaType = board[index, 0];

                    for (int y = 0; y < boardSize - 1; y++)
                    {
                        board[index, y] = board[index, y + 1];
                        lamaObjects[index, y] = lamaObjects[index, y + 1];
                        if (lamaObjects[index, y] != null)
                        {
                            lamaObjects[index, y].GetComponent<Lama>().boardPosition = new Vector2Int(index, y);
                            insertMoveTweens.Add(lamaObjects[index, y].transform.DOMove(GetWorldPosition(index, y), lamaMoveDuration));
                        }
                    }
                    insertBoardPos = new Vector2Int(index, boardSize - 1);
                    break;

                case Direction.Left:
                    pushedOutLamaObject = lamaObjects[boardSize - 1, index];
                    pushedOutLamaType = board[boardSize - 1, index];

                    for (int x = boardSize - 1; x > 0; x--)
                    {
                        board[x, index] = board[x - 1, index];
                        lamaObjects[x, index] = lamaObjects[x - 1, index];
                        if (lamaObjects[x, index] != null)
                        {
                            lamaObjects[x, index].GetComponent<Lama>().boardPosition = new Vector2Int(x, index);
                            insertMoveTweens.Add(lamaObjects[x, index].transform.DOMove(GetWorldPosition(x, index), lamaMoveDuration));
                        }
                    }
                    insertBoardPos = new Vector2Int(0, index);
                    break;

                case Direction.Right:
                    pushedOutLamaObject = lamaObjects[0, index];
                    pushedOutLamaType = board[0, index];

                    for (int x = 0; x < boardSize - 1; x++)
                    {
                        board[x, index] = board[x + 1, index];
                        lamaObjects[x, index] = lamaObjects[x + 1, index];
                        if (lamaObjects[x, index] != null)
                        {
                            lamaObjects[x, index].GetComponent<Lama>().boardPosition = new Vector2Int(x, index);
                            insertMoveTweens.Add(lamaObjects[x, index].transform.DOMove(GetWorldPosition(x, index), lamaMoveDuration));
                        }
                    }
                    insertBoardPos = new Vector2Int(boardSize - 1, index);
                    break;
            }

            if (pushedOutLamaObject != null && pushedOutLamaType != LamaType.None)
            {
                Vector3 flyDirection = Vector3.zero;
                switch (direction)
                {
                    case Direction.Up: flyDirection = Vector3.back; break;
                    case Direction.Left: flyDirection = Vector3.right; break;
                    case Direction.Right: flyDirection = Vector3.left; break;
                }

                Vector3 targetFlyPosition = pushedOutLamaObject.transform.position + flyDirection * pushedOutLamaFlyDistance;

                Sequence sequence = DOTween.Sequence();
                sequence.Append(pushedOutLamaObject.transform.DOMove(targetFlyPosition, lamaMoveDuration * 1.5f).SetEase(Ease.OutQuad));
                sequence.Join(pushedOutLamaObject.transform.DORotate(new Vector3(0, 720, 0), lamaMoveDuration * 1.5f, RotateMode.FastBeyond360));
                sequence.Join(pushedOutLamaObject.transform.DOScale(Vector3.zero, lamaMoveDuration * 1.5f).SetEase(Ease.InQuad));
                sequence.OnComplete(() => {
                    Debug.Log($"Pushed out lama at ({pushedOutLamaObject.transform.position}) destroyed.");
                    Destroy(pushedOutLamaObject);
                });
                insertMoveTweens.Add(sequence);
            }
        }
        else // ���� ���� ������ ������ (���/������� �� �����)
        {
            int targetCoord = -1; // ����������, ���� � ����� ����������� ����� ����

            switch (direction)
            {
                case Direction.Up: // ��������� ������, ���� �������� ����
                    // ���� ������ ������ ������ ����� ����� (���� ������ ����� ����)
                    for (int y = 0; y < boardSize; y++)
                    {
                        if (board[index, y] == LamaType.None)
                        {
                            targetCoord = y;
                            break;
                        }
                    }
                    // �������� ��� ���� ���� ��������� ������ ������ �� ���� ������� ����
                    for (int y = boardSize - 1; y > targetCoord; y--)
                    {
                        board[index, y] = board[index, y - 1];
                        lamaObjects[index, y] = lamaObjects[index, y - 1];
                        if (lamaObjects[index, y] != null)
                        {
                            lamaObjects[index, y].GetComponent<Lama>().boardPosition = new Vector2Int(index, y);
                            insertMoveTweens.Add(lamaObjects[index, y].transform.DOMove(GetWorldPosition(index, y), lamaMoveDuration));
                        }
                    }
                    insertBoardPos = new Vector2Int(index, targetCoord); // ����� ���� ������ �� ��������� ������ �������
                    break;

                case Direction.Left: // ��������� �����, ���� �������� ������
                    // ���� ������ ������ ������ ����� ������� (���� ������ ����� ����)
                    for (int x = 0; x < boardSize; x++)
                    {
                        if (board[x, index] == LamaType.None)
                        {
                            targetCoord = x;
                            break;
                        }
                    }
                    // �������� ���� �� (targetCoord - 1) �� 0 �� ���� ������� ������
                    // ������: [A][B][ ][C][D], targetCoord = 2.
                    // �������� C -> D, B -> C, A -> B. �������� [ ][A][B][C][D]
                    for (int x = targetCoord; x > 0; x--)
                    {
                        board[x, index] = board[x - 1, index];
                        lamaObjects[x, index] = lamaObjects[x - 1, index];
                        if (lamaObjects[x, index] != null)
                        {
                            lamaObjects[x, index].GetComponent<Lama>().boardPosition = new Vector2Int(x, index);
                            insertMoveTweens.Add(lamaObjects[x, index].transform.DOMove(GetWorldPosition(x, index), lamaMoveDuration));
                        }
                    }
                    insertBoardPos = new Vector2Int(0, index); // ����� ���� ������ �� ����� ����� �������
                    break;

                case Direction.Right: // ��������� ������, ���� �������� �����
                    // ���� ������ ������ ������ ������ ������ (���� ������ ����� ����)
                    for (int x = boardSize - 1; x >= 0; x--)
                    {
                        if (board[x, index] == LamaType.None)
                        {
                            targetCoord = x;
                            break;
                        }
                    }
                    // �������� ���� �� (targetCoord + 1) �� (boardSize - 1) �� ���� ������� �����
                    // ������: [A][B][ ][C][D], targetCoord = 2.
                    // �������� B -> A, C -> B, D -> C. �������� [A][B][C][D][ ]
                    for (int x = targetCoord; x < boardSize - 1; x++)
                    {
                        board[x, index] = board[x + 1, index];
                        lamaObjects[x, index] = lamaObjects[x + 1, index];
                        if (lamaObjects[x, index] != null)
                        {
                            lamaObjects[x, index].GetComponent<Lama>().boardPosition = new Vector2Int(x, index);
                            insertMoveTweens.Add(lamaObjects[x, index].transform.DOMove(GetWorldPosition(x, index), lamaMoveDuration));
                        }
                    }
                    insertBoardPos = new Vector2Int(boardSize - 1, index); // ����� ���� ������ �� ����� ������ �������
                    break;
            }

            if (targetCoord == -1)
            {
                Debug.LogError("Error: IsRowOrColumnFull reported false, but no empty spot found! This should not happen. Destroying new lama.");
                Destroy(newLamaGameObject);
                IsProcessingTurn = false;
                return;
            }
        }

        // ��������� ����� ���� � ������ board � lamaObjects
        SetLama(insertBoardPos.x, insertBoardPos.y, currentLamaType, newLamaGameObject);
        insertWorldPos = GetWorldPosition(insertBoardPos.x, insertBoardPos.y);

        // �������� ����� ����
        insertMoveTweens.Add(newLamaGameObject.transform.DOMove(insertWorldPos, lamaMoveDuration).SetEase(Ease.OutQuad)
            .OnComplete(() => Debug.Log($"New lama inserted at ({insertBoardPos.x},{insertBoardPos.y}) and animation complete."))
        );

        PlaySFX(lamaInsertSound);

        Debug.Log("--- Board State After Insert & Shift (before stability check) ---");
        for (int y = boardSize - 1; y >= 0; y--) // ������ ����
        {
            string rowTypes = "";
            string rowObjects = "";
            for (int x = 0; x < boardSize; x++)
            {
                rowTypes += $"({x},{y}):{board[x, y].ToString().PadRight(8)} | ";
                rowObjects += $"({x},{y}):{(lamaObjects[x, y] != null ? "OBJECT" : "NULL ")} | ";
            }
            Debug.Log($"Types: {rowTypes}");
            Debug.Log($"Objects: {rowObjects}");
        }
        Debug.Log("------------------------------------------------------------------");


        if (insertMoveTweens.Any())
        {
            await Task.WhenAll(insertMoveTweens.Select(t => t.AsyncWaitForCompletion()));
        }
        await Task.Delay(150); // ��������� �������� ����� �������� �������


        // ��������� ������� ������������ ����� (����� ���������� � �������)
        await boardMatcher.ProcessBoardStability(TurnManager.Instance.currentPlayer);
    }

    private bool IsRowOrColumnFull(Direction direction, int index)
    {
        switch (direction)
        {
            case Direction.Up:
            case Direction.Down:
                for (int y = 0; y < boardSize; y++)
                {
                    if (board[index, y] == LamaType.None)
                    {
                        return false;
                    }
                }
                return true;
            case Direction.Left:
            case Direction.Right:
                for (int x = 0; x < boardSize; x++)
                {
                    if (board[x, index] == LamaType.None)
                    {
                        return false;
                    }
                }
                return true;
            default:
                return false;
        }
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        float startOffset = (boardSize - 1) * cellSize / 2f;
        return new Vector3(
            x * cellSize - startOffset,
            lamaOffset,
            y * cellSize - startOffset
        );
    }

    private void HandleLamasMatched(PlayerType player, int scoreAmount, Vector3 averageMatchWorldPosition)
    {
        ScoreManager.Instance.AddScore(player, scoreAmount);

        if (floatingScoreTextPrefab != null)
        {
            Vector3 targetScoreWorldUIPosition = ScoreManager.Instance.GetPlayerScoreTextWorldPosition(player);

            Transform mainCanvasTransform = ScoreManager.Instance.player1ScoreText.transform.parent;
            if (mainCanvasTransform == null)
            {
                Debug.LogError("Main Canvas not found! Make sure ScoreManager.player1ScoreText is assigned.");
                return;
            }

            GameObject floatingTextGO = Instantiate(floatingScoreTextPrefab, mainCanvasTransform);
            FloatingText floatingText = floatingTextGO.GetComponent<FloatingText>();
            if (floatingText != null)
            {
                floatingText.Initialize(scoreAmount, averageMatchWorldPosition, targetScoreWorldUIPosition);
            }
            else
            {
                Debug.LogError("FloatingText component not found on floatingScoreTextPrefab!");
                Destroy(floatingTextGO);
            }
        }
        else
        {
            Debug.LogWarning("floatingScoreTextPrefab is not assigned in GameManager.");
        }
    }

    private void HandleBoardStable()
    {
        Debug.Log("Board is now stable.");
        CheckGameEndConditions();
        GenerateNewCurrentLama();
        // �����: TurnManager.Instance.NextTurn() ������ ���������� ������ ����� ����,
        // ��� ���������, ��� ���� �� ����������� � ����� ���������� ���.
        // ���� TurnManager.Instance.NextTurn() ���� �� ������, ������������� ��� �������� ���.
        // ��� ����� ���� ����� ������ ������� GenerateNewCurrentLama() � ���������� IsProcessingTurn = false.
        // TurnManager.Instance.NextTurn(); // �����������������, ����� TurnManager ����� �����
        IsProcessingTurn = false;
        Debug.Log("Ready for next turn.");
    }


    void CheckGameEndConditions()
    {
        // ... (����� ����������� �����) ...
        // Debug.Log("Checking Game End Conditions...");
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}