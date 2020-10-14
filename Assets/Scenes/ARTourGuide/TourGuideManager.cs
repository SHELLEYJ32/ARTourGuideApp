using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Listens for touch events and performs an AR raycast from the screen touch point.
/// AR raycasts will only hit detected trackables like feature points and planes.
///
/// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
/// and moved to the hit position.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class TourGuideManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip blanchHallClip;
    public AudioClip dwightHallClip;
    public GameObject title;
    public GameObject description;
    public GameObject subtitle;
    ARTrackedImage QRCode;

    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject tourGuide { get; private set; }

    /// <summary>
    /// Invoked whenever an object is placed in on a plane.
    /// </summary>
    public static event Action onPlacedObject;

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            var mousePosition = Input.mousePosition;
            touchPosition = new Vector2(mousePosition.x, mousePosition.y);
            return true;
        }
#else
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
#endif

        touchPosition = default;
        return false;
    }

    void Update()
    {
        if (QRCodeInfoManager.inSession)
        {
            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                var hitPose = s_Hits[0].pose;

                if (tourGuide == null)
                {
                    tourGuide = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                    onPlacedObject?.Invoke();

                    //display texts
                    if (!title.activeSelf)
                    {
                        title.SetActive(true);
                        description.SetActive(true);
                        subtitle.SetActive(true);
                    }

                    audioSource.Play();
                }
            }
        }

    }

    public void SetQRCode(ARTrackedImage trackedImage)
    {
        QRCode = trackedImage;

        //find audio clip
        if (trackedImage.referenceImage.name == "Dwight Hall QR")
        {
            audioSource.clip = dwightHallClip;
            title.GetComponent<Text>().text = "Dwight Hall";
            description.GetComponent<Text>().text = "Dwight Hall houses the academic centers, some interdisciplinary program offices, and the College’s Archives and Special Collections.";
        }
        else if (trackedImage.referenceImage.name == "Blanchard Hall QR")
        {
            audioSource.clip = blanchHallClip;
            title.GetComponent<Text>().text = "Blanchard Hall";
            description.GetComponent<Text>().text = "Blanchard Hall is a meeting, eating, study and social place for the entire community.";
        }
    }

    public void ResetTourGuide()
    {
        Destroy(tourGuide);
        audioSource.Stop();

        if (title.activeSelf)
        {
            title.SetActive(false);
            description.SetActive(false);
            subtitle.SetActive(false);
        }
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;

}
