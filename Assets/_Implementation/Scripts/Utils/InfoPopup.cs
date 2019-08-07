using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InfoPopup : MonoBehaviour {

    public static InfoPopup instance;
    InfoPopup() { instance = this; }

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

    public void OpenWithoutAction(string title = "")
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        if (this.GetComponentsInChildren<Button>().Length > 0)
        {
            this.GetComponentsInChildren<Button>()[0].onClick.RemoveAllListeners();
            this.GetComponentsInChildren<Button>()[0].onClick.AddListener(() => { Close(); });
        }
        if (title.Length > 0 && this.GetComponentsInChildren<Text>().Length > 0)
        {
            this.GetComponentsInChildren<Text>()[0].text = title.Replace("[n]", System.Environment.NewLine);
        }
        this.GetComponent<Animation>().Play("popupIn");

        StartCoroutine(UpdateCoroutine());
    }

    public void Open(UnityAction btn1Action = null, string title = "")
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        if (btn1Action != null && this.GetComponentsInChildren<Button>().Length > 0)
        {
            this.GetComponentsInChildren<Button>()[0].onClick.RemoveAllListeners();
            this.GetComponentsInChildren<Button>()[0].onClick.AddListener(btn1Action);
        }
        if (title.Length > 0 && this.GetComponentsInChildren<Text>().Length > 0)
        {
            this.GetComponentsInChildren<Text>()[0].text = title.Replace("[n]", System.Environment.NewLine);
        }
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
