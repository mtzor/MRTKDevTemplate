using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GridData : MonoBehaviour
{
    Dictionary<Vector3Int, PlacementData> placedObjects = new();

    public void AddObjectAt(Vector3Int gridosition, Vector2Int objectSize,int ID, int placedObjectIndex)
    {
        List<Vector3Int> positionToOccupy= CalculatePositions(gridosition,objectSize);
        PlacementData data = new PlacementData(positionToOccupy,ID,placedObjectIndex);
        foreach(var position in positionToOccupy)
        {
            if(placedObjects.ContainsKey(position))
            {
                throw new Exception("Dictionary already contains position" + position);
            }
        }
    }

    public bool CanPlaceObjectAt(Vector3Int gridPosition, Vector2Int objectSize)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize);
        //Debug.Log("Can place object");


        foreach (var position in positionToOccupy)
        {
            if (placedObjects.ContainsKey(position))
            {
                Debug.Log("Cant place object");
                return false;
            }
        }

        return true;
    }
    private List<Vector3Int> CalculatePositions(Vector3Int gridosition, Vector2Int objectSize)
    {
        List<Vector3Int> retval = new();
        for(int x = 0; x< objectSize.x; x++)
        {
            for(int y = 0; y< objectSize.y; y++)
            {
                retval.Add(gridosition + new Vector3Int(x,0,y));
            }
        }
        return retval;

    }
}

public class PlacementData
{
    public List<Vector3Int> occupiedPositions;
    public int ID { get; private set; }

    public int PlacedObjectIndex {  get; private set; }

    public PlacementData(List<Vector3Int> occupiedPositions, int iD, int placedObjectIndex)
    {
        this.occupiedPositions = occupiedPositions;
        ID = iD;
        PlacedObjectIndex = placedObjectIndex;
    }
}