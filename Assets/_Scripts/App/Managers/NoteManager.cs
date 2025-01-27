using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MixedReality.Toolkit.UX;
using static AppManager;
using TMPro;
using static Microsoft.MixedReality.GraphicsTools.MeshInstancer;

public class NoteManager : MonoBehaviour
{
    private static NoteManager _instance;

    [SerializeField] private Transform notePrefab;

    [SerializeField] private GameObject keyboard;
    [SerializeField] private GameObject inputField;

    [SerializeField] private TMP_InputField noteText;



    private bool textEntered = false;
    public static NoteManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<NoteManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject noteManagerObject = new GameObject("NoteManager");
                    _instance = noteManagerObject.AddComponent<NoteManager>();
                }
            }

            return _instance;
        }
    }
    public async Task<DialogButtonType> LeaveNoteDialog()
    {
        DialogButtonType result = await DialogManager.Instance.SpawnDialogWithAsync("Want to leave a note ?", "", "YES", "NO");

        return result;
    }

    public async Task LeaveNoteAsync()
    {
        textEntered = false;

        DialogButtonType result = await LeaveNoteDialog();

        if (result == DialogButtonType.Positive)
        {
            NoteInputUI(true);

            while (!textEntered)
            {
                await Task.Delay(200);
            }

            Debug.Log("note TEXT" + noteText.text);
            string noteCache = noteText.text;

            Transform noteTransform= Instantiate(notePrefab);

            noteTransform.GetComponent<NoteLogic>().SetHeader(LobbyManager.Instance.GetPlayerName());

            noteTransform.GetComponent<NoteLogic>().SetMainText(noteCache);

        }
        else
        {
            return;
        }
    }

    public void NoteInputUI(bool isActive)
    {
        keyboard.SetActive(isActive);
        inputField.SetActive(isActive);
    }

    public void SetText()
    {
        textEntered = true;
    }

    public void SaveNote(GameObject note, string header, string text)
    {

    }
}
