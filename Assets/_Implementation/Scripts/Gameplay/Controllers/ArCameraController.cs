using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ArCameraController : MonoBehaviour
{

    // CloudReco detection handler
    public ARCloudTagDetectionController CloudRecoDetectionHandler;

    // AR Camera transition image animation
    public Animation ARCameraTransitionAnim;

    // AR Camera exit button
    public GameObject ARCameraExitButton;

    // AR Camera torch button
    public GameObject ARCameraTorchButton;

    // Torch active flag
    public bool TorchActive = false;

    // Waiting for initialization
    IEnumerator Start()
    {
        this.GetComponent<Camera>().cullingMask = 0;
        this.GetComponent<Camera>().depth = 0;
        yield return new WaitUntil(() => this.transform.childCount > 0);
        for (int i = 0; i < this.transform.childCount; i++)
            this.transform.GetChild(i).gameObject.layer = LayerMask.NameToLayer("AR");
        
        yield return new WaitUntil(() => CameraDevice.Instance.IsActive());
        CameraDevice.Instance.Stop();
    }

    // Opening camera
    public void Open(Deposit deposit)
    {
        StartCoroutine(ARCameraTransition(true, deposit));

    }

    // Closing camera
    public void Close()
    {
        StartCoroutine(ARCameraTransition(false));
    }

    // Toggling torch mode
    public void ToggleTorch()
    {
        if (CameraDevice.Instance.IsActive())
        {
            TorchActive = !TorchActive;
            CameraDevice.Instance.SetFlashTorchMode(TorchActive);
            ARCameraTorchButton.GetComponent<CanvasGroup>().alpha = TorchActive ? 1f : 0.5f;
        }
    }

    // Transition Coroutine
    private IEnumerator ARCameraTransition(bool open, Deposit deposit = null)
    {
        if (open)
        {
            CameraDevice.Instance.Start();
        }
        else
        {
            TorchActive = false;
            CameraDevice.Instance.SetFlashTorchMode(TorchActive);
            ARCameraTorchButton.GetComponent<CanvasGroup>().alpha = TorchActive ? 1f : 0.5f;
            CameraDevice.Instance.Stop();
        }

        ARCameraTransitionAnim.Play("ARCameraTransitionImageIn");
        yield return new WaitWhile(() => ARCameraTransitionAnim.isPlaying);

        if (open)
        {
            yield return new WaitUntil(() => CameraDevice.Instance.IsActive());
        }
        else
        {
            yield return new WaitUntil(() => !CameraDevice.Instance.IsActive());
        }

        if (open)
        {
            this.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("UI");
            this.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("AR");
            this.GetComponent<Camera>().depth = 2;
            if (CloudRecoDetectionHandler && deposit != null)
                CloudRecoDetectionHandler.StartSearching(deposit);

            ARCameraExitButton.SetActive(true);
            ARCameraTorchButton.SetActive(true);
        }
        else
        {
            this.GetComponent<Camera>().cullingMask = 0;
            this.GetComponent<Camera>().depth = 0;
            if (CloudRecoDetectionHandler)
                CloudRecoDetectionHandler.StopSearching();

            ARCameraExitButton.SetActive(false);
            ARCameraTorchButton.SetActive(false);
        }

        if (DownButtonController.instance.standardMenuButtonsVisible)
            DownButtonController.instance.ToggleStandardMenu();

        ARCameraTransitionAnim.Play("ARCameraTransitionImageOut");
    }

}
