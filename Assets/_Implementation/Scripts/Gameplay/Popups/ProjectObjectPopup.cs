using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ProjectObjectPopup : MonoBehaviour
{

    public static ProjectObjectPopup instance;
    ProjectObjectPopup() { instance = this; }

    bool opened = false;

    private void Awake()
    {
        this.transform.localScale = Vector3.zero;
        if (this.GetComponent<CanvasGroup>() != null)
        {
            this.GetComponent<CanvasGroup>().alpha = 0;
            this.GetComponent<CanvasGroup>().blocksRaycasts = false;
            this.GetComponent<CanvasGroup>().interactable = false;
        }
    }

    public void Open(ProjectObject projectObject)
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        this.GetComponent<Animation>().Play("popupIn");

        StartCoroutine(UpdateCoroutine());
    }

    public void Close()
    {
        if (!opened)
            return;
        opened = false;

        UIController.instance.uiActive = false;

        StopAllCoroutines();

        this.GetComponent<Animation>().Play("popupOut");
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (Input.GetKey(KeyCode.Escape))
                Close();
        }
    }

}