using MixedReality.Toolkit.UX;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
//using static UnityEngine.Rendering.DebugUI;
//using UnityEditor.PackageManager;
using System;

public class SharedView : NetworkBehaviour, IView
{
    public List<Transform> items; // Shared list of items

    public NetworkVariable<int> currentIndex = new NetworkVariable<int>(0);

    private NetworkVariable<int> compareIndex = new NetworkVariable<int>(0);
    private Transform compareItem;
    private Transform compareItemInstance;

    private Transform currentItem;

    private Transform layoutContainer;

    private Transform LayoutContainer { get; set; }

    public NetworkVariable<int> selectedIndex = new NetworkVariable<int>(-2);

    public NetworkVariable<bool> answered = new NetworkVariable<bool>(false);

    private int localSelectedIndex=-1;  

    public List<ulong> sharedClients ;// List of clients in shared view
    public void SetItems(List<Transform> items) {  this.items = items; }
    public Transform GetLayoutContainer() { return layoutContainer; }
    public void Initialize(List<Transform> sharedItems, Transform layoutTransform)
    {
        items = sharedItems;
        layoutContainer = layoutTransform;
       //selectedIndex = new NetworkVariable<int>(-1);
      //  currentIndex = new NetworkVariable<int>(0);
      //  compareIndex = new NetworkVariable<int>(0);
        IsInCompareMode = false;
        IsComplete = false;

        if (sharedClients == null)
        {
            sharedClients = new List<ulong>();
        }
    }
    
