using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

/// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
/// and overlays some information as well as the source Texture2D on top of the
/// detected image.
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class QRCodeInfoManager : MonoBehaviour
{
    public Button playPauseButton;
    public Button resetButton;
    public static bool inSession;
    bool firstSession;
    ARTrackedImageManager m_TrackedImageManager;

    /// <summary>
    /// Invoked whenever QRCode is scanned.
    /// </summary>
    public static event Action onCodeScanned;

    /// <summary>
    /// Invoked whenever session is reset.
    /// </summary>
    public static event Action onResetSession;

    void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void EnablePlaneAndGuide(ARTrackedImage trackedImage)
    {
        if (!inSession)
        {
            inSession = true;
            onCodeScanned();
            playPauseButton.interactable = true;
            resetButton.interactable = true;
            if (firstSession)
            {
                gameObject.GetComponent<ARPlaneManager>().enabled = true;
                gameObject.GetComponent<ARPointCloudManager>().enabled = true;
                gameObject.GetComponent<ARRaycastManager>().enabled = true;
                gameObject.GetComponent<TourGuideManager>().enabled = true;
                firstSession = false;
            }
            gameObject.GetComponent<TourGuideManager>().SetQRCode(trackedImage);
        }
    }

    public void ResetSession()
    {
        if (inSession)
        {
            inSession = false;
            playPauseButton.interactable = false;
            resetButton.interactable = false;
            gameObject.GetComponent<TourGuideManager>().ResetTourGuide();
            onResetSession();
        }
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        //only allow one image to be scanned at one time
        foreach (var trackedImage in eventArgs.added)
        {
            EnablePlaneAndGuide(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            EnablePlaneAndGuide(trackedImage);
        }
    }
}
