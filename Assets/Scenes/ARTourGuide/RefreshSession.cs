using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Reloads the ARSession by first destroying the ARSession's GameObject
/// and then instantiating a new ARSession from a Prefab.
/// </summary>
public class RefreshSession : MonoBehaviour
{
    public ARSession session;
    public GameObject sessionPrefab;

    public void ReloadSession()
    {
        if (session != null)
        {
            StartCoroutine(DoReload());
        }
    }

    IEnumerator DoReload()
    {
        Destroy(session.gameObject);
        yield return null;

        if (sessionPrefab != null)
        {
            session = Instantiate(sessionPrefab).GetComponent<ARSession>();
        }

    }
}
