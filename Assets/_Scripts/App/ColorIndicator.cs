using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorIndicator : MonoBehaviour
{
    [SerializeField] private Color[] colors;

    [SerializeField] private GameObject[] avatarImages;
    public void ChooseColor(int i)
    {
        for (int j = 0; j < colors.Length; j++)
        {
            if (j == i)
            {
                avatarImages[j].SetActive(true);
                LobbyManager.Instance.SetPlayerColor(colors[i]);
            }
            else
            {
                avatarImages[j].SetActive(false);
            }
        }

    }

    private void Start()
    {
        ChooseColor(0);
    }
}
