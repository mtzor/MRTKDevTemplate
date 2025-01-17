using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class ModuleLayouts 
{
    public string moduleName; // The string
    public List<Transform> layouts = new List<Transform>(); // The associated list of strings
    public List<RoomLayouts> roomLayouts = new List<RoomLayouts>(); // The associated list of strings
}
