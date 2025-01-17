using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TooltipController : MonoBehaviour
{
    [SerializeField] private GameObject tooltip;
    private bool tooltipToggled = false;
    private Coroutine gazeCoroutine = null;

    // Start is called before the first frame update
    void Start()
    {
        tooltip.SetActive(false); // Make sure the tooltip is initially hidden

    }

    // Update is not necessary for gaze, so we're removing it.
    // We are using the events to control tooltip toggling.

    /// <summary>
    /// Triggered when the attached StatefulInteractable enters eye gaze.
    /// Starts the countdown to toggle the tooltip.
    /// </summary>
    public void OnGazeHoverEntered()
    {
        // If there's already a running coroutine, stop it (to avoid multiple coroutines running)
        if (gazeCoroutine != null)
        {
            StopCoroutine(gazeCoroutine);
        }

        // Start the countdown coroutine
        gazeCoroutine = StartCoroutine(TooltipCountdown());
    }

    /// <summary>
    /// Triggered when the attached StatefulInteractable leaves eye gaze.
    /// Stops the countdown and hides the tooltip.
    /// </summary>
    public void OnGazeHoverExited()
    {
        // Stop the tooltip countdown if it's running
        if (gazeCoroutine != null)
        {
            StopCoroutine(gazeCoroutine);
            gazeCoroutine = null;
        }

        // Hide the tooltip immediately
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(TooltipDismissCountdown());
        }
        else
        {
            Debug.LogWarning("Attempted to start exit coroutine, but GameObject is inactive.");
        }

        tooltipToggled = false;
    }

    // Coroutine to handle the 2-second countdown before showing the tooltip
    private IEnumerator TooltipCountdown()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Toggle the tooltip after the delay
        tooltipToggled = !tooltipToggled;
        tooltip.SetActive(tooltipToggled);
    }
    private IEnumerator TooltipDismissCountdown()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(2f);

        // Toggle the tooltip after the delay
        tooltip.SetActive(false);
        tooltip.SetActive(true);
        tooltip.SetActive(false);
    }

    public void SetTooltip(GameObject tooltip)
    {
        this.tooltip = tooltip;
    }

}
