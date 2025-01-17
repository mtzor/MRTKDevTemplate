using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RatingIndicator : MonoBehaviour
{
    [SerializeField] private GameObject[] rateBtnImages;

    private int  currrRating=0;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Rate(int i)
    {
        for (int j = 0; j < rateBtnImages.Length; j++)
        {
            if (j < i)
            {
                rateBtnImages[j].gameObject.SetActive(true);
            }
            else
            {
                rateBtnImages[j].gameObject.SetActive(false);
            }
        }

        currrRating = i;

        VisualizeManager.Instance.View.RateCurrentDesign(currrRating);
    }

    public int GetRating() { return currrRating; }

}
