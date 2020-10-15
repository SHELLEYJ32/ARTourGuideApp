using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class CoachingUIManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The ARCameraManager which will produce frame events.")]
    ARCameraManager m_CameraManager;

    /// <summary>
    /// Get or set the <c>ARCameraManager</c>.
    /// </summary>
    public ARCameraManager cameraManager
    {
        get { return m_CameraManager; }
        set
        {
            if (m_CameraManager == value)
                return;

            if (m_CameraManager != null)
                m_CameraManager.frameReceived -= FrameChanged;

            m_CameraManager = value;

            if (m_CameraManager != null & enabled)
                m_CameraManager.frameReceived += FrameChanged;
        }
    }

    const string k_FadeOffAnim = "FadeOff";
    const string k_FadeOnAnim = "FadeOn";

    [SerializeField]
    ARPlaneManager m_PlaneManager;

    public ARPlaneManager planeManager
    {
        get { return m_PlaneManager; }
        set { m_PlaneManager = value; }
    }

    [SerializeField]
    Animator m_MoveDeviceAnimation;

    public Animator moveDeviceAnimation
    {
        get { return m_MoveDeviceAnimation; }
        set { m_MoveDeviceAnimation = value; }
    }

    [SerializeField]
    Animator m_TapToPlaceAnimation;

    public Animator tapToPlaceAnimation
    {
        get { return m_TapToPlaceAnimation; }
        set { m_TapToPlaceAnimation = value; }
    }

    [SerializeField]
    Animator m_ScanQRCodeAnimation;

    public Animator scanQRCodeAnimation
    {
        get { return m_ScanQRCodeAnimation; }
        set { m_ScanQRCodeAnimation = value; }
    }

    static List<ARPlane> s_Planes = new List<ARPlane>();

    bool m_ShowingScanQRCode = false;

    bool m_ShowingTapToPlace = false;

    bool m_ShowingMoveDevice = false;

    void OnEnable()
    {
        if (cameraManager != null)
            cameraManager.frameReceived += FrameChanged;
        PromptScan();
    }

    void OnDisable()
    {
        if (cameraManager != null)
            cameraManager.frameReceived -= FrameChanged;
    }

    void FrameChanged(ARCameraFrameEventArgs args)
    {
        TourGuideManager.onPlacedObject += PlacedObject;
        QRCodeInfoManager.onCodeScanned += CodeScanned;
        QRCodeInfoManager.onResetSession += PromptScan;

        if (PlanesFound() && m_ShowingMoveDevice)
        {
            if (moveDeviceAnimation)
                moveDeviceAnimation.SetTrigger(k_FadeOffAnim);

            if (tapToPlaceAnimation)
                tapToPlaceAnimation.SetTrigger(k_FadeOnAnim);

            m_ShowingTapToPlace = true;
            m_ShowingMoveDevice = false;
        }
    }

    bool PlanesFound()
    {
        if (planeManager == null)
            return false;

        return planeManager.trackables.count > 0;
    }

    void PlacedObject()
    {
        if (m_ShowingTapToPlace)
        {
            if (tapToPlaceAnimation)
                tapToPlaceAnimation.SetTrigger(k_FadeOffAnim);

            m_ShowingTapToPlace = false;
            SetAllPlanesActive(false);
        }
    }

    void CodeScanned()
    {
        if (m_ShowingScanQRCode)
        {
            if (scanQRCodeAnimation)
            {
                scanQRCodeAnimation.SetTrigger(k_FadeOffAnim);
                //TODO: fix this later
                //var scanImage = scanQRCodeAnimation.GetComponent<Image>();
                //var scanImageColor = scanImage.color;
                //scanImageColor.a = 0f;
                //scanImage.color = scanImageColor;

                //scanQRCodeAnimation.enabled = false;
            }

            if (moveDeviceAnimation)
                moveDeviceAnimation.SetTrigger(k_FadeOnAnim);

            m_ShowingScanQRCode = false;
            m_ShowingMoveDevice = true;
            SetAllPlanesActive(true);
        }
    }

    void PromptScan()
    {
        if (!m_ShowingScanQRCode)
        {
            scanQRCodeAnimation.SetTrigger(k_FadeOnAnim);
            m_ShowingScanQRCode = true;
        }
    }

    /// <summary>
    /// Iterates over all the existing planes and activates
    /// or deactivates their <c>GameObject</c>s'.
    /// </summary>
    /// <param name="value">Each planes' GameObject is SetActive with this value.</param>
    void SetAllPlanesActive(bool value)
    {
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(value);
    }
}
