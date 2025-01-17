using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomContainer : MonoBehaviour
{
    [SerializeField] private bool isDouble;
    [SerializeField] private Transform spawnPos;
    [SerializeField] private int rotation;

    // Start is called before the first frame update
    void Awake()
    {
        
    }

  public void onRoomContainerBtnPressed()
    {
        //ASK THE LAYOUT MANAGER TO CLOSE PREVIOUS OPEN MENUS
        //ASK LAYOUT MANAGER TO OPEN THIS MENU PASS THE IS DOUBLE PARAMETER AND THE POSITION PARAMETER

        Debug.Log("CUSTOMIZE MANAGER " + CustomizeManager.Instance.name);

        if (CustomizeManager.Instance.GetCurrentLayoutManager() == null)
        {

            Debug.Log("CUSTOMIZE MANAGER CURRENT LAYOUT MANAGER NULL" );
        }
        Debug.Log("SPAWN POS ROOM CONTAINER" + spawnPos.position.x + spawnPos.position.y + spawnPos.position.z);
        CustomizeManager.Instance.GetCurrentLayoutManager().OpenMenu(isDouble, spawnPos,rotation);

        Debug.Log("after open menu");
    }
}