    public void SetSelectedIndex(int value)
    {
        if (IsServer)
        {
            selectedIndex.Value = value;
        }
        else
        {
            SetSelectedIndexServerRpc(value);
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSelectedIndexServerRpc(int index)
    {
        selectedIndex.Value = index;
    }

    public int SelectedIndex()
    {
        return selectedIndex.Value;
    }

    public bool IsShared=> true; //completeview flag
    public bool IsComplete {get; set;} // Shared view flag
    public bool IsInCompareMode { get; set; }
    public Transform GetCurrentItem() { return currentItem; }
    public void ResetCurrentIndex() { ResetCurrentIndexServerRPC(); }
    public override void OnNetworkSpawn()
    {

        if (sharedClients == null)
        {
            sharedClients = new List<ulong>();
        }
        if (IsServer)
        {
            // Ensure currentIndex is properly initialized on the server
            currentIndex.Value = 0;
        }

        // Subscribe to the OnValueChanged callback for currentIndex
        currentIndex.OnValueChanged += OnCurrentIndexChanged;
    }
  
    public List<ulong> SharedClients() {  return sharedClients; }

    public override void OnNetworkDespawn()
    {
        // Unsubscribe from the OnValueChanged callback to avoid memory leaks
        currentIndex.OnValueChanged -= OnCurrentIndexChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetCurrentIndexServerRPC() 
    {
        currentIndex.Value = 0;
    }
    public void NextItem()
    {
        ChangeToNextItemServerRpc();//call server function to chage currentindex and request the spawn
    }

    [ServerRpc(RequireOwnership =false)]
    private void ChangeToNextItemServerRpc(ServerRpcParams rpcParams = default)
    {
        currentIndex.Value = (currentIndex.Value + 1) % items.Count;
    }

    public void PreviousItem()
    {
        ChangeToPreviousItemServerRpc();          
    }

    [ServerRpc(RequireOwnership =false)]
    private void ChangeToPreviousItemServerRpc(ServerRpcParams rpcParams = default)
    {
        currentIndex.Value = (currentIndex.Value - 1 + items.Count) % items.Count; // Circular navigation
        SpawnObjectForSharedClients();//???????????????? double spawn ?    
    }

    private void SpawnObjectForSharedClients()
    {
        foreach (ulong clientId in sharedClients)
        {
            SpawnForClientRpc(clientId);
            Debug.Log("ClientID IN SHAREDCLIENTS" + clientId);
            ShowCurrentItem();
        }
    }

    [ClientRpc(RequireOwnership =false)]
    private void SpawnForClientRpc(ulong clientId)
    {
        if (NetworkManager.LocalClientId == clientId)
        {
            Debug.Log("SPAWNING FOR CLIENT"+NetworkManager.LocalClientId);
            ShowCurrentItem();
        }
        else { Debug.Log("not valid client id"); }
    }

    public void DestroyCurrentItem()
    {
            if (layoutContainer == null)
            {
                Debug.LogError("LayoutContainer is not assigned. Cannot display items.");
                return;
            }

            // Destroy previously instantiated current item
            if (currentItem != null)
            {
            Destroy(currentItem.gameObject);
            }       
    }

    public void ShowCurrentItem()//LOCAL ITEM SHOW
    {

            if (layoutContainer == null)
            {                
                layoutContainer = ViewManager.Instance.PrivateView.GetLayoutContainer();
            return;
            }

            // Destroy previously instantiated current item
            if (currentItem != null)
            {
                Destroy(currentItem.gameObject);//destroy previously instantiated item
                if (CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Customize_layout)
                {
                    CustomizeManager.Instance.PrivateLayoutManager().DespawnAllRooms();
                }
            }

            // Destroy compare item if compare mode is toggled off
            if (!IsInCompareMode && compareItemInstance != null)
            {
                Destroy(compareItemInstance.gameObject); ;//destroy previously instantiated item
                compareItemInstance = null;
            }
            if(currentIndex.Value<=items.Count && currentIndex.Value >= 0)
            {
                Debug.Log("Current index before instantiation" + currentIndex.Value);
                Debug.Log($"Shared view displaying: {items[currentIndex.Value]}");
                currentItem = Instantiate(items[currentIndex.Value], layoutContainer);
            }
            

            if (CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Customize_layout)
            {

                if (CustomizeManager.Instance.isCurrentLayoutManagerShared)
                {
                    CustomizeManager.Instance.SharedLayoutManager().RespawnAllRooms();
                }
                else
                {
                    CustomizeManager.Instance.PrivateLayoutManager().RespawnAllRooms();
                }
            }

            if (CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Customize_layout)
                {
                currentItem.transform.localPosition = new Vector3(0.51f, 0.1f, 0.394f);
                currentItem.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                currentItem.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            }

        if (IsInCompareMode)
            {
                Debug.Log("Compare Mode Enabled");

                // Ensure compareIndex is valid
                compareIndex.Value = Mathf.Clamp(compareIndex.Value, 0, items.Count - 1);

                // Destroy the previous compare item if it exists
                if (compareItemInstance != null)
                {
                    Destroy(compareItemInstance.gameObject);
                }
                compareItem = items[compareIndex.Value];
                // Instantiate the compare item
                compareItemInstance = Instantiate(compareItem, layoutContainer);

                // Positioning
                float offset = 0.10f; // Adjust for desired spacing
                currentItem.localPosition = currentItem.localPosition + new Vector3(offset, 0, 0); // Move slightly to the right
                compareItemInstance.localPosition = compareItemInstance.localPosition + new Vector3(-offset, 0, 0); // Move symmetrically to the left

                Debug.Log($"Compare mode displaying: {currentItem.name} (right) and {compareItemInstance.name} (left)");
            }
       
    }

    private void OnCurrentIndexChanged(int oldValue, int newValue)
    {
        // Trigger logic whenever currentIndex changes
        Debug.Log($"Index changed from {oldValue} to {newValue}");
        SpawnObjectForSharedClients();//CALLED TWICE?????
    }

    public void CompareViewConvert()
    {
            compareViewServerRPC();
            ShowCurrentItem();
            // NextItem();

    }

    [ServerRpc(RequireOwnership =false)]
    private void compareViewServerRPC()
    {
       //setting compare index at the current item

        if (IsInCompareMode)
        {
            compareIndex.Value = currentIndex.Value;//setting compare index at the current item
            Debug.Log("Compare Index"+compareIndex.Value);  

            compareItem = items[compareIndex.Value];
            Debug.Log("Compare Item" + compareItem);

            items.RemoveAt(compareIndex.Value);


            currentIndex.Value = currentIndex.Value % items.Count;
        }
        else
        {
            // Exiting compare mode, clean up compare item
            if (compareItemInstance != null)
            {
                items.Insert(compareIndex.Value, compareItem);
                Destroy(compareItemInstance.gameObject);
                compareItemInstance = null;
            }
        }

        ShowCurrentItem();
    }

    // Called by clients to report their shared view status
    public void ReportSharedViewState(bool isShared)
    {
        if (IsClient)
        {
            ReportSharedViewStateServerRpc(NetworkManager.LocalClientId, isShared);
        }
    }

    [ServerRpc(RequireOwnership =false)]
    private void ReportSharedViewStateServerRpc(ulong clientId, bool isShared)
    {
        if (isShared)
        {
           AddClientClientRPC(clientId);
        }
        else
        {
            RemoveClientClientRPC(clientId);
        }
    }

    [ClientRpc(RequireOwnership =false)]
    public void AddClientClientRPC(ulong clientId)
    {
        if (!sharedClients.Contains(clientId))
            sharedClients.Add(clientId);
    }

    [ClientRpc(RequireOwnership = false)]
    public void RemoveClientClientRPC(ulong clientId)
    {
        if (sharedClients.Contains(clientId))
            sharedClients.Remove(clientId);
    }

    public async Task FinalizeChoice()
    {
        SetSelectedIndexServerRpc(-2);

        if (CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Customize_layout)
        {
            if (CustomizeManager.Instance.isCurrentLayoutManagerShared)
            {
                CustomizeManager.Instance.SharedLayoutManager().DespawnAllRooms();
            }
            else
            {
                CustomizeManager.Instance.PrivateLayoutManager().DespawnAllRooms();
            }
            Destroy(currentItem.gameObject);
        }
        //spawning loading screen for shared clients
        SpawnLoadingScreenForSharedClientsServerRPC();

        //spawning dialog for server to choose 
        SelectLayoutDialogServerRPC();

        //blocking while waiting answer
        while (!answered.Value)
        {
            await Task.Delay(1000);
            Debug.Log("In while");
        }

        //setting up based on the choices made
       if (selectedIndex.Value == -1)
       {

            ViewManager.Instance.finalizeChoiceBtn.gameObject.SetActive(true);
            DeSpawnLoadingScreenForSharedClientsServerRPC(false);
       }
       else if (selectedIndex.Value >= 0) 
       {

            DeSpawnLoadingScreenForSharedClientsServerRPC(true);
        }

        SetAnsweredServerRPC(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAnsweredServerRPC(bool value)
    {
        answered.Value = value;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeSpawnLoadingScreenForSharedClientsServerRPC(bool accepted)
    {
        foreach (ulong clientId in sharedClients)
        {
            DeSpawnLoadingScreenForClientRpc(clientId,accepted);
            Debug.Log("deSpawning loading screen for client: " + clientId);
        }
    }


    [ClientRpc(RequireOwnership = false)]
    private void DeSpawnLoadingScreenForClientRpc(ulong clientId,bool accepted)
    {
        //?????????? SHOULD WE CHANGE THE SHARED ENVIRONMENT WHILE SOMEONE ISNT THERE
        if (NetworkManager.LocalClientId == clientId)
        {
            if (!accepted)
            {
                Debug.Log("deSpawning text for client " + clientId);
                if (CustomizeManager.Instance.SharedPhase== CustomizeManager.CustomizePhase.Customize_layout)
                {
                    ShowCurrentItem();
                    if (CustomizeManager.Instance.isCurrentLayoutManagerShared)
                    {
                        CustomizeManager.Instance.SharedLayoutManager().RespawnAllRooms();
                    }
                    else
                    {
                        CustomizeManager.Instance.PrivateLayoutManager().RespawnAllRooms();
                    }
                }
            }
            else 
            {
                CustomizeManager.Instance.SetChoice(true,selectedIndex.Value);//seting choice

                Debug.Log("SETTING HOPEFULLY CHOICE FOR"+NetworkManager.LocalClientId +"S ELECTED INDEX IS "+ selectedIndex.Value);
                //CustomizeManager.Instance.SetupInterface(true);//setting next phase for shared view

                ViewManager.Instance.SetNextCurrentViewPhase();//calling next phase for this view


            }
            //disabling loading screen          
            CustomizeManager.Instance.ToggleCustomize_P1_UI(true);
            
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void SpawnLoadingScreenForSharedClientsServerRPC()
    {
        foreach (ulong clientId in sharedClients)
        {
            SpawnLoadingScreenForClientRpc(clientId);
            Debug.Log("Spawning loading screen for client: " + clientId);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void SpawnLoadingScreenForClientRpc(ulong clientId)
    {
        if (IsServer)
        {
            return;
        }

        if (NetworkManager.LocalClientId == clientId)
        {
            Debug.Log("Spawning text for client "+clientId);
            LoadingManager.Instance.SetLoadingText("Waiting for host confirm the choice of layout. ");
            LoadingManager.Instance.EnableLoadingScreen();
            CustomizeManager.Instance.ToggleCustomize_P1_UI(false);
            if(CustomizeManager.Instance.SharedPhase== CustomizeManager.CustomizePhase.Customize_layout)
            {
                if (CustomizeManager.Instance.isCurrentLayoutManagerShared)
                {
                    CustomizeManager.Instance.SharedLayoutManager().DespawnAllRooms();
                }
                else
                {
                    CustomizeManager.Instance.PrivateLayoutManager().DespawnAllRooms();
                }

                if (currentItem != null)
                {
                    Destroy(currentItem.gameObject);
                }
            }
        }
    }
    [ServerRpc(RequireOwnership =false)]
    public void SelectLayoutDialogServerRPC()
    {
        CustomizeManager.Instance.ToggleCustomize_P1_UI(false);
        selectLayoutDialog();
    }
    public async Task selectLayoutDialog()
    {
        DialogButtonType answer = await DialogManager.Instance.SpawnDialogWithAsync("Layout selected!", "Would you like to confirm the choice ?", "YES", "NO");

        if (answer == DialogButtonType.Positive)
        {
            selectedIndex.Value = currentIndex.Value;
            answered.Value = true;

        }
        else
        {
            selectedIndex.Value = -1;
            answered.Value = true;
        }
               
    }


    private void DespawnLoadingScreenForClients()
    {
        foreach (ulong clientId in sharedClients)
        {
            DespawnLoadingScreenForClientsClientRPC(clientId);
            Debug.Log("Spawning Dialog for client: " + clientId);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void DespawnLoadingScreenForClientsClientRPC(ulong clientId)
    {
        if (IsServer)
        {
            return;
        }

        if (NetworkManager.LocalClientId == clientId)
        {
            LoadingManager.Instance.DisableLoadingScreen();
        }
    }

    public void SetSharedItemsForClients(int index)
    {
        Debug.Log("IN SetSharedItemsForClients");
        foreach (ulong clientId in sharedClients)
        {
            Debug.Log("iN FIRST Setting shared itmes for client: " + clientId);
            SetSharedItemsForClientsClientRPC(clientId,index);
        }
    }


    [ClientRpc(RequireOwnership = false)]
    public void SetSharedItemsForClientsClientRPC(ulong clientId, int index)
    {
        Debug.Log("IN SECOND OUT OF IF ");

        Debug.Log("LOCAL CLIENT ID"+NetworkManager.LocalClientId);

        if (NetworkManager.LocalClientId == clientId)
        {
            Debug.Log("IN SECOND Setting shared itmes for client: " + clientId);
            CustomizeManager.Instance.selectedModuleRoomLayouts = CustomizeManager.Instance.currModule.roomLayouts[index].roomLayouts; 
        }
    }
}

