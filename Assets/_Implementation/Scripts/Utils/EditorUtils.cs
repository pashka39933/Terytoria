using UnityEditor;
using UnityEngine;

public class EditorUtils : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("EditorUtils/ClearPlayerPrefs")]
    static void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared!");
    }
#endif
}
