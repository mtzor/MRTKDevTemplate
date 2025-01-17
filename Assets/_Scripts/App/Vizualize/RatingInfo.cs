using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RatingInfo : INetworkSerializable
{
        public string roomName;
        public int[] ratings; 

        public RatingInfo(string roomName, int[] ratings)
        {
            this.roomName = roomName;
            this.ratings = ratings;
        }


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // Serialize each value
            serializer.SerializeValue(ref roomName);
            serializer.SerializeValue(ref ratings);
        }
        // Implementing IEquatable<ModuleData>
    
    }
