using UnityEngine;
using TMPro; // <-- ���������, ��� ��� ������ ����!
using DG.Tweening; // <-- ���������, ��� ��� ������ ����, ���� �� ����������� DOTween!

public class FloatingText : MonoBehaviour
{
    public float moveUpDistance = 50f; // ��������� ����� ���������� ����� � �������� Canvas
    public float initialMoveDuration = 0.5f; // ������������ ������� �������
    public float flyToScoreDuration = 0.7f; // ������������ ������ � �����
    public Vector3 initialWorldOffset = new Vector3(0, 0.5f, 0); // ��������� �������� �� ������� ���� � ������� �����������
    public Ease easeType = Ease.OutQuad; // ��� �������� ��� �����������

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
    /// �������������� � ��������� �������� ������������ ������.
    /// </summary>
    /// <param name="scoreAmount">���������� �����.</param>
    /// <param name="startWorldPosition">������� �������, ��� ��������� ���������� (��������, ����� ������ ���).</param>
    /// <param name="targetScoreWorldUIPosition">������� ������� UI �������� �� ������ ������ (���������� �� ScoreManager).</param>
    public void Initialize(int scoreAmount, Vector3 startWorldPosition, Vector3 targetScoreWorldUIPosition)
    {
        textMesh.text = $"+{scoreAmount}";
        textMesh.color = Color.white; // ���������� ��������� ���� (����� ������� ��������� �� ������)
        rectTransform.localScale = Vector3.one; // ��������, ��� ������� ����������

        // 1. ������������� ��������� ������� ������ � Canvas
        // ������������ ������� ������� � ������� Canvas
        Vector3 spawnWorldPos = startWorldPosition + initialWorldOffset;
        
        // ��� Canvas � Screen Space - Camera, ���������� RectTransformUtility
        // ����� �������� ��������� ���������� � RectTransform �������� (Canvas)
        RectTransform canvasRectTransform = transform.parent.GetComponent<RectTransform>();
        if (canvasRectTransform == null)
        {
            Debug.LogError("FloatingText's parent is not a RectTransform (Canvas). Cannot set position correctly.");
            Destroy(gameObject);
            return;
        }

        Vector2 localPoint;
        // ��������� ������� ������� � ��������, ����� �� �������� � ��������� ��� Canvas
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


        // 2. ��������: ������� ������� �����, ����� � �����
        Sequence sequence = DOTween.Sequence();

        // 2.1. ����� �����
        sequence.Append(rectTransform.DOLocalMoveY(rectTransform.localPosition.y + moveUpDistance, initialMoveDuration).SetEase(easeType));

        // 2.2. �������� �����
        sequence.AppendInterval(0.1f);

        // 2.3. ����� � ����� ������
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
        

        // 2.4. ������������ � �������: ���������� � ������������
        sequence.Join(rectTransform.DOScale(0.2f, flyToScoreDuration).SetEase(Ease.InBack)); // ��������� �� 20% �� ��������� �������
        sequence.Join(textMesh.DOFade(0f, flyToScoreDuration).SetEase(Ease.OutQuad)); // ��������

        // ���������� ������ ����� ���������� ��������
        sequence.OnComplete(() => Destroy(gameObject));
    }
}