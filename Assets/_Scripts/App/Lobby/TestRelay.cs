using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    private static TestRelay _instance;
    public static TestRelay Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<TestRelay>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject testRelayObject = new GameObject("TestRelay");
                    _instance = testRelayObject.AddComponent<TestRelay>();
                }
            }

            return _instance;
        }
    }

    public async Task<string> CreateRelay()
    {
        try
        {
           Allocation allocation= await RelayService.Instance.CreateAllocationAsync(4);

            string joinCode= await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log("JoinCode: "+joinCode);

            RelayServerData relayServerData= new RelayServerData(allocation,"dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e.Message);
        }
        return null;
    }

    public async void JoinRelay(string JoinCode)
    {
        try
        {
           JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(JoinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch(RelayServiceException e)
        {
            Debug.LogError(e.Message);
        }
    }
}
