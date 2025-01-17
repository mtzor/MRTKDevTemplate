using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerDataObj", menuName = "myObjects/PlayerData")]
public class PlayerDataScriptable : ScriptableObject
{
    public string playerName;
    public int playerID;
    public Color Color;
    public PlayerType playerType;


    // Start is called before the first frame update
    void Start()
    {
        playerType = PlayerType.NonExpert;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public enum PlayerType
{
    Expert,
    NonExpert
}
