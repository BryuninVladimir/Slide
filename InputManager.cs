using UnityEngine;

public class InputManager : MonoBehaviour {
    // GameManager Instance будет использоваться для вызова логики
    // Примечание: Убедись, что GameManager.Instance инициализирован первым
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // Для мобильных устройств (тачскрин)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            ProcessTouch(Input.GetTouch(0).position);
        }
        // Для ПК (клик мышью) - можно удалить, если ориентируешься только на мобильные
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
            // Проверяем, коснулись ли мы InsertPoint
            InsertPoint insertPoint = hit.collider.GetComponent<InsertPoint>();
            if (insertPoint != null)
            {
                Debug.Log($"Clicked Insert Point: {insertPoint.direction} at index {insertPoint.index}");
                // Здесь мы вызовем метод в GameManager для обработки хода
                GameManager.Instance.PerformLamaInsert(insertPoint.direction, insertPoint.index);
            }
            // Можно добавить логику для клика по ламам, если она будет нужна позже
            else
            {
                Debug.Log($"Clicked on: {hit.collider.name}");
            }
        }
    }
}