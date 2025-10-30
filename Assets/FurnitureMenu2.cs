using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureMenu : MonoBehaviour
{
    [System.Serializable]
    public class FurnitureItem
    {
        public string name;         // UI 표시용 이름
        public GameObject prefab;   // 실제 프리팹
        public Sprite icon;         // 버튼 이미지
    }

    [Header("가구 리스트")]
    public FurnitureItem[] furnitureList;

    [Header("UI 요소")]
    public GameObject menuPanel;          // 메뉴 전체 패널 (E키로 열고 닫기)
    public Transform buttonContainer;     // 버튼들이 들어갈 Content (ScrollView 내부)
    public Button buttonTemplate;         // 버튼 템플릿 (비활성 상태)
    public KeyCode toggleKey = KeyCode.E; // 메뉴 열기/닫기 키

    private ObjectPlacer1 objectPlacer;
    private bool isMenuOpen = false;

    void Start()
    {
        objectPlacer = FindObjectOfType<ObjectPlacer1>();

        // 버튼 템플릿은 복제용이므로 숨김
        if (buttonTemplate != null)
            buttonTemplate.gameObject.SetActive(false);

        // 버튼 생성
        foreach (var item in furnitureList)
        {
            Button newButton = Instantiate(buttonTemplate, buttonContainer);
            newButton.gameObject.SetActive(true);

            // 텍스트 설정
            Text text = newButton.GetComponentInChildren<Text>();
            if (text != null) text.text = item.name;

            // 아이콘 설정
            Image img = newButton.GetComponent<Image>();
            if (item.icon != null && img != null)
                img.sprite = item.icon;

            // 클릭 이벤트
            newButton.onClick.AddListener(() => OnSelectFurniture(item));
        }

        // 시작 시 메뉴 닫기
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    void Update()
    {
        // E키로 메뉴 토글
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        if (menuPanel != null)
            menuPanel.SetActive(isMenuOpen);

        // 마우스 커서 잠금 / 해제
        if (isMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // 게임 일시정지 (선택 편하게)
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    void OnSelectFurniture(FurnitureItem item)
    {
        if (objectPlacer == null) return;

        // 선택한 가구를 ObjectPlacer에 전달
        objectPlacer.SetFurniture(item.prefab);

        // 메뉴 닫고 설치 모드로 전환
        ToggleMenu();

        Debug.Log($"선택된 가구: {item.name}");
    }
}
