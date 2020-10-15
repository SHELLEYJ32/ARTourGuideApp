using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Listens for touch events and performs an AR raycast from the screen touch point.
/// AR raycasts will only hit detected trackables like feature points and planes.
///
/// If a raycast hits a trackable, the <see cref="placedPrefab"/> is instantiated
/// and moved to the hit position.
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class TourGuideManager : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    AudioSource m_AudioSource;

    public AudioSource audioSource
    {
        get { return m_AudioSource; }
        set { m_AudioSource = value; }
    }

    public GameObject title;
    public GameObject description;
    public GameObject subtitle;

    ARTrackedImage QRCode;

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;

    /// <summary>
    /// Used to associate an `XRReferenceImage` with other code info by using the `XRReferenceImage`'s guid as a unique identifier for a particular reference image.
    /// </summary>
    [Serializable]
    struct QRCodeInfo
    {
        // System.Guid isn't serializable, so we store the Guid as a string. At runtime, this is converted back to a System.Guid
        public string codeGuid;
        public AudioClip codeClip;
        public string codeDescription;

        public QRCodeInfo(Guid guid, AudioClip clip, string description)
        {
            codeGuid = guid.ToString();
            codeClip = clip;
            codeDescription = description;
        }
    }

    [SerializeField]
    [HideInInspector]
    List<QRCodeInfo> m_QRCodeInfoList = new List<QRCodeInfo>();

    /// <summary>
    /// Store QRCode clip and description for dictionary value
    /// </summary>
    [Serializable]
    struct QRCodeInfoDictionaryVal
    {
        public AudioClip codeClip;
        public string codeDescription;

        public QRCodeInfoDictionaryVal(AudioClip clip, string description)
        {
            codeClip = clip;
            codeDescription = description;
        }
    }

    Dictionary<Guid, QRCodeInfoDictionaryVal> m_QRCodeInfoDictionary = new Dictionary<Guid, QRCodeInfoDictionaryVal>();

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

    [SerializeField]
    [Tooltip("Reference Image Library")]
    XRReferenceImageLibrary m_ImageLibrary;

    /// <summary>
    /// Get the <c>XRReferenceImageLibrary</c>
    /// </summary>
    public XRReferenceImageLibrary imageLibrary
    {
        get => m_ImageLibrary;
        set => m_ImageLibrary = value;
    }

    public void OnBeforeSerialize()
    {
        m_QRCodeInfoList.Clear();
        foreach (var kvp in m_QRCodeInfoDictionary)
        {
            m_QRCodeInfoList.Add(new QRCodeInfo(kvp.Key, kvp.Value.codeClip, kvp.Value.codeDescription));
        }
    }

    public void OnAfterDeserialize()
    {
        m_QRCodeInfoDictionary = new Dictionary<Guid, QRCodeInfoDictionaryVal>();
        foreach (var entry in m_QRCodeInfoList)
        {
            m_QRCodeInfoDictionary.Add(Guid.Parse(entry.codeGuid), new QRCodeInfoDictionaryVal(entry.codeClip, entry.codeDescription));
        }
    }

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

        if (m_QRCodeInfoDictionary.TryGetValue(trackedImage.referenceImage.guid, out var codeInfo))
        {
            audioSource.clip = codeInfo.codeClip;
            title.GetComponent<Text>().text = trackedImage.referenceImage.name;
            description.GetComponent<Text>().text = codeInfo.codeDescription;
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

    public AudioClip GetCodeClipForReferenceImage(XRReferenceImage referenceImage)
            => m_QRCodeInfoDictionary.TryGetValue(referenceImage.guid, out var codeVal) ? codeVal.codeClip : null;

    public string GetCodeDesForReferenceImage(XRReferenceImage referenceImage)
            => m_QRCodeInfoDictionary.TryGetValue(referenceImage.guid, out var codeVal) ? codeVal.codeDescription : null;


#if UNITY_EDITOR
    /// <summary>
    /// This customizes the inspector component and updates the prefab list when
    /// the reference image library is changed.
    /// </summary>
    [CustomEditor(typeof(TourGuideManager))]
    class TourGuideManagerInspector : Editor
    {
        List<XRReferenceImage> m_ReferenceImages = new List<XRReferenceImage>();
        bool m_IsExpanded = true;

        bool HasLibraryChanged(XRReferenceImageLibrary library)
        {
            if (library == null)
                return m_ReferenceImages.Count == 0;

            if (m_ReferenceImages.Count != library.count)
                return true;

            for (int i = 0; i < library.count; i++)
            {
                if (m_ReferenceImages[i] != library[i])
                    return true;
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
            //customized inspector
            var behaviour = serializedObject.targetObject as TourGuideManager;

            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            var libraryProperty = serializedObject.FindProperty(nameof(m_ImageLibrary));
            EditorGUILayout.PropertyField(libraryProperty);
            var library = libraryProperty.objectReferenceValue as XRReferenceImageLibrary;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AudioSource"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("title"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("description"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("subtitle"));

            //check library changes
            if (HasLibraryChanged(library))
            {
                if (library)
                {
                    var tempDictionary = new Dictionary<Guid, QRCodeInfoDictionaryVal>();
                    foreach (var referenceImage in library)
                    {
                        tempDictionary.Add(referenceImage.guid,
                            new QRCodeInfoDictionaryVal(behaviour.GetCodeClipForReferenceImage(referenceImage),
                            behaviour.GetCodeDesForReferenceImage(referenceImage)));
                    }
                    behaviour.m_QRCodeInfoDictionary = tempDictionary;
                }
            }

            // update current
            m_ReferenceImages.Clear();
            if (library)
            {
                foreach (var referenceImage in library)
                {
                    m_ReferenceImages.Add(referenceImage);
                }
            }

            //show QRCode info list
            m_IsExpanded = EditorGUILayout.Foldout(m_IsExpanded, "QRCode Info List");
            if (m_IsExpanded)
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUI.BeginChangeCheck();

                    var tempDictionary = new Dictionary<Guid, QRCodeInfoDictionaryVal>();
                    foreach (var image in library)
                    {
                        var codeClip = (AudioClip)EditorGUILayout.ObjectField(image.name, behaviour.m_QRCodeInfoDictionary[image.guid].codeClip, typeof(AudioClip), false);

                        EditorGUILayout.LabelField("Description");
                        GUIStyle myStyle = new GUIStyle(EditorStyles.textArea);
                        myStyle.wordWrap = true;
                        var codeDes = EditorGUILayout.TextArea(behaviour.m_QRCodeInfoDictionary[image.guid].codeDescription, myStyle);

                        tempDictionary.Add(image.guid, new QRCodeInfoDictionaryVal(codeClip, codeDes));
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Update QRCode Info");
                        behaviour.m_QRCodeInfoDictionary = tempDictionary;
                        EditorUtility.SetDirty(target);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}
