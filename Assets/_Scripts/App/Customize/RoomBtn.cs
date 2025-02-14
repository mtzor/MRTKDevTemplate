using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBtn : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private bool isDouble;

     

    public void onRoomBtnPressed()
    {
        if (CustomizeManager.Instance.isCurrentLayoutManagerShared)
        {
            CustomizeManager.Instance.SharedLayoutManager().SpawnRoom(id);
        }
        else
        {
            CustomizeManager.Instance.PrivateLayoutManager().SpawnRoom(id);
        }
    }
}
