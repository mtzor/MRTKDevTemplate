using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MixedReality.Toolkit.UX;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] PressableButton cancelTutorialButton;
    private static TutorialManager _instance;

    public GameObject cube;

    private bool handDetected;
    private bool handRemoved;
    private bool menuManipulated;
    private bool menuToggled;
    private bool menuClosed;
    private bool sliderUpdated;
    public bool cubeMoved;
    public bool cubeRotated;

    private CancellationTokenSource _cancellationTokenSource;
    private bool tutorialPaused = false;
    private string lastDialogueTitle;       // Stores the last dialogue's title
    private string lastDialogueMessage;     // Stores the last dialogue's message

    private Quaternion initialRotstion;
    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TutorialManager>();

                if (_instance == null)
                {
                    GameObject tutorialManagerObject = new GameObject("TutorialManager");
                    _instance = tutorialManagerObject.AddComponent<TutorialManager>();
                }
            }

            return _instance;
        }
    }
    public void CheckCubeRotated()
    {
        if(cube.transform.rotation!= initialRotstion)
        {
            CubeRotated = true;
        }
    }
    void Start()
    {
        cancelTutorialButton.gameObject.SetActive(false);
        cube.SetActive(false);

        cancelTutorialButton.OnClicked.AddListener(() => { cancelTutorial(); });
        initialRotstion = cube.transform.localRotation;
        //runTutorial();
    }

    private async void cancelTutorial()
    {
        tutorialPaused = true;

        int result = await cancelTutorialAsync();

        if (result == 1)
        {
            Debug.Log("Cancel confirmed.");
            _cancellationTokenSource?.Cancel();
        }
        else
        {
            Debug.Log("Resuming tutorial.");
            tutorialPaused = false;

            // Re-enable the cancel button upon resuming
            cancelTutorialButton.gameObject.SetActive(true);

            // Show the last dialogue before the pause
            if (!string.IsNullOrEmpty(lastDialogueTitle) && !string.IsNullOrEmpty(lastDialogueMessage))
            {
                DialogManager.Instance.SpawnNeutralDialogFromCode(lastDialogueTitle, lastDialogueMessage);
            }
        }
    }

    private async Task<int> cancelTutorialAsync()
    {
        DialogButtonType result = await DialogManager.Instance.SpawnDialogWithAsync(
            "Cancel Tutorial",
            "Do you want to leave the tutorial?",
            "Yes, Leave",
            "No, Continue"
        );

        return result == DialogButtonType.Positive ? 1 : 0;
    }

    public async Task<int> runTutorial()
    {
        cancelTutorialButton.gameObject.SetActive(true);

        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _cancellationTokenSource.Token;

        try
        {
            await SpawnAndSaveDialog(
                "Welcome to the AR-Apt application tutorial!",
                "In this tutorial, we will show you the main gestures needed to navigate the app. It's fairly easy!",
                "PRESS TO CONTINUE",
                token
            );

            cancelTutorialButton.gameObject.SetActive(true);

            await SpawnAndSaveNeutralDialog("Let's view the Hand Menu.",
                "Raise your hand and look at your flat palm. You may need to move your hands out of view then back into view for the toggled menu to appear.",
                token);

            await WaitForCondition(() => handDetected, token);

            await SpawnAndSaveNeutralDialog("Well done!",
                "This is the Hand Menu. Remove your hand, and it will remain on your view.",
                token);

            await WaitForCondition(() => handRemoved, token);

            await SpawnAndSaveNeutralDialog("Let's learn our first gesture: The pinch Gesture!",
                "We will try pinching to move the HandMenu.\r\n 1. Hold up your arm and connect your pointer with your thumb to grab the bar on the bottom of the HandMenu.\r\n " +
                "2.Move and let go, to place it in your view. Give it a try now!", token);

            await WaitForCondition(() => menuManipulated, token);

            await SpawnAndSaveNeutralDialog("Moving on. Let's explore some features of the hand menu.",
                "Press the scene helper toggle!",
                token);

            await WaitForCondition(() => menuToggled, token);

            await Task.Delay(2000, token);

            await SpawnAndSaveNeutralDialog("The scene helper is there to guide you throughout the app.",
                "You can turn it off now.",
                token);

            await WaitForCondition(() => !menuToggled, token);

            await SpawnAndSaveNeutralDialog("Well done!", "You can now close the hand menu.", token);

            await WaitForCondition(() => menuClosed, token);

            await SpawnAndSaveNeutralDialog("Well done! Let's now see far interaction.",
                "When an object is not close enough, you can interact with it by:\r\n 1. Pointing at it with your index finger \r\n2. Pinching \r\n Try to move this cube from afar.",
                token);

            cube.SetActive(true);

            await WaitForCondition(() => cubeMoved, token);


            await SpawnAndSaveNeutralDialog("Well done! Let's rotate the cube now.",
                " 1. Point at hte object with your index finger \r\n2. Pinch and rotate your hand.",
                token);

            await WaitForCondition(() => cubeRotated, token);

            await SpawnAndSaveDialog(
                "Well done! The tutorial is now complete.",
                "You can view the tutorial at any time by using the corresponding hand menu button",
                "OK",
                token
            );

            cube.SetActive(false);
            cancelTutorialButton.gameObject.SetActive(false);

            AppManager.Instance.TutorialComplete();
            return 1;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Tutorial was canceled.");
            AppManager.Instance.TutorialComplete();
            return 0;
        }
    }

    private async Task SpawnAndSaveDialog(string title, string message, string buttonText, CancellationToken token)
    {
        lastDialogueTitle = title;
        lastDialogueMessage = message;

        await DialogManager.Instance.SpawnDialogWithAsync(title, message, buttonText);
    }

    private async Task SpawnAndSaveNeutralDialog(string title, string message, CancellationToken token)
    {
        lastDialogueTitle = title;
        lastDialogueMessage = message;

        DialogManager.Instance.SpawnNeutralDialogFromCode(title, message);
        await Task.Delay(500); // Short delay to allow the dialogue to fully load
    }

    private async Task WaitForCondition(Func<bool> condition, CancellationToken token)
    {
        while (!condition())
        {
            token.ThrowIfCancellationRequested();

            if (tutorialPaused)
            {
                await Task.Delay(100);
                continue;
            }

            await Task.Delay(500, token);
        }
    }

    // Properties for tracking tutorial events
    public bool HandDetected { get => handDetected; set => handDetected = value; }
    public bool HandRemoved { get => handRemoved; set => handRemoved = value; }
    public bool MenuManipulated { get => menuManipulated; set => menuManipulated = value; }
    public bool MenuToggled { get => menuToggled; set => menuToggled = value; }
    public bool MenuClosed { get => menuClosed; set => menuClosed = value; }
    public bool SliderUpdated { get => sliderUpdated; set => sliderUpdated = value; }
    public bool CubeMoved { get => cubeMoved; set => cubeMoved = value; }

    public bool CubeRotated {  get => cubeRotated; set => cubeRotated = value; }
}
