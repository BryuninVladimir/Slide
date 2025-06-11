using UnityEngine;

public class InputManager : MonoBehaviour {
    // GameManager Instance ����� �������������� ��� ������ ������
    // ����������: �������, ��� GameManager.Instance ��������������� ������
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // ��� ��������� ��������� (��������)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            ProcessTouch(Input.GetTouch(0).position);
        }
        // ��� �� (���� �����) - ����� �������, ���� �������������� ������ �� ���������
        else if (Input.GetMouseButtonDown(0))
        {
            ProcessTouch(Input.mousePosition);
        }
    }

    void ProcessTouch(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // ���������, ��������� �� �� InsertPoint
            InsertPoint insertPoint = hit.collider.GetComponent<InsertPoint>();
            if (insertPoint != null)
            {
                Debug.Log($"Clicked Insert Point: {insertPoint.direction} at index {insertPoint.index}");
                // ����� �� ������� ����� � GameManager ��� ��������� ����
                GameManager.Instance.PerformLamaInsert(insertPoint.direction, insertPoint.index);
            }
            // ����� �������� ������ ��� ����� �� �����, ���� ��� ����� ����� �����
            else
            {
                Debug.Log($"Clicked on: {hit.collider.name}");
            }
        }
    }
}