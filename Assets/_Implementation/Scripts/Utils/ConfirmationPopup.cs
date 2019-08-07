using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmationPopup : MonoBehaviour
{

    public static ConfirmationPopup instance;
    ConfirmationPopup() { instance = this; }

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

    public void Open(UnityAction btn1Action = null, UnityAction btn2Action = null, string title = "")
    {
        UIController.instance.uiActive = true;

        if (btn1Action != null && this.GetComponentsInChildren<Button>().Length > 0)
        {
            this.GetComponentsInChildren<Button>()[0].onClick.RemoveAllListeners();
            this.GetComponentsInChildren<Button>()[0].onClick.AddListener(btn1Action);
        }
        if (btn2Action != null && this.GetComponentsInChildren<Button>().Length > 1)
        {
            this.GetComponentsInChildren<Button>()[1].onClick.RemoveAllListeners();
            this.GetComponentsInChildren<Button>()[1].onClick.AddListener(btn2Action);
        }
        if (title.Length > 0 && this.GetComponentsInChildren<Text>().Length > 0)
        {
            this.GetComponentsInChildren<Text>()[0].text = title;
        }
        this.GetComponent<Animation>().Play("popupIn");

        StartCoroutine(UpdateCoroutine());
    }

    public void Close()
    {
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
