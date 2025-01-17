using System;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct ModuleData : INetworkSerializable, IEquatable<ModuleData>
{
    public int moduleID;

    // Individual position components
    public float positionX;
    public float positionY;
    public float positionZ;

    // Individual rotation components
    public float rotationX;
    public float rotationY;
    public float rotationZ;

    // Change ownerID to an integer
    public int ownerID; // Now using int instead of char array

    // Constructor to initialize ModuleData from a GameObject
    public ModuleData(GameObject module)
    {
        // Initialize position and rotation
        positionX = module.transform.position.x;
        positionY = module.transform.position.y;
        positionZ = module.transform.position.z;

        rotationX = module.transform.rotation.eulerAngles.x;
        rotationY = module.transform.rotation.eulerAngles.y;
        rotationZ = module.transform.rotation.eulerAngles.z;

        // Module ID
        var moduleComponent = module.GetComponent<Module>();
        moduleID = moduleComponent?.moduleData?.moduleID ?? -1;

        ownerID = LobbyManager.Instance.GetPlayerID();
    }

    // Network serialization method
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serialize each value
        serializer.SerializeValue(ref moduleID);
        serializer.SerializeValue(ref positionX);
        serializer.SerializeValue(ref positionY);
        serializer.SerializeValue(ref positionZ);
        serializer.SerializeValue(ref rotationX);
        serializer.SerializeValue(ref rotationY);
        serializer.SerializeValue(ref rotationZ);

        // Serialize ownerID as an integer
        serializer.SerializeValue(ref ownerID);
    }

    // Implementing IEquatable<ModuleData>
    public bool Equals(ModuleData other)
    {
        // Compare all relevant fields for equality
        return moduleID == other.moduleID &&
               positionX.Equals(other.positionX) &&
               positionY.Equals(other.positionY) &&
               positionZ.Equals(other.positionZ) &&
               rotationX.Equals(other.rotationX) &&
               rotationY.Equals(other.rotationY) &&
               rotationZ.Equals(other.rotationZ) &&
               ownerID == other.ownerID;
    }

    public override bool Equals(object obj)
    {
        if (obj is ModuleData other)
            return Equals(other);
        return false;
    }


}
