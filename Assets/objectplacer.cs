using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [Header("배치 설정")]
    public Camera playerCamera;              // FPS 카메라
    public GameObject furniturePrefab;       // 배치할 가구 프리팹
    public LayerMask groundMask;             // 바닥 감지용 (Ground 레이어)
    public float placeDistance = 5f;         // 배치 가능한 최대 거리
    public float rotationSpeed = 90f;        // 회전 속도 (도/초)
    public GameObject playerCapsule;         // 플레이어(캡슐) — 충돌 제외용

    private GameObject previewObject;        // 미리보기 오브젝트
    private float currentRotation = 0f;      // 현재 회전값
    private bool isPlacing = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPlacing)
            {
                // 배치 시작
                if (furniturePrefab != null)
                {
                    previewObject = Instantiate(furniturePrefab);
                    Collider col = previewObject.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                    SetPreviewTransparent(previewObject, true);
                    isPlacing = true;
                }
                else
                {
                    Debug.Log("⚠️ 가구 프리팹이 선택되지 않았습니다!");
                }
            }
            else
            {
                // 배치 취소
                CancelPlacement();
            }
        }// E 키로 배치 모드 시작/취소
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPlacing)
            {
                if (furniturePrefab != null)
                {
                    previewObject = Instantiate(furniturePrefab);
                    Collider col = previewObject.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                    SetPreviewTransparent(previewObject, true);
                    isPlacing = true;
                }
            }
            else
            {
                CancelPlacement();
            }
        }

        // ESC나 우클릭으로 취소
        if (isPlacing && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            CancelPlacement();
            return;
        }

        // 배치 모드 아닐 땐 return
        if (!isPlacing) return;

        // 미리보기 및 설치 로직
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (!isPlacing) return; // 배치 모드 아닐 때는 아무것도 하지 않음

        if (Physics.Raycast(ray, out RaycastHit hit, placeDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 placePos = hit.point;
            previewObject.transform.position = placePos;
            previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
                currentRotation += scroll * rotationSpeed * Time.deltaTime * 100f;

            if (Input.GetMouseButtonDown(0))
            {
                GameObject placed = Instantiate(furniturePrefab, placePos, Quaternion.Euler(0f, currentRotation, 0f));
                Collider playerCol = playerCapsule.GetComponent<Collider>();
                Collider furnitureCol = placed.GetComponent<Collider>();
                if (playerCol != null && furnitureCol != null)
                    Physics.IgnoreCollision(playerCol, furnitureCol);

                SetPreviewTransparent(placed, false);
                CancelPlacement();
            }
        }
    }

    // 반투명 처리 함수
    private void SetPreviewTransparent(GameObject obj, bool transparent)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                if (transparent)
                {
                    m.shader = Shader.Find("Standard");
                    Color c = m.color;
                    c.a = 0.5f;
                    m.color = c;
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.renderQueue = 3000;
                }
                else
                {
                    Color c = m.color;
                    c.a = 1f;
                    m.color = c;
                    m.SetInt("_ZWrite", 1);
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.renderQueue = -1;
                }
            }
        }
    }
    private void CancelPlacement()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }
        isPlacing = false;
        currentRotation = 0f;
    }
}