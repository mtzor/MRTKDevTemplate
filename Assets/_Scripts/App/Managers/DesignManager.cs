using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using static Microsoft.MixedReality.GraphicsTools.MeshInstancer;

public class DesignManager : MonoBehaviour
{
    private static DesignManager _instance;
    [SerializeField] private Transform buildingPrefab;
    [SerializeField] private Transform container;


    private Transform _building;
    private int floorCount = 0;

    // Singleton pattern
    public static DesignManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<DesignManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach DesignManager if not found
                    GameObject designManagerObject = new GameObject("DesignManager");
                    _instance = designManagerObject.AddComponent<DesignManager>();
                }
            }

            return _instance;
        }
    }

    public int FloorCount {
        set => floorCount = value; get => (int) floorCount; 
    }
 
    void OnDestroy() // Unsubscribe from event when UIManager is destroyed
    {
        if (AppManager.Instance != null)
        {
            // AppManager.Instance.OnAppPhaseChanged -= UpdateAppPhaseEvent;
        }
    }

    public void PlaceBuilding()
    {
        if (_building == null)
        {
            Transform buildingTransform = Instantiate(buildingPrefab, container);
            buildingTransform.gameObject.SetActive(true);
        }
    }

        

 }
