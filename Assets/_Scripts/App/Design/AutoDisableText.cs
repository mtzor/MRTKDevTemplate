using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AutoDisableText : MonoBehaviour
{
    [SerializeField]
    private float disableTime = 3f; // Time in seconds before disabling the text

    private float timer = 0f;

    void OnEnable()
    {
        timer = 0f; // Reset the timer when the object is enabled
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= disableTime)
        {
            gameObject.SetActive(false); // Deactivate the text object
        }
    }
}
