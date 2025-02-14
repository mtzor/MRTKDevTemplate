using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
//using Unity.VisualScripting;
using UnityEngine;

public class PrivateLayoutManager : NetworkBehaviour
{
    [SerializeField] Transform viewPos;
    public int aptID;
    [SerializeField] private Transform layout;
    [SerializeField] private Transform roomLayout;

    [SerializeField] bool isShared;

    [SerializeField] GameObject roomMenuS;
    [SerializeField] GameObject roomMenuL;

    [SerializeField] Transform[] roomPrefabs;
    [SerializeField] Transform[] furniturePrefabs;

    private GameObject openMenu;
    private RoomContainer lastPressedButton;

    public Vector3 spawnPos = new Vector3(0f, 0f, 0f);
    public int spawnRotY = 0;

    private bool isSpawning = false;

    // List to hold RoomData instances for respawning
    public List<RoomData> _spawnedRoomData;
    public List<GameObject> _spawnedRooms;
    private List<GameObject> _spawnedFurniture;

    private static float ADJUSTED_MULTIPLIER = 0.30137f;
    private static float ADJUSTED_OFFSET = 0;

    private static float DISTANCE_THRESHOLD = 0.01f;
    void Start()
    {
        _spawnedRooms = new List<GameObject>();
        _spawnedRoomData = new List<RoomData>();
        _spawnedFurniture = new List<GameObject>();
    }

    public Transform RoomLayout { get; set; }
    public void SetLayout(Transform layout)
    {
        roomLayout = layout;
    }
    public void SetRoomLayout(Transform layout)
    {
        roomLayout = layout;
    }
    public void SetLastPressedButton(RoomContainer newButton)
    {
        // Reset the appearance of the last pressed button
        if (lastPressedButton != null)
        {
            lastPressedButton.ResetButton();
        }

        // Update the last pressed button and highlight it
        lastPressedButton = newButton;
        lastPressedButton.HighlightButton();
    }
    public void CloseOpenMenu()
    {
        if (openMenu != null)
        {
            openMenu.gameObject.SetActive(false);
            lastPressedButton.ResetButton();
        }
    }

    public void DisplayLayoutModel()
    {
        if (roomLayout == null)
        {
            Debug.LogError("RoomLayout is not assigned!");
            return;
        }

        Debug.Log("CALLING DISPLAY MODEL SERVER RPC");
        Transform instantiatedLayout = Instantiate(roomLayout);
        ScaleAndAnchorObject(instantiatedLayout, roomLayout, 2, ADJUSTED_MULTIPLIER, ADJUSTED_OFFSET);

    }

    public void ScaleAndAnchorObject(Transform target, Transform roomLayout, float scaleMultiplier, float adjustmentMultiplier, float adjustedOffset)
    {
        if (target == null || roomLayout == null)
        {
            Debug.LogError("Invalid target or roomLayout");
            return;
        }

        Vector3 originalPosition = roomLayout.position;
        target.rotation = roomLayout.rotation;
        target.localScale = roomLayout.localScale * scaleMultiplier;

        float shiftAmount = scaleMultiplier * adjustmentMultiplier + adjustedOffset;
        Vector3 adjustedPosition = originalPosition;
        adjustedPosition.x += shiftAmount;
        target.position = adjustedPosition;

        Debug.Log($"Scaled object '{target.name}'. New scale: {target.localScale}, Adjusted position: {target.position}");
    }

    public void OpenMenu(bool isDouble, Transform pos, int rotation)
    {
        if (openMenu != null)
        {
            openMenu.SetActive(false);
            isSpawning = false;
        }

        if (IsServer)
        {
            spawnPos = pos.position;
            spawnRotY = rotation;
        }
        else
        {
            SetRoomPos(pos.position, rotation);
        }

        isSpawning = true;

        GameObject roomMenu = (isDouble) ? roomMenuL : roomMenuS;
        Transform parent = roomMenu.transform.parent;

        Vector3 offset = new Vector3(0.245f, 0.1375f, -0.254f);
        roomMenu.transform.localPosition = parent.InverseTransformPoint(pos.position + offset);
        roomMenu.SetActive(true);

        openMenu = roomMenu;
    }

    private void SetRoomPos(Vector3 pos, int rot)
    {
        spawnPos = pos;
        spawnRotY = rot;
    }

