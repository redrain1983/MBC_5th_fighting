using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer2 : MonoBehaviour
{
    [Header("배치 설정")]
    public Camera playerCamera;
    public LayerMask groundMask;
    public float placeDistance = 5f;
    public float rotationSpeed = 90f;
    public GameObject playerCapsule;

    private GameObject previewObject;
    private float currentRotation = 0f;
    private bool isPlacing = false;

    void Update()
    {
        
        // 🔹 E 키로 배치 모드 시작/취소
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPlacing)
            {
                var data = FurnitureManager.Instance.GetSelectedFurniture();
                if (data != null && data.prefab != null)
                {
                    previewObject = Instantiate(data.prefab);
                    Collider col = previewObject.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                    SetTransparent(previewObject, true);
                    isPlacing = true;
                }
                else
                    Debug.Log("⚠️ 선택된 가구가 없습니다!");
            }
            else
            {
                CancelPlacement();
            }
        }

        // 🔹 ESC나 우클릭으로 취소
        if (isPlacing && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
        {
            CancelPlacement();
            return;
        }

        if (!isPlacing) return;

        // 🔹 Raycast로 위치 갱신
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, placeDistance, groundMask))
        {
            Vector3 placePos = hit.point;
            previewObject.transform.position = placePos;
            previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

            // 회전
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
                currentRotation += scroll * rotationSpeed * Time.deltaTime * 100f;

            // 좌클릭 시 설치
            if (Input.GetMouseButtonDown(0))
            {
                var data = FurnitureManager.Instance.GetSelectedFurniture();
                if (data != null)
                {
                    GameObject placed = Instantiate(data.prefab, placePos, Quaternion.Euler(0f, currentRotation, 0f));
                    Collider playerCol = playerCapsule.GetComponent<Collider>();
                    Collider furnitureCol = placed.GetComponent<Collider>();
                    if (playerCol != null && furnitureCol != null)
                        Physics.IgnoreCollision(playerCol, furnitureCol);
                    SetTransparent(placed, false);
                }
                CancelPlacement();
            }
        }
    }

    private void CancelPlacement()
    {
        if (previewObject != null)
            Destroy(previewObject);
        previewObject = null;
        isPlacing = false;
        currentRotation = 0f;
    }

    private void SetTransparent(GameObject obj, bool transparent)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)
            {
                if (transparent)
                {
                    Color c = m.color; c.a = 0.5f; m.color = c;
                    m.shader = Shader.Find("Standard");
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.renderQueue = 3000;
                }
                else
                {
                    Color c = m.color; c.a = 1f; m.color = c;
                    m.SetInt("_ZWrite", 1);
                    m.DisableKeyword("_ALPHABLEND_ON");
                    m.renderQueue = -1;
                }
            }
        }
    }
}