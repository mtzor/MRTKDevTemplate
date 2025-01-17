using UnityEngine;

[CreateAssetMenu(fileName = "NewObjectData", menuName = "myObjects/Object Data")]
public class ObjectDataSO : ScriptableObject
{
    public string Name;
    public int ID;
    public Vector2Int Size = Vector2Int.one;
    public GameObject Prefab;
}