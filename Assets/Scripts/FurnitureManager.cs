using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureManager : MonoBehaviour
{
    public static FurnitureManager Instance;
    private FurnitureData currentFurniture;

    void Awake()
    {
        Instance = this;
    }

    public void SelectFurniture(FurnitureData data)
    {
        currentFurniture = data;
        Debug.Log("선택된 가구: " + data.furnitureName);
    }

    public FurnitureData GetSelectedFurniture()
    {
        return currentFurniture;
    }
}
