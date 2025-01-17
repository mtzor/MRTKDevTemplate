using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RoomLayouts 
{
    public List<Transform> roomLayouts = new List<Transform>(); // The associated list of strings
    public List<Transform>customizationLayouts= new List<Transform>();// gameobjects for customization
}
