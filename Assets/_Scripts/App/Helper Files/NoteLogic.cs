using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MixedReality.Toolkit.UX;
public class NoteLogic : MonoBehaviour
{

    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text mainText;

    [SerializeField] private PressableButton confirmBtn;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetHeader(string text)
    {
        headerText.text = text;
    }

    public void SetMainText(string text)
    {
        mainText.text = text;
    }

    public void OnConfirmBtnPressed()
    {
        this.gameObject.SetActive(false);
        NoteManager.Instance.SaveNote(this.gameObject,headerText.text,mainText.text);
    }
}
