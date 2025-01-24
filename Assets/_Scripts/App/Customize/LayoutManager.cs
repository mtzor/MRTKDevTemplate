using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
//using Unity.VisualScripting;
using UnityEngine;

public class LayoutManager : NetworkBehaviour
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
    public NetworkVariable<Vector3> spawnPos = new NetworkVariable<Vector3>(new Vector3(0f, 0f, 0f), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> spawnRotY = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    public void CloseOpenMenu()
    {
        if (openMenu != null)
        {
            openMenu.gameObject.SetActive(false);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DisplayLayoutModelServerRPC()
    {
        if (roomLayout == null)
        {
            Debug.LogError("RoomLayout is not assigned!");
            return;
        }

        Debug.Log("CALLING DISPLAY MODEL SERVER RPC");
        Transform instantiatedLayout = Instantiate(roomLayout);
        ScaleAndAnchorObject(instantiatedLayout, roomLayout, 2, ADJUSTED_MULTIPLIER, ADJUSTED_OFFSET);

        if (isShared)
        {
            instantiatedLayout.GetComponent<NetworkObject>().Spawn(true);
        }
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
            spawnPos.Value = pos.position;
            spawnRotY.Value = rotation;
        }
        else
        {
            SetRoomPosServerRPC(pos.position, rotation);
        }

        isSpawning = true;

        GameObject roomMenu = (isDouble) ? roomMenuL : roomMenuS;
        Transform parent = roomMenu.transform.parent;

        Vector3 offset = new Vector3(0.245f, 0.1375f, -0.254f);
        roomMenu.transform.localPosition = parent.InverseTransformPoint(pos.position + offset);
        roomMenu.SetActive(true);

        openMenu = roomMenu;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRoomPosServerRPC(Vector3 pos, int rot)
    {
        spawnPos.Value = pos;
        spawnRotY.Value = rot;
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
                return roomData.IsAtSamePosition(spawnPos.Value) ;
            });

            GameObject overlappingRoom = _spawnedRooms.FirstOrDefault(room =>
            {
                return Vector3.Distance(room.transform.position, spawnPos.Value) < DISTANCE_THRESHOLD;
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
            spawnedRoom.position = spawnPos.Value;
            spawnedRoom.rotation = Quaternion.Euler(roomPrefabs[id].transform.rotation.x, spawnRotY.Value, roomPrefabs[id].transform.rotation.z);
            spawnedRoom.localScale = roomPrefabs[id].localScale;

            TransformRoom(spawnedRoom);

            // Add the newly spawned room to both lists
            _spawnedRoomData.Add(new RoomData(spawnedRoom.gameObject));
            _spawnedRooms.Add(spawnedRoom.gameObject);

            openMenu.SetActive(false);
        }
        else
        {
            // Handle server-side spawning
            SpawnRoomForSharedClientsServerRPC(id);
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
            room.rotation = Quaternion.Euler(40, currentRotation.y, currentRotation.z);
        }
        else if (Mathf.Approximately(normalizedYRotation, 90f))
        {
            room.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, -40);
        }
        else if (Mathf.Approximately(normalizedYRotation, -90f))
        {
            room.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 40);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void RemoveRoomClientRPC(ulong networkObjectId)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObject))
        {
            GameObject overlappingRoom = netObject.gameObject;
            _spawnedRoomData.RemoveAll(roomData => { return roomData.IsAtSamePosition(spawnPos.Value);} );
            _spawnedRooms.RemoveAll( data => data == overlappingRoom);
        }
        else
        {
            Debug.LogError($"Failed to find NetworkObject with ID: {networkObjectId}");
        }
    }

    public void AddSpawnedRoomGameObject(GameObject room)
    {
        if (room!=null)
        { 
            _spawnedRooms.Add(room);
            if (openMenu != null)
            {
                openMenu.gameObject.SetActive(false);
            }
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnRoomForSharedClientsServerRPC(int id)
    {
        foreach (ulong clientId in ViewManager.Instance.SharedView.SharedClients())
        {
            SpawnRoomForClientRpc(id,clientId);
            Debug.Log("deSpawning loading screen for client: " + clientId);
        }
    }


    [ClientRpc(RequireOwnership = false)]
    private void SpawnRoomForClientRpc(int id,ulong clientId)
    {       
        if (NetworkManager.LocalClientId == clientId)
        {
            // Find existing room data based on name and position (to check overlap)
            RoomData overlappingRoomData = _spawnedRoomData.FirstOrDefault(roomData =>
            {
                return roomData.IsAtSamePosition(spawnPos.Value);
            });
            Debug.Log("OVERLAPPING ROOM DATA" + overlappingRoomData);

            // Remove RoomData if it exists
            if (overlappingRoomData != null)
            {
                Debug.Log("OVERLAPPING NOT NULL REMOVED");
                _spawnedRoomData.Remove(overlappingRoomData);
                _spawnedRoomData.RemoveAll(roomData => { return roomData.IsAtSamePosition(spawnPos.Value); });
            }

            // Find the existing overlapping GameObject
            GameObject overlappingRoom = _spawnedRooms.FirstOrDefault(room =>
            {
                return Vector3.Distance(room.transform.position, spawnPos.Value) < DISTANCE_THRESHOLD;
            });

            // If overlap is detected, remove the existing room
            if (overlappingRoom != null)
            {
                // Remove from both lists
                _spawnedRooms.Remove(overlappingRoom);
                Destroy(overlappingRoom);
                                            
            }

            // Spawn the new room
            Transform spawnedRoom = Instantiate(roomPrefabs[id]);
            spawnedRoom.position = spawnPos.Value;
            spawnedRoom.rotation = Quaternion.Euler(roomPrefabs[id].transform.rotation.x, spawnRotY.Value, roomPrefabs[id].transform.rotation.z);
            spawnedRoom.localScale = roomPrefabs[id].localScale;

            TransformRoom(spawnedRoom);

            Debug.Log("POSITION before SPAWN" + spawnedRoom.transform.position.x);

            AddSpawnedRoomGameObject(spawnedRoom.gameObject);


            AddRoomDataClientRpc(new RoomData(spawnedRoom.gameObject));
        }
    }

    [ClientRpc(RequireOwnership =false)]
    public void AddRoomDataClientRpc(RoomData roomData)
    {
        if (roomData != null)
        {
            if (!_spawnedRoomData.Contains(roomData))
            {
                _spawnedRoomData.Add(roomData);
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
        if (isShared)
        {
            SaveRoomDataServerRPC(null, CustomizeManager.Instance.sharedChoices);
        }
        else 
        {

            SaveRoomDataServerRPC(LobbyManager.Instance.GetPlayerName(), CustomizeManager.Instance.privateChoices);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SaveRoomDataServerRPC(string playerName, int[] choices)
    {
        // Add to the placedModules network list
        SaveRoomDataClientRPC(playerName,choices);
    }

    [ClientRpc(RequireOwnership = false)]
    public void SaveRoomDataClientRPC(string playerName,int[] choices)
    {
        // Add to the placedModules network list
        saveRoomDataAsync(playerName,choices);
    }

    public async void saveRoomDataAsync(string playerName, int[] choices)
    {
        await SaveSystem.SaveCustomizeDesignAsync(_spawnedRoomData, playerName, CustomizeManager.Instance.selectedModuleIndex, CustomizeManager.Instance._selectedModule, choices);
    }

}
