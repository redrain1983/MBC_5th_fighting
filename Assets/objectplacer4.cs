using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectplacer4 : MonoBehaviour
{
    [Header("배치 설정")]
    public Camera playerCamera;
    public GameObject furniturePrefab;
    public LayerMask groundMask;
    public LayerMask furnitureMask;
    public float placeDistance = 5f;
    public float rotationSpeed = 90f;
    public GameObject playerCapsule;

    [Header("가구 인식 조건")]
    public string targetTag = "Furniture";

    private GameObject previewObject;
    private float currentRotation = 0f;
    private bool isPlacing = false;
    private bool isDeleting = false;
    private bool isRepositioning = false;
    private bool isSelectingForReposition = false;

    private GameObject selectedObject;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private GameObject highlightedObject;
    private Material[][] originalMaterials;

    [Header("프리뷰 머티리얼")]
    public Material previewBaseMaterial; // 인스펙터에서 할당

    void Update()
    {
        // 설치 모드 진입/종료
        if (Input.GetKeyDown(KeyCode.E) && !isDeleting && !isRepositioning && !isSelectingForReposition)
        {
            if (!isPlacing) StartPlacement();
            else CancelPlacement();
        }

        // 삭제 모드 토글
        if (Input.GetKeyDown(KeyCode.R) && !isRepositioning && !isSelectingForReposition)
        {
            isDeleting = !isDeleting;
            if (isDeleting)
            {
                CancelPlacement();
                Debug.Log("삭제 모드 활성화");
            }
            else
            {
                ClearHighlight();
                Debug.Log("삭제 모드 해제");
            }
        }

        // 재배치 선택 모드 토글
        if (Input.GetKeyDown(KeyCode.T) && !isPlacing && !isDeleting && !isRepositioning)
        {
            isSelectingForReposition = !isSelectingForReposition;
            ClearHighlight();
            Debug.Log(isSelectingForReposition ? "재배치 선택 모드 활성화" : "재배치 선택 모드 해제");
        }

        // 모드별 처리
        if (isDeleting) { HandleDeleteMode(); return; }
        if (isSelectingForReposition) { HandleRepositionSelection(); return; }
        if (isRepositioning) { HandleRepositionMode(); return; }
        if (isPlacing) { HandlePlacementMode(); }
    }

    // ================= 설치 모드 =================
    private void StartPlacement()
    {
        if (furniturePrefab == null) return;
        CreatePreview(furniturePrefab);
        isPlacing = true;
    }

    private void HandlePlacementMode()
    {
        UpdatePreviewPosition();

        // 설치 확정
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = previewObject.transform.position;
            PlaceObject(pos, currentRotation);
        }

        // 취소
        if (Input.GetMouseButtonDown(1))
            CancelPlacement();
    }

    private void PlaceObject(Vector3 pos, float rotation)
    {
        GameObject placed = Instantiate(furniturePrefab, pos, Quaternion.Euler(0f, rotation, 0f));
        placed.tag = targetTag;

        Collider playerCol = playerCapsule.GetComponent<Collider>();
        Collider furnitureCol = placed.GetComponent<Collider>();
        if (playerCol != null && furnitureCol != null)
            Physics.IgnoreCollision(playerCol, furnitureCol);

        CancelPlacement();
    }

    private void CancelPlacement()
    {
        if (previewObject != null) Destroy(previewObject);
        previewObject = null;
        isPlacing = false;
        currentRotation = 0f;
    }

    private void UpdatePreviewPosition()
    {
        if (previewObject == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, placeDistance, groundMask))
            previewObject.transform.position = hit.point;
        else
            previewObject.transform.position = playerCamera.transform.position + playerCamera.transform.forward * (placeDistance * 0.8f);

        // 마우스 스크롤 회전
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
            currentRotation += scroll * rotationSpeed * Time.deltaTime * 100f;

        previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);
    }

    // ================= 삭제 모드 =================
    private void HandleDeleteMode()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, placeDistance, furnitureMask))
        {
            GameObject target = hit.collider.gameObject;
            if (!target.CompareTag(targetTag)) { ClearHighlight(); return; }

            if (highlightedObject != target)
            {
                ClearHighlight();
                ApplyHighlight(target);
            }

            if (Input.GetMouseButtonDown(0))
            {
                Destroy(target);
                ClearHighlight();
                isDeleting = false;
            }
        }
        else ClearHighlight();
    }

    // ================= 재배치 선택 모드 =================
    private void HandleRepositionSelection()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, placeDistance, furnitureMask))
        {
            GameObject target = hit.collider.gameObject;
            if (!target.CompareTag(targetTag)) { ClearHighlight(); return; }

            if (highlightedObject != target)
            {
                ClearHighlight();
                ApplyHighlight(target);
            }

            if (Input.GetMouseButtonDown(0))
            {
                selectedObject = target;
                originalPosition = selectedObject.transform.position;
                originalRotation = selectedObject.transform.rotation;
                ClearHighlight();
                StartReposition();
                isSelectingForReposition = false;
            }

            if (Input.GetMouseButtonDown(1))
            {
                ClearHighlight();
                isSelectingForReposition = false;
            }
        }
        else ClearHighlight();
    }

    // ================= 재배치 모드 =================
    private void StartReposition()
    {
        if (selectedObject == null) return;

        // 프리뷰 생성 (Collider/Rigidbody 제거)
        CreatePreview(selectedObject);
        foreach (var col in previewObject.GetComponentsInChildren<Collider>())
            Destroy(col);
        foreach (var rb in previewObject.GetComponentsInChildren<Rigidbody>())
            Destroy(rb);

        selectedObject.SetActive(false);
        isRepositioning = true;
        currentRotation = selectedObject.transform.eulerAngles.y;
    }

    private void HandleRepositionMode()
    {
        UpdatePreviewPosition();

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = previewObject.transform.position;
            ConfirmReposition(pos, currentRotation);
        }

        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            CancelReposition();
    }

    private void ConfirmReposition(Vector3 pos, float rotation)
    {
        selectedObject.transform.position = pos;
        selectedObject.transform.rotation = Quaternion.Euler(0f, rotation, 0f);
        selectedObject.SetActive(true);

        if (previewObject != null) Destroy(previewObject);
        previewObject = null;
        selectedObject = null;
        isRepositioning = false;
    }

    private void CancelReposition()
    {
        if (selectedObject != null)
        {
            selectedObject.transform.position = originalPosition;
            selectedObject.transform.rotation = originalRotation;
            selectedObject.SetActive(true);
        }

        if (previewObject != null) Destroy(previewObject);
        previewObject = null;
        selectedObject = null;
        isRepositioning = false;
    }

    // ================= 프리뷰 생성 =================
    private void CreatePreview(GameObject original)
    {
        if (previewObject != null) Destroy(previewObject);

        previewObject = Instantiate(original, original.transform.position, original.transform.rotation);

        // 프리뷰용 머티리얼 적용 (하이라이트와 동일하게 통합)
        Renderer[] rends = previewObject.GetComponentsInChildren<Renderer>();
        foreach (var r in rends)
        {
            Material[] mats = new Material[r.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                Material m = new Material(previewBaseMaterial);
                Color c = m.color;
                c.a = 0.4f; // 반투명 유지
                m.color = c;
                mats[i] = m;
            }
            r.materials = mats;
        }

        // Rigidbody/Collider 제거
        foreach (var col in previewObject.GetComponentsInChildren<Collider>())
            Destroy(col);
        foreach (var rb in previewObject.GetComponentsInChildren<Rigidbody>())
            Destroy(rb);
    }

    // ================= 하이라이트 =================
    private void ApplyHighlight(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            originalMaterials[i] = r.materials;

            Material[] highlightMats = new Material[r.materials.Length];
            for (int j = 0; j < r.materials.Length; j++)
            {
                Material m = new Material(previewBaseMaterial);
                Color c = m.color;
                c.a = 0.4f; // 반투명 유지
                m.color = c;
                highlightMats[j] = m;
            }
            r.materials = highlightMats;
        }

        highlightedObject = obj;
    }

    private void ClearHighlight()
    {
        if (highlightedObject == null || originalMaterials == null) return;

        Renderer[] renderers = highlightedObject.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (i < originalMaterials.Length)
                renderers[i].materials = originalMaterials[i];
        }

        highlightedObject = null;
        originalMaterials = null;
    }

    public void SetFurniture(GameObject prefab)
    {
        furniturePrefab = prefab;
    }
}
