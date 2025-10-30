using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FurnitureDatabase", menuName = "Furnitures/Furniture Database")]
public class FurnitureDatabase : ScriptableObject
{
    public List<FurnitureData> allFurniture = new List<FurnitureData>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
