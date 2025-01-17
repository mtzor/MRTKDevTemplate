using Microsoft.MixedReality.OpenXR;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

[RequireComponent(typeof(ObjectManipulator))]
public class ObjectRotator : MonoBehaviour
{
    public GameObject objectToRotate;

    public void OnManipulationStarted()
    {

    }
    public void OnManipulationEnded()
    {
       
        // Snap rotation to 90 degrees along the Y-axis
        Vector3 currentRotation = transform.localEulerAngles;
        transform.rotation = Quaternion.Euler(new Vector3(0, Mathf.Round(currentRotation.y / 90) * 90f, 0));
        Debug.Log("Manipulation Ended: " + Mathf.Round(currentRotation.y / 90) * 90);
        
    }
}
