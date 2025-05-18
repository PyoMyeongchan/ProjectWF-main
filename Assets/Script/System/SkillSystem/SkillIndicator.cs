using UnityEngine;
using System;

public class SkillIndicator : MonoBehaviour
{
    [SerializeField] private GameObject targetingPanel;

    private GameObject currentIndicator;
    private Action<Vector3> onTargetConfirmed;
    private bool isTargeting = false;
    
    private Camera mainCamera;
    
    public void StartTargeting(GameObject indicatorPrefab, Action<Vector3> onConfirm)
    {
        if (isTargeting)
        {
            return;
        }


        isTargeting = true;
        targetingPanel.SetActive(true);

        onTargetConfirmed = onConfirm;
        currentIndicator = Instantiate(indicatorPrefab, targetingPanel.transform);

        UpdateIndicatorPositionToMouse();
    }

    private void Start()
    {
        targetingPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isTargeting) return;

        UpdateIndicatorPositionToMouse();

        if (Input.GetMouseButtonDown(0))
        {
            if (IsMouseOverPanel())
            {
                Vector3 targetWorldPos = GetTargetWorldPosition();
                onTargetConfirmed?.Invoke(targetWorldPos);
            }

            EndTargeting();
        }
    }

    private void UpdateIndicatorPositionToMouse()
    {
        Vector2 localPoint;
        RectTransform panelRect = targetingPanel.GetComponent<RectTransform>();
        RectTransform indicatorRect = currentIndicator.GetComponent<RectTransform>();

        // 마우스 위치를 패널 기준 로컬 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            panelRect,
            Input.mousePosition,
            null,
            out localPoint
        );

        // 인디케이터 크기 고려한 클램프 위치 계산
        Vector2 clampedPosition = ClampToPanel(panelRect, indicatorRect, localPoint);

        // 위치 적용
        indicatorRect.localPosition = clampedPosition;
    }
    
    private Vector2 ClampToPanel(RectTransform panel, RectTransform indicator, Vector2 localPos)
    {
        Vector2 panelSize = panel.rect.size;
        Vector2 indicatorSize = indicator.rect.size;

        float halfPanelWidth = panelSize.x / 2f;
        float halfPanelHeight = panelSize.y / 2f;

        float halfIndicatorWidth = indicatorSize.x / 2f;
        float halfIndicatorHeight = indicatorSize.y / 2f;

        float clampedX = Mathf.Clamp(localPos.x, -halfPanelWidth + halfIndicatorWidth, halfPanelWidth - halfIndicatorWidth);
        float clampedY = Mathf.Clamp(localPos.y, -halfPanelHeight + halfIndicatorHeight, halfPanelHeight - halfIndicatorHeight);

        return new Vector2(clampedX, clampedY);
    }

    private bool IsMouseOverPanel()
    {
        RectTransform panelRect = targetingPanel.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(panelRect, Input.mousePosition);
    }

    private Vector3 GetTargetWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private void EndTargeting()
    {
        isTargeting = false;
        targetingPanel.SetActive(false);

        if (currentIndicator != null)
        {
            Destroy(currentIndicator);
            currentIndicator = null;
        }

        onTargetConfirmed = null;
    }
}
