using UnityEngine;
using UnityEngine.UI;

public class FurnitureMenu : MonoBehaviour
{
    public FurnitureDatabase database;
    public GameObject buttonPrefab;
    public Transform contentParent;
    public FurnitureManager manager;

    [Header("메뉴 활성화 여부")]
    public GameObject menuPanel; //  반드시 Panel을 연결해야 함!

    private bool isMenuOpen = false;

    void Start()
    {
        // 버튼 생성
        foreach (var data in database.allFurniture)
        {
            GameObject btn = Instantiate(buttonPrefab, contentParent);
            btn.GetComponentInChildren<Text>().text = data.furnitureName;
            btn.GetComponentInChildren<Image>().sprite = data.icon;

            btn.GetComponent<Button>().onClick.AddListener(() =>
            {
                manager.SelectFurniture(data);
                CloseMenu(); // 선택 후 자동 닫기
            });
        }

        CloseMenu(); // 시작 시 비활성화
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isMenuOpen) CloseMenu();
            else OpenMenu();
        }
    }

    public void OpenMenu()
    {
        isMenuOpen = true;
        menuPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f; // 플레이어 멈춤
    }

    public void CloseMenu()
    {
        isMenuOpen = false;
        menuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }
}

