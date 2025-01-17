using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Netcode;
//using Unity.VisualScripting;
//using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;
public class VisualizeManager : NetworkBehaviour
{
    private static VisualizeManager _instance;
    [SerializeField] private VisualizeViewUIController uiController;

    [SerializeField] private View view;

    [SerializeField] private GameObject visualize_P1_UI;

    [SerializeField] private Transform layoutContainer;

    [SerializeField] private ModuleLayouts[] moduleLayouts = new ModuleLayouts[9];

    [SerializeField] Transform[] roomPrefabs;

    public List<(RoomInfo, List<RoomData>)> savedData;

    public List<RoomInfo> roomInfo;
    public List<List<RoomData>> roomData;


    public List<RoomData> _spawnedRoomData;
    public List<GameObject> _spawnedRooms;

    public List<RoomData> _compareSpawnedRoomData;
    public List<GameObject> _compareSpawnedRooms;

    private VizualizePhase currentPhase;

    public int[] ratings;

    public List<Transform> itemList = new List<Transform>();

    public string _selectedModule;
    public int selectedModuleIndex;
    public string SelectedModule { set => _selectedModule = value; }

    public VisualizeViewUIController UIController { get; }
    public View View {  get { return view; } }

    public override void OnNetworkSpawn()
    {
       
    }
    public void SetRoomInfo(List<RoomInfo> roomInfo)
    {
        this.roomInfo = roomInfo;
    }
    public void SetRoomData(List<List<RoomData>> roomData)
    {
        this.roomData = roomData;
    }
    public void SetSpawnedRoomData(int index)
    {
        _spawnedRoomData=savedData[index].Item2;
    }
    public void SetCompareSpawnedRoomData(int index)
    {
        _compareSpawnedRoomData = savedData[index].Item2;
    }

    public VizualizePhase CurrentPhase { get; set; }
    public void ToggleVisualize_P1_UI(bool toggle)
    {
        visualize_P1_UI.SetActive(toggle);
    }

    public static VisualizeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<VisualizeManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject customizeManagerObject = new GameObject("VisualizeManager");
                    _instance = customizeManagerObject.AddComponent<VisualizeManager>();
                }
            }

            return _instance;
        }
    }

    public enum VizualizePhase
    {
        Vizualize_layout,
        Rated_layout,
    }
    public List<Transform> SetupItemList()
    {
        int count = 0;
        foreach (var data in savedData)
        {
            RoomInfo roomInfo = data.Item1;
            int roomId = roomInfo.roomId;
            int layoutIndex = roomInfo.layoutIndex;
            int customizationIndex = roomInfo.customizationIndex;
            Debug.Log("Saved data roomid:" +roomId+" layoutIndex:"+layoutIndex+" customizationindex"+customizationIndex);

            Transform item = moduleLayouts[roomId].roomLayouts[layoutIndex].customizationLayouts[customizationIndex];
            itemList.Add(item);
            count++;
        }
        ratings=new int[count];

        for (int i = 0; i < count; i++)
        {
            ratings[i] = 0;
        }

        return itemList;
    }
    public async void SetupLayoutInterfaces()
    {
        Debug.Log("SELECTED MODULE: " + _selectedModule);
        ToggleVisualize_P1_UI(true);

        currentPhase = VizualizePhase.Vizualize_layout;

        savedData = await SaveSystem.LoadRoomTuplesFromCloudAsync(_selectedModule);

        // Find layouts for the selected module
        List<Transform> itemList = SetupItemList();

        // Instantiate and configure the PrivateView (not networked)
        view.Setup(itemList);

        uiController.SetView();

        uiController.RatingIndicator.Rate(0);
    }

    public ModuleLayouts currModule;

    public List<Transform> selectedModuleRoomLayouts = new List<Transform>();
   
    public void PropagatePhaseUpdate(AppManager.AppPhase phase)
    {
        AppManager.Instance.UpdatePhase(phase);
    }

    public void MainMenu()
    {
        AppManager.Instance.UpdatePhase(AppManager.AppPhase.MainMenu);
        ToggleVisualize_P1_UI(false);
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

    public void DespawnCompareRooms()
    {
        foreach (GameObject room in _compareSpawnedRooms)
        {
            // If not networked, simply destroy it
            Destroy(room);
        }
        _compareSpawnedRooms.Clear();
    }

    public void RespawnAllRooms()
    {
        foreach (RoomData roomData in _spawnedRoomData)
        {
            RespawnRoom(roomData);
        }
    }

    public void RespawnAllCompareRooms()
    {
        foreach (RoomData roomData in _compareSpawnedRoomData)
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

    public void OffsetRooms(float offset)
    {
        // Offset the rooms in the _spawnedRooms list
        foreach (GameObject room in _spawnedRooms)
        {
            if (room != null)
            {
                room.transform.localPosition += new Vector3(offset, 0, 0);
            }
        }

        // Offset the rooms in the _compareSpawnedRooms list symmetrically
        foreach (GameObject room in _compareSpawnedRooms)
        {
            if (room != null)
            {
                room.transform.localPosition += new Vector3(-offset, 0, 0);
            }
        }
    }
    public void SetRating(int rating)
    {
        uiController.RatingIndicator.Rate(rating);
    }
    public async void SubmitRatings()
    {
        await SaveSystem.SaveDesignRatingAsync(LobbyManager.Instance.GetPlayerName(),_selectedModule,ratings);
        MainMenu();
    }
}
