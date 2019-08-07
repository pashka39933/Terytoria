using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ClosedDepositPopup : MonoBehaviour
{

    public static ClosedDepositPopup instance;
    ClosedDepositPopup() { instance = this; }

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

    private DepositController depositController;

    public Text depositName;
    public Image depositPreview;
    public Button scanButton;
    public ArCameraController arCamera;

    public void Open(Deposit deposit)
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        StartCoroutine(UpdateCoroutine(DepositsController.instance.deposits.FindIndex(x => x.depositData.id == deposit.id)));

        this.GetComponent<Animation>().Play("popupIn");
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

    IEnumerator UpdateCoroutine(int depositIndex)
    {
        while (true)
        {
            depositController = DepositsController.instance.deposits[depositIndex];
            UpdatePopup();
            yield return new WaitForEndOfFrame();
            if (Input.GetKey(KeyCode.Escape))
                Close();
        }

    }

    private void UpdatePopup()
    {
        depositName.text = depositController.depositData.name;

        if (depositController.previewSprite != null)
        {
            float spriteAspect = depositController.previewSprite.rect.size.y / depositController.previewSprite.rect.size.x;
            depositPreview.sprite = depositController.previewSprite;
            depositPreview.GetComponent<RectTransform>().sizeDelta = new Vector2(depositPreview.GetComponent<RectTransform>().sizeDelta.x, depositPreview.GetComponent<RectTransform>().sizeDelta.x * spriteAspect);
            depositPreview.GetComponent<CanvasGroup>().alpha = 1;
        }
        else
        {
            depositPreview.sprite = null;
            depositPreview.GetComponent<CanvasGroup>().alpha = 0;
        }

        scanButton.onClick.RemoveAllListeners();
        scanButton.onClick.AddListener(() =>
        {
            arCamera.Open(depositController.depositData);
            Close();
        });
    }

}