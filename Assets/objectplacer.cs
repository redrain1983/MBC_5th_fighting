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

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        // 바닥만 감지
        if (Physics.Raycast(ray, out RaycastHit hit, placeDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 placePos = hit.point;

            // 1️⃣ 프리뷰 오브젝트가 없으면 한 번만 생성
            if (previewObject == null)
            {
                previewObject = Instantiate(furniturePrefab);
                // 미리보기 충돌 비활성화 (자기 자신에 맞지 않게)
                Collider col = previewObject.GetComponent<Collider>();
                if (col != null) col.enabled = false;
                // 반투명 머티리얼 적용 (선택사항)
                SetPreviewTransparent(previewObject, true);
            }

            // 2️⃣ 위치/회전 갱신
            previewObject.transform.position = placePos;
            previewObject.transform.rotation = Quaternion.Euler(0f, currentRotation, 0f);

            // 3️⃣ 회전 (마우스 휠)
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
                currentRotation += scroll * rotationSpeed * Time.deltaTime * 100f;

            // 4️⃣ 왼쪽 클릭 시 설치
            if (Input.GetMouseButtonDown(0))
            {
                GameObject placed = Instantiate(furniturePrefab, placePos, Quaternion.Euler(0f, currentRotation, 0f));

                // 플레이어(캡슐)과 충돌하지 않도록 Physics.IgnoreCollision 적용
                Collider playerCol = playerCapsule.GetComponent<Collider>();
                Collider furnitureCol = placed.GetComponent<Collider>();
                if (playerCol != null && furnitureCol != null)
                    Physics.IgnoreCollision(playerCol, furnitureCol);

                SetPreviewTransparent(placed, false);
            }
        }
        else
        {
            // 시야에 바닥이 없으면 프리뷰 제거
            if (previewObject != null)
            {
                Destroy(previewObject);
                previewObject = null;
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
}