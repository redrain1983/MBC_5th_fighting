using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectplacer3 : MonoBehaviour
{
    [Header("배치 설정")]
    public Camera playerCamera;              // FPS 카메라
    public GameObject furniturePrefab;       // 배치할 가구 프리팹
    public LayerMask groundMask;             // 바닥 감지용
    public LayerMask furnitureMask;          // 가구 감지용 (삭제용)
    public float placeDistance = 5f;         // 배치 거리
    public float rotationSpeed = 90f;        // 회전 속도
    public GameObject playerCapsule;         // 플레이어 충돌 제외용

    [Header("가구 인식 조건")]
    public string targetTag = "Furniture";   // 이 태그가 붙은 오브젝트만 삭제/하이라이트

    private GameObject previewObject;
    private float currentRotation = 0f;
    private bool isPlacing = false;
    private bool isDeleting = false;

    // 하이라이트 관련
    private GameObject highlightedObject;
    private Material[] originalMaterials;

    [Header("하이라이트 설정")]
    public Color highlightColor = Color.yellow;

    void Update()
    {
        // --- E키: 설치 모드 On/Off ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPlacing)
                StartPlacement();
            else
                CancelPlacement();
        }

        // --- R키: 삭제 모드 On/Off ---
        if (Input.GetKeyDown(KeyCode.R))
        {
            isDeleting = !isDeleting;
            if (isDeleting)
            {
                Debug.Log("삭제 모드 활성화");
                CancelPlacement(); // 설치 중이었다면 취소
            }
            else
            {
                Debug.Log("삭제 모드 해제");
                ClearHighlight();
            }
        }

        // --- 삭제 모드 ---
        if (isDeleting)
        {
            HandleDeleteMode();
            return;
        }

        // --- 설치 모드 아닐 때 ---
        if (!isPlacing) return;

        // --- 배치 위치 갱신 ---
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, placeDistance, groundMask))
        {
            Vector3 placePos = hit.point;
            previewObject.transform.position = placePos;
            previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

            // --- 회전 ---
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
                currentRotation += scroll * rotationSpeed * Time.deltaTime * 100f;

            // --- 설치 ---
            if (Input.GetMouseButtonDown(0))
                PlaceObject(placePos);
        }

        // --- ESC나 우클릭으로 취소 ---
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            CancelPlacement();
    }

    // === 설치 모드 시작 ===
    private void StartPlacement()
    {
        if (furniturePrefab == null) return;

        previewObject = Instantiate(furniturePrefab);
        Collider col = previewObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;
        SetPreviewTransparent(previewObject, true);
        isPlacing = true;
    }

    // === 설치 실행 ===
    private void PlaceObject(Vector3 pos)
    {
        GameObject placed = Instantiate(furniturePrefab, pos, Quaternion.Euler(0f, currentRotation, 0f));

        // 설치된 오브젝트에 태그 자동 부여
        placed.tag = targetTag;

        Collider playerCol = playerCapsule.GetComponent<Collider>();
        Collider furnitureCol = placed.GetComponent<Collider>();
        if (playerCol != null && furnitureCol != null)
            Physics.IgnoreCollision(playerCol, furnitureCol);

        SetPreviewTransparent(placed, false);
        CancelPlacement();
    }

    // === 삭제 모드 처리 ===
    private void HandleDeleteMode()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, placeDistance, furnitureMask))
        {
            GameObject target = hit.collider.gameObject;

            // ✅ 특정 태그만 반응
            if (!target.CompareTag(targetTag))
            {
                ClearHighlight();
                return;
            }

            // 하이라이트 적용
            if (highlightedObject != target)
            {
                ClearHighlight();
                ApplyHighlight(target);
            }

            // 삭제
            if (Input.GetMouseButtonDown(0))
            {
                Destroy(target);
                Debug.Log("가구 삭제 완료: " + target.name);
                ClearHighlight();

                // ✅ 삭제 후 삭제 모드 해제
                isDeleting = false;
                Debug.Log("삭제 모드 자동 해제");
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    // === 반투명 처리 ===
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

    // === 하이라이트 적용 ===
    private void ApplyHighlight(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        originalMaterials = renderer.materials;
        Material highlightMat = new Material(Shader.Find("Standard"));
        highlightMat.color = highlightColor;
        renderer.material = highlightMat;

        highlightedObject = obj;
    }

    // === 하이라이트 해제 ===
    private void ClearHighlight()
    {
        if (highlightedObject == null || originalMaterials == null) return;

        Renderer renderer = highlightedObject.GetComponent<Renderer>();
        if (renderer == null) renderer = highlightedObject.GetComponentInChildren<Renderer>();
        if (renderer == null) return;

        renderer.materials = originalMaterials;
        highlightedObject = null;
        originalMaterials = null;
    }

    // === 배치 취소 ===
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
