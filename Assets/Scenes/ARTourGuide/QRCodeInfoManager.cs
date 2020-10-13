using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
/// and overlays some information as well as the source Texture2D on top of the
/// detected image.
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class QRCodeInfoManager : MonoBehaviour
{
    ARTrackedImageManager m_TrackedImageManager;

    /// <summary>
    /// Invoked whenever an object is placed in on a plane.
    /// </summary>
    public static event Action onCodeScanned;

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
        gameObject.GetComponent<ARPlaneManager>().enabled = true;
        gameObject.GetComponent<ARPointCloudManager>().enabled = true;
        gameObject.GetComponent<ARRaycastManager>().enabled = true;
        gameObject.GetComponent<TourGuideManager>().enabled = true;
        gameObject.GetComponent<TourGuideManager>().SetQRCode(trackedImage);
        Debug.Log(trackedImage.name);
        onCodeScanned();
    }


    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            EnablePlaneAndGuide(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
            EnablePlaneAndGuide(trackedImage);
    }
}
