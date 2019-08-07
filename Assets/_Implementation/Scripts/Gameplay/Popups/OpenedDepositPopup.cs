using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OpenedDepositPopup : MonoBehaviour
{

    public static OpenedDepositPopup instance;
    OpenedDepositPopup() { instance = this; }

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

    private Deposit deposit;

    public Text receiverEnergy, receiverBiomass, receiverGadgets, receiverFuel, sendEnergy, sendBiomass, sendGadgets, sendFuel;
    public InputField nickInput;
    public CanvasGroup receiveButton, sendButton;
    public ArCameraController arCamera;

    public void Open(Deposit deposit)
    {
        if (opened)
            return;
        opened = true;

        UIController.instance.uiActive = true;

        if (arCamera.TorchActive)
            arCamera.ToggleTorch();

        sendEnergy.text = "0";
        sendBiomass.text = "0";
        sendGadgets.text = "0";
        sendFuel.text = "0";
        nickInput.text = "";

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
            deposit = DepositsController.instance.deposits[depositIndex].depositData;
            UpdatePopup();
            yield return new WaitForEndOfFrame();
            if (Input.GetKey(KeyCode.Escape))
                Close();
        }

    }

    private void UpdatePopup()
    {
        receiverEnergy.text = deposit.receiverEnergy.ToString();
        receiverBiomass.text = deposit.receiverBiomass.ToString();
        receiverGadgets.text = deposit.receiverGadgets.ToString();
        receiverFuel.text = deposit.receiverFuel.ToString();

        receiveButton.alpha = (deposit.receiverEnergy + deposit.receiverBiomass + deposit.receiverGadgets + deposit.receiverFuel > 0) ? 1 : 0.5f;
        receiveButton.interactable = (deposit.receiverEnergy + deposit.receiverBiomass + deposit.receiverGadgets + deposit.receiverFuel > 0);
        receiveButton.GetComponent<Button>().interactable = (deposit.receiverEnergy + deposit.receiverBiomass + deposit.receiverGadgets + deposit.receiverFuel > 0);

        sendButton.alpha = (sendEnergy.text != "0" || sendBiomass.text != "0" || sendGadgets.text != "0" || sendFuel.text != "0") ? 1 : 0.5f;
        sendButton.interactable = (sendEnergy.text != "0" || sendBiomass.text != "0" || sendGadgets.text != "0" || sendFuel.text != "0");
        sendButton.GetComponent<Button>().interactable = (sendEnergy.text != "0" || sendBiomass.text != "0" || sendGadgets.text != "0" || sendFuel.text != "0");
    }

    public void Receive()
    {
        Deposit depositData = DepositsController.instance.deposits.Find(x => x.depositData.id == deposit.id).depositData;
        if (depositData.receiverEnergy + depositData.receiverBiomass + depositData.receiverGadgets + depositData.receiverFuel > 0)
        {
            receiveButton.alpha = 0.5f;
            receiveButton.interactable = false;
            receiveButton.GetComponent<Button>().interactable = false;
            Utils.Web.GetJSON(
                AppConstants.DepositReceiveResourcesUrl + "USER_ID=" + AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID + "&DEPOSIT_ID=" + deposit.id,
                (success, json) =>
                {
                    if (success)
                    {
                        AnalyticsController.instance.Report("DepositReceive", new Dictionary<string, object>() {
                            { "energy", depositData.receiverEnergy },
                            { "biomass", depositData.receiverBiomass },
                            { "gadgets", depositData.receiverGadgets },
                            { "fuel", depositData.receiverFuel },
                            { "depositName", depositData.name },
                            { "depositId", depositData.id }
                        });

                        ResourcesController.instance.AddEnergy(depositData.receiverEnergy);
                        ResourcesController.instance.AddBiomass(depositData.receiverBiomass);
                        ResourcesController.instance.AddGadgets(depositData.receiverGadgets);
                        ResourcesController.instance.AddFuel(depositData.receiverFuel);

                        depositData.receiverEnergy = 0;
                        depositData.receiverBiomass = 0;
                        depositData.receiverGadgets = 0;
                        depositData.receiverFuel = 0;

                        BannerController.instance.showBannerWithText(true, "Pobrano zasoby z depozytu", true);
                    }
                    else
                    {
                        BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                    }

                    UpdatePopup();
                }
            );
        }
        else
        {
            BannerController.instance.showBannerWithText(true, "Nikt nie umieścił dla Ciebie zasobów w tym depozycie", true);
        }
    }

    public void ModifyEnergy(bool add)
    {
        float currentValue = float.Parse(sendEnergy.text);
        float newValue = currentValue + (add ? 1000 : -1000);
        if (newValue > PlayerPrefs.GetFloat(AppConstants.EnergyTag))
            newValue = PlayerPrefs.GetFloat(AppConstants.EnergyTag);
        if (newValue >= 0)
            sendEnergy.text = newValue.ToString();
        else
            BannerController.instance.showBannerWithText(true, "Wartość przekroczona", true);
    }

    public void ModifyBiomass(bool add)
    {
        float currentValue = float.Parse(sendBiomass.text);
        float newValue = currentValue + (add ? 1000 : -1000);
        if (newValue > PlayerPrefs.GetFloat(AppConstants.BiomassTag))
            newValue = PlayerPrefs.GetFloat(AppConstants.BiomassTag);
        if (newValue >= 0)
            sendBiomass.text = newValue.ToString();
        else
            BannerController.instance.showBannerWithText(true, "Wartość przekroczona", true);
    }

    public void ModifyGadgets(bool add)
    {
        float currentValue = float.Parse(sendGadgets.text);
        float newValue = currentValue + (add ? 1000 : -1000);
        if (newValue > PlayerPrefs.GetFloat(AppConstants.GadgetsTag))
            newValue = PlayerPrefs.GetFloat(AppConstants.GadgetsTag);
        if (newValue >= 0)
            sendGadgets.text = newValue.ToString();
        else
            BannerController.instance.showBannerWithText(true, "Wartość przekroczona", true);
    }

    public void ModifyFuel(bool add)
    {
        float currentValue = float.Parse(sendFuel.text);
        float newValue = currentValue + (add ? 1000 : -1000);
        if (newValue > PlayerPrefs.GetFloat(AppConstants.FuelTag))
            newValue = PlayerPrefs.GetFloat(AppConstants.FuelTag);
        if (newValue >= 0)
            sendFuel.text = newValue.ToString();
        else
            BannerController.instance.showBannerWithText(true, "Wartość przekroczona", true);
    }

    public void Send()
    {
        if(nickInput.text.Length < 3)
        {
            BannerController.instance.showBannerWithText(true, "Podaj poprawny nick odbiorcy", true);
            nickInput.text = "";
            return;
        }
        if (sendEnergy.text != "0" || sendBiomass.text != "0" || sendGadgets.text != "0" || sendFuel.text != "0")
        {
            sendButton.alpha = 0.5f;
            sendButton.interactable = false;
            sendButton.GetComponent<Button>().interactable = false;
            Utils.Web.PostValues(AppConstants.DepositSendResourcesUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
                new KeyValuePair<string, string>("DEPOSIT_ID", deposit.id),
                new KeyValuePair<string, string>("RECEIVER_NICK", nickInput.text),
                new KeyValuePair<string, string>("ENERGY", sendEnergy.text),
                new KeyValuePair<string, string>("BIOMASS", sendBiomass.text),
                new KeyValuePair<string, string>("GADGETS", sendGadgets.text),
                new KeyValuePair<string, string>("FUEL", sendFuel.text)
            }), (code, response) =>
            {
                if (code == 200)
                {
                    AnalyticsController.instance.Report("DepositSend", new Dictionary<string, object>() {
                        { "energy", float.Parse(sendEnergy.text) },
                        { "biomass", float.Parse(sendBiomass.text) },
                        { "gadgets", float.Parse(sendGadgets.text) },
                        { "fuel", float.Parse(sendFuel.text) },
                        { "depositName", deposit.name },
                        { "depositId", deposit.id },
                        { "receiver", nickInput.text }
                    });

                    ResourcesController.instance.AddEnergy(-float.Parse(sendEnergy.text));
                    ResourcesController.instance.AddBiomass(-float.Parse(sendBiomass.text));
                    ResourcesController.instance.AddGadgets(-float.Parse(sendGadgets.text));
                    ResourcesController.instance.AddFuel(-float.Parse(sendFuel.text));

                    sendEnergy.text = "0";
                    sendBiomass.text = "0";
                    sendGadgets.text = "0";
                    sendFuel.text = "0";

                    nickInput.text = "";

                    BannerController.instance.showBannerWithText(true, "Wysłano zasoby do gracza", true);
                }
                else if (code == 400)
                {
                    nickInput.text = "";
                    BannerController.instance.showBannerWithText(true, "Nie odnaleziono gracza o podanym nicku", true);
                }
                else
                {
                    BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                }

                UpdatePopup();
            });
        }
        else
        {
            BannerController.instance.showBannerWithText(true, "Dodaj zasoby do wysyłki", true);
        }
    }

}