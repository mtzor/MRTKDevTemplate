using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "NewModuleScriptableObj", menuName = "myObjects/ModuleData")]
public class ModuleScriptable : ScriptableObject
{
    public int moduleID;
    public string Name;
    public Vector3 Size;
    public int[] Area;
   
}





