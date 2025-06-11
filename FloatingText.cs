using UnityEngine;
using TMPro; // <-- УБЕДИТЕСЬ, ЧТО ЭТА СТРОКА ЕСТЬ!
using DG.Tweening; // <-- УБЕДИТЕСЬ, ЧТО ЭТА СТРОКА ЕСТЬ, ЕСЛИ ВЫ ИСПОЛЬЗУЕТЕ DOTween!

public class FloatingText : MonoBehaviour
{
    public float moveUpDistance = 50f; // Насколько текст поднимется вверх в единицах Canvas
    public float initialMoveDuration = 0.5f; // Длительность первого подъема
    public float flyToScoreDuration = 0.7f; // Длительность полета к счету
    public Vector3 initialWorldOffset = new Vector3(0, 0.5f, 0); // Начальное смещение от позиции ламы в мировых координатах
    public Ease easeType = Ease.OutQuad; // Тип анимации для перемещения

    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        if (textMesh == null || rectTransform == null)
        {
            Debug.LogError("FloatingText requires a TextMeshProUGUI and RectTransform component!");
            enabled = false;
        }
    }

    /// <summary>
    /// Инициализирует и запускает анимацию всплывающего текста.
    /// </summary>
    /// <param name="scoreAmount">Количество очков.</param>
    /// <param name="startWorldPosition">Мировая позиция, где появилась комбинация (например, центр группы лам).</param>
    /// <param name="targetScoreWorldUIPosition">Мировая позиция UI элемента со счетом игрока (получается из ScoreManager).</param>
    public void Initialize(int scoreAmount, Vector3 startWorldPosition, Vector3 targetScoreWorldUIPosition)
    {
        textMesh.text = $"+{scoreAmount}";
        textMesh.color = Color.white; // Установите начальный цвет (можно сделать зависимым от игрока)
        rectTransform.localScale = Vector3.one; // Убедимся, что масштаб правильный

        // 1. Устанавливаем начальную позицию текста в Canvas
        // Конвертируем мировую позицию в позицию Canvas
        Vector3 spawnWorldPos = startWorldPosition + initialWorldOffset;
        
        // Для Canvas в Screen Space - Camera, используем RectTransformUtility
        // Чтобы получить локальные координаты в RectTransform родителя (Canvas)
        RectTransform canvasRectTransform = transform.parent.GetComponent<RectTransform>();
        if (canvasRectTransform == null)
        {
            Debug.LogError("FloatingText's parent is not a RectTransform (Canvas). Cannot set position correctly.");
            Destroy(gameObject);
            return;
        }

        Vector2 localPoint;
        // Переводим мировую позицию в экранную, затем из экранной в локальную для Canvas
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            Camera.main.WorldToScreenPoint(spawnWorldPos),
            Camera.main,
            out localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
        else
        {
            Debug.LogError("Failed to convert world position to local Canvas position.");
            Destroy(gameObject);
            return;
        }


        // 2. Анимация: сначала немного вверх, затем к счету
        Sequence sequence = DOTween.Sequence();

        // 2.1. Полет вверх
        sequence.Append(rectTransform.DOLocalMoveY(rectTransform.localPosition.y + moveUpDistance, initialMoveDuration).SetEase(easeType));

        // 2.2. Короткая пауза
        sequence.AppendInterval(0.1f);

        // 2.3. Полет к счету игрока
        Vector2 targetLocalScorePoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            Camera.main.WorldToScreenPoint(targetScoreWorldUIPosition),
            Camera.main,
            out targetLocalScorePoint))
        {
            sequence.Append(rectTransform.DOLocalMove(targetLocalScorePoint, flyToScoreDuration).SetEase(easeType));
        }
        else
        {
            Debug.LogError("Failed to convert target score UI world position to local Canvas position.");
            Destroy(gameObject);
            return;
        }
        

        // 2.4. Одновременно с полетом: уменьшение и исчезновение
        sequence.Join(rectTransform.DOScale(0.2f, flyToScoreDuration).SetEase(Ease.InBack)); // Уменьшаем до 20% от исходного размера
        sequence.Join(textMesh.DOFade(0f, flyToScoreDuration).SetEase(Ease.OutQuad)); // Исчезаем

        // Уничтожаем объект после завершения анимации
        sequence.OnComplete(() => Destroy(gameObject));
    }
}