using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBtn : MonoBehaviour
{
    [SerializeField] private int id;
    [SerializeField] private bool isDouble;

     

    public void onRoomBtnPressed()
    {
        CustomizeManager.Instance.GetCurrentLayoutManager().SpawnRoom(id);
    }
}
