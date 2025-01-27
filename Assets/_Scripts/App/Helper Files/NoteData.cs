using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static BuildingModelScriptable;

[Serializable]

public class NoteData : INetworkSerializable, IEquatable<NoteData>
{
    public string header;
    public string text;
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
    public NoteData(GameObject note,string header,string text)
    {
        this.header = header;
        this.text = text;
        // Initialize position and rotation
        positionX = note.transform.position.x;
        positionY = note.transform.position.y;
        positionZ = note.transform.position.z;

        rotationX = note.transform.rotation.eulerAngles.x;
        rotationY = note.transform.rotation.eulerAngles.y;
        rotationZ = note.transform.rotation.eulerAngles.z;

        // Initialize position and rotation
        scaleX = note.transform.localScale.x;
        scaleY = note.transform.localScale.y;
        scaleZ = note.transform.localScale.z;

    }

    // Network serialization method
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serialize each value
        serializer.SerializeValue(ref header);
        serializer.SerializeValue(ref text);
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
    public bool Equals(NoteData other)
    {
        // Compare all relevant fields for equality
        return header == other.header &&
               text == other.text &&
               positionX.Equals(other.positionX) &&
               positionY.Equals(other.positionY) &&
               positionZ.Equals(other.positionZ) &&
               rotationX.Equals(other.rotationX) &&
               rotationY.Equals(other.rotationY) &&
               rotationZ.Equals(other.rotationZ) &&
               positionX.Equals(other.scaleX) &&
               positionY.Equals(other.scaleY) &&
               positionZ.Equals(other.scaleZ);
    }

    public override bool Equals(object obj)
    {
        if (obj is NoteData other)
            return Equals(other);
        return false;
    }

}
