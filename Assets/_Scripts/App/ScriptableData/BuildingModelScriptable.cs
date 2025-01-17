using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;


[CreateAssetMenu(fileName = "NewBuildingModelObj", menuName = "myObjects/BuildingData")]
public class BuildingModelScriptable : ScriptableObject
{
    [SerializeField] BuildingType type;
    [SerializeField] BuildingMovement move_type;

    public List<Room> Rooms { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public enum BuildingType
    {
        small,
        medium_T1,
        medium_T2,
        large_T1,
        large_T2,
        xlarge_T1,
        xlarge_T2
    }

    public enum BuildingMovement
    {
        T1,
        T2,
        T3
    }
    public class Room
    {
        public string Name { get; set; }
        public Size Size { get; set; }

        public Room(string name, Size size)
        {
            Name = name;
            Size = size;
        }
    }

    public enum Size
    {
        Small,
        Large
    }
}
