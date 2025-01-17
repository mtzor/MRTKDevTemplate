using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class RoomData : INetworkSerializable, IEquatable<RoomData>
{
    public int roomID;

    // Individual position components
    public float positionX;
    public float positionY;
    public float positionZ;

    // Individual rotation components
    public float rotationX;
    public float rotationY;
    public float rotationZ;

    public float scaleX;
    public float scaleY;
    public float scaleZ;


    // Default constructor (required for deserialization)
    public RoomData() { }
    // Constructor to initialize ModuleData from a GameObject
    public RoomData(GameObject room)
    {
        // Initialize position and rotation
        positionX = room.transform.position.x;
        positionY = room.transform.position.y;
        positionZ = room.transform.position.z;

        rotationX = room.transform.rotation.eulerAngles.x;
        rotationY = room.transform.rotation.eulerAngles.y;
        rotationZ = room.transform.rotation.eulerAngles.z;

        // Initialize position and rotation
        scaleX = room.transform.localScale.x;
        scaleY = room.transform.localScale.y;
        scaleZ = room.transform.localScale.z;

        // Module ID
        var roomComponent = room.GetComponent<Room>();
        roomID = roomComponent?.roomId ?? -1;

    }

    // Network serialization method
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serialize each value
        serializer.SerializeValue(ref roomID);
        serializer.SerializeValue(ref positionX);
        serializer.SerializeValue(ref positionY);
        serializer.SerializeValue(ref positionZ);
        serializer.SerializeValue(ref rotationX);
        serializer.SerializeValue(ref rotationY);
        serializer.SerializeValue(ref rotationZ);
        serializer.SerializeValue(ref scaleX);
        serializer.SerializeValue(ref scaleY);
        serializer.SerializeValue(ref scaleZ);

    }

    // Implementing IEquatable<ModuleData>
    public bool Equals(RoomData other)
    {
        // Compare all relevant fields for equality
        return roomID == other.roomID &&
               positionX.Equals(other.positionX) &&
               positionY.Equals(other.positionY) &&
               positionZ.Equals(other.positionZ) &&
               rotationX.Equals(other.rotationX) &&
               rotationY.Equals(other.rotationY) &&
               rotationZ.Equals(other.rotationZ)&&
               positionX.Equals(other.scaleX) &&
               positionY.Equals(other.scaleY) &&
               positionZ.Equals(other.scaleZ) ;
    }

    public override bool Equals(object obj)
    {
        if (obj is RoomData other)
            return Equals(other);
        return false;
    }


    public bool IsAtSamePosition(Vector3 position, float tolerance = 0.01f)
    {
        return Mathf.Abs(positionX - position.x) <= tolerance &&
               Mathf.Abs(positionY - position.y) <= tolerance &&
               Mathf.Abs(positionZ - position.z) <= tolerance;
    }
}
