using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureMenu : MonoBehaviour
{
    [System.Serializable]
    public class FurnitureItem
    {
        public string name;         // UI ǥ�ÿ� �̸�
        public GameObject prefab;   // ���� ������
        public Sprite icon;         // ��ư �̹���
    }

    [Header("���� ����Ʈ")]
    public FurnitureItem[] furnitureList;

    [Header("UI ���")]
    public GameObject menuPanel;          // �޴� ��ü �г� (EŰ�� ���� �ݱ�)
    public Transform buttonContainer;     // ��ư���� �� Content (ScrollView ����)
    public Button buttonTemplate;         // ��ư ���ø� (��Ȱ�� ����)
    public KeyCode toggleKey = KeyCode.E; // �޴� ����/�ݱ� Ű

    private ObjectPlacer1 objectPlacer;
    private bool isMenuOpen = false;

    void Start()
    {
        objectPlacer = FindObjectOfType<ObjectPlacer1>();

        // ��ư ���ø��� �������̹Ƿ� ����
        if (buttonTemplate != null)
            buttonTemplate.gameObject.SetActive(false);

        // ��ư ����
        foreach (var item in furnitureList)
        {
            Button newButton = Instantiate(buttonTemplate, buttonContainer);
            newButton.gameObject.SetActive(true);

            // �ؽ�Ʈ ����
            Text text = newButton.GetComponentInChildren<Text>();
            if (text != null) text.text = item.name;

            // ������ ����
            Image img = newButton.GetComponent<Image>();
            if (item.icon != null && img != null)
                img.sprite = item.icon;

            // Ŭ�� �̺�Ʈ
            newButton.onClick.AddListener(() => OnSelectFurniture(item));
        }

        // ���� �� �޴� �ݱ�
        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    void Update()
    {
        // EŰ�� �޴� ���
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

        // ���콺 Ŀ�� ��� / ����
        if (isMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0f; // ���� �Ͻ����� (���� ���ϰ�)
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

        // ������ ������ ObjectPlacer�� ����
        objectPlacer.SetFurniture(item.prefab);

        // �޴� �ݰ� ��ġ ���� ��ȯ
        ToggleMenu();

        Debug.Log($"���õ� ����: {item.name}");
    }
}
