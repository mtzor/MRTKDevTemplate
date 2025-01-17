using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;


[Serializable]
public class RoomInfo: INetworkSerializable, IEquatable<RoomInfo>
{
     public int roomId; // The module name
     public int layoutIndex; // The index in the layouts list
     public int customizationIndex; // The index in the customization list (same as roomLayouts)

     public RoomInfo(int roomId, int layoutIndex, int customizationIndex)
     {
         this.roomId = roomId;
         this.layoutIndex = layoutIndex;
         this.customizationIndex = customizationIndex;
     }


    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Serialize each value
        serializer.SerializeValue(ref roomId);
        serializer.SerializeValue(ref layoutIndex);
        serializer.SerializeValue(ref customizationIndex);
    }
    // Implementing IEquatable<ModuleData>
    public bool Equals(RoomInfo other)
    {
        // Compare all relevant fields for equality
        return roomId == other.roomId &&
               layoutIndex.Equals(other.layoutIndex) &&
               customizationIndex == other.customizationIndex;
    }

    public override bool Equals(object obj)
    {
        if (obj is ModuleData other)
            return Equals(other);
        return false;
    }
}