    public void SpawnRoom(int id)
    {
        if (id >= roomPrefabs.Count() || id < 0)
        {
            Debug.Log("Invalid room ID to spawn");
            return;
        }

        if (!isShared)
        {
            // Check for overlapping room based on name and position
            RoomData overlappingRoomData = _spawnedRoomData.FirstOrDefault(roomData =>
            {
                return roomData.IsAtSamePosition(spawnPos);
            });

            GameObject overlappingRoom = _spawnedRooms.FirstOrDefault(room =>
            {
                return Vector3.Distance(room.transform.position, spawnPos) < DISTANCE_THRESHOLD;
            });

            // If overlap is detected, remove the overlapping room
            if (overlappingRoom != null)
            {
                // Remove from both lists
                _spawnedRooms.Remove(overlappingRoom);

                // Find the corresponding RoomData and remove it
                if (overlappingRoomData != null)
                {
                    _spawnedRoomData.Remove(overlappingRoomData);
                }

                // Destroy the overlapping room
                Destroy(overlappingRoom);
            }

            // Spawn the new room
            Transform spawnedRoom = Instantiate(roomPrefabs[id]);
            spawnedRoom.position = spawnPos;
            spawnedRoom.rotation = Quaternion.Euler(roomPrefabs[id].transform.rotation.x, spawnRotY, roomPrefabs[id].transform.rotation.z);
            spawnedRoom.localScale = roomPrefabs[id].localScale;

            TransformRoom(spawnedRoom);

            // Add the newly spawned room to both lists
            _spawnedRoomData.Add(new RoomData(spawnedRoom.gameObject));
            _spawnedRooms.Add(spawnedRoom.gameObject);

            CloseOpenMenu();
        }        
    }

    public void TransformRoom(Transform room)
    {
        // Scale the room
        room.localScale = room.localScale * 2;

        // Get the current rotation in Euler angles
        Vector3 currentRotation = room.rotation.eulerAngles;

        // Normalize the Y rotation to the range [-180, 180] for easier comparison
        float normalizedYRotation = Mathf.Repeat(currentRotation.y + 180f, 360f) - 180f;

        if (Mathf.Approximately(normalizedYRotation, 180f) || Mathf.Approximately(normalizedYRotation, -180f))
        {
            room.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z);
        }
        else if (Mathf.Approximately(normalizedYRotation, 90f))
        {
            room.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z);
        }
        else if (Mathf.Approximately(normalizedYRotation, -90f))
        {
            room.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentRotation.z);
        }
    }


    public void AddSpawnedRoomGameObject(GameObject room)
    {
        if (room != null)
        {
            _spawnedRooms.Add(room);
            if (openMenu != null)
            {
                CloseOpenMenu();
            }
        }

    }

    public void DespawnAllRooms()
    {
        foreach (GameObject room in _spawnedRooms)
        {
            // If not networked, simply destroy it
            Destroy(room);
        }
        _spawnedRooms.Clear();

    }

    public void RespawnAllRooms()
    {
        foreach (RoomData roomData in _spawnedRoomData)
        {
            RespawnRoom(roomData);
        }
    }

    private void RespawnRoom(RoomData roomData)
    {
        if (roomData.roomID >= 0 && roomData.roomID < roomPrefabs.Length)
        {
            Transform roomObject = Instantiate(roomPrefabs[roomData.roomID]);

            if (roomObject != null)
            {
                // Set the transform properties
                roomObject.localScale = new Vector3(roomData.scaleX, roomData.scaleY, roomData.scaleZ);
                roomObject.transform.position = new Vector3(roomData.positionX, roomData.positionY, roomData.positionZ);
                roomObject.transform.rotation = Quaternion.Euler(roomData.rotationX, roomData.rotationY, roomData.rotationZ);

                Debug.Log($"Respawned object '{roomObject.name}'.");

                // Add the respawned room to the spawnedRooms list
                _spawnedRooms.Add(roomObject.gameObject);
            }
        }


    }

    public void FinalizeCurrentLayout()
    {

            SaveRoomData(LobbyManager.Instance.GetPlayerName(), CustomizeManager.Instance.privateChoices);
    }

    public void SaveRoomData(string playerName, int[] choices)
    {
        // Add to the placedModules network list
        saveRoomDataAsync(playerName, choices);
    }

    public async void saveRoomDataAsync(string playerName, int[] choices)
    {
        await SaveSystem.SaveCustomizeDesignAsync(_spawnedRoomData, playerName, CustomizeManager.Instance.selectedModuleIndex, CustomizeManager.Instance._selectedModule, choices);
    }

}
