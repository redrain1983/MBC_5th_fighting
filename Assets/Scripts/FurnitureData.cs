using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFurniture", menuName = "Furnitures/Furniture Data")]
public class FurnitureData : ScriptableObject
{
    public string furnitureName;      // ���� �̸�
    public GameObject prefab;         // ���� ��ġ�� ������
    public Sprite icon;               // �޴��� ǥ�õ� ������
    public Vector3 previewOffset;     // ���� ������ (�ɼ�)
                                      // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
