using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameInitializer : MonoBehaviour
{

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL externs
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void SetUUID(string uuid);
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern string GetUUID();
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void DeleteUUID();
#endif

    // Instance
    public static GameInitializer instance;
    GameInitializer() { instance = this; }

    // Map
    public GoMap.GOMap map;

    // Location
    public GoShared.LocationManager location;

    // Fader
    public GameObject fader;
    public Animation faderAnim;

    // Nick popup
    public Animation nickPopup;
    public InputField nickInput;

    // Fraction popup
    public Animation fractionPopup;
    public Image fractionIcon;
    public Text fractionText;
    public Sprite natureSprite, commercySprite, industrySprite;

    // Generator creation tutorial popup
    public Animation generatorCreationPopup;
    public Animation[] generatorCreationTutorialPages;

    // Patch creation tutorial popup
    public Animation patchCreationPopup;
    public Animation[] patchCreationTutorialPages;
    public Image patchCreationPopupFractionIcon;

    // Generator button
    public Animation generatorButton;

    // Patch flag button
    public Animation patchFlagButton;
    public Text patchFlagButtonCount;
    public Button patchFlagButtonEnd;

    // Patch flag prefab
    public GameObject patchFlag;

    // Patch created confirmation
    public Animation patchCreatedConfirmation;
    public Text patchCreatedConfirmationInfoText;
    public Text patchCreatedConfirmationButtonText;

    // Tiles loading with progress showing
    int tilesCounter = 0;
    Coroutine fillCoroutine;

    // HUD and world objects
    public GameObject HUD;

    // Initial tutorial's sprites
    public Sprite welcomeSprite, natureObjectSprite, commercyObjectSprite, industryObjectSprite, generatorSprite, clickSprite;

    // Login flag
    public static bool loggedIn = false;

    // Initialization
    void Awake()
    {

        // UUID management
#if UNITY_WEBGL && !UNITY_EDITOR
        string uuid = GetUUID();
        uuid = (uuid.Length == 0 ? System.Guid.NewGuid().ToString("N") : uuid);
        SetUUID(uuid);
        AppConstants.UUID = uuid;
#else
        PlayerPrefs.SetString(AppConstants.UniqueIdentifier, SystemInfo.deviceUniqueIdentifier);
        PlayerPrefs.Save();
        AppConstants.UUID = PlayerPrefs.GetString(AppConstants.UniqueIdentifier);
#endif

        // OneSignal initialization
        OneSignal.StartInit("927cbb8d-88bc-4468-a8b7-f810a72594a4").EndInit();
        OneSignal.inFocusDisplayType = OneSignal.OSInFocusDisplayOption.InAppAlert;
        OneSignal.IdsAvailable(OnOneSignalIDsAvailable);

        Application.targetFrameRate = AppConstants.TargetFPS;

        HUD.SetActive(false);

        map.OnTileLoad.AddListener((tile) => { OnTileLoaded(); });

        // Logging in
        Utils.Web.GetJSON(AppConstants.LoginUrl + "USER_ID=" + AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID, (loginSuccess, json) =>
        {
            loggedIn = loginSuccess;
            if (loggedIn)
            {
                PlayerPrefs.SetString(AppConstants.NickTag, Utils.JSON.GetString(json, "nick"));
                PlayerPrefs.SetInt(AppConstants.FractionTag, (int)Utils.JSON.GetLong(json, "fraction"));

                PlayerPrefs.SetInt(AppConstants.PatchCreatedTag, (int)Utils.JSON.GetLong(json, "patchCreated"));

                if (!PlayerPrefs.HasKey(AppConstants.EnergyTag))
                    PlayerPrefs.SetFloat(AppConstants.EnergyTag, float.Parse(Utils.JSON.GetString(json, "resource0")));
                if (!PlayerPrefs.HasKey(AppConstants.BiomassTag))
                    PlayerPrefs.SetFloat(AppConstants.BiomassTag, float.Parse(Utils.JSON.GetString(json, "resource1")));
                if (!PlayerPrefs.HasKey(AppConstants.GadgetsTag))
                    PlayerPrefs.SetFloat(AppConstants.GadgetsTag, float.Parse(Utils.JSON.GetString(json, "resource2")));
                if (!PlayerPrefs.HasKey(AppConstants.FuelTag))
                    PlayerPrefs.SetFloat(AppConstants.FuelTag, float.Parse(Utils.JSON.GetString(json, "resource3")));
                if (!PlayerPrefs.HasKey(AppConstants.GeneratorVisitTimestampTag))
                    PlayerPrefs.SetInt(AppConstants.GeneratorVisitTimestampTag, (int)Utils.JSON.GetLong(json, "generatorVisitTimestamp"));

                PlayerPrefs.SetInt(AppConstants.NaturePoints, (int)Utils.JSON.GetLong(json, "naturePoints"));
                PlayerPrefs.SetInt(AppConstants.CommercyPoints, (int)Utils.JSON.GetLong(json, "industryPoints"));
                PlayerPrefs.SetInt(AppConstants.IndustryPoints, (int)Utils.JSON.GetLong(json, "commercyPoints"));
                PlayerPrefs.SetInt(AppConstants.ProjectObjectsCountTag, (int)Utils.JSON.GetLong(json, "projectObjectCount"));

                PlayerPrefs.SetInt(AppConstants.GeneratorCreatedTag, (int)Utils.JSON.GetLong(json, "generatorCreated"));
                PlayerPrefs.SetInt(AppConstants.ConverterCreatedTag, (int)Utils.JSON.GetLong(json, "converterCreated"));
                PlayerPrefs.SetInt(AppConstants.BatteryCreatedTag, (int)Utils.JSON.GetLong(json, "batteryCreated"));

                PlayerPrefs.Save();
            }
            else
            {
                PlayerPrefs.DeleteAll();
            }
        });
    }

    // OneSignalID callback
    void OnOneSignalIDsAvailable(string userID, string pushID)
    {
        AppConstants.OneSignalID = userID;
    }

    void OnTileLoaded()
    {
        tilesCounter++;
        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);
        if (tilesCounter <= Mathf.Pow(map.tileBuffer * 2 + 1, 2))
            fillCoroutine = StartCoroutine(SmoothProgress((float)tilesCounter / Mathf.Pow(map.tileBuffer * 2 + 1, 2)));
    }

    IEnumerator SmoothProgress(float fillValue)
    {
        while (Mathf.Abs(fader.GetComponentInChildren<Image>().fillAmount - fillValue) > 0.0001f)
        {
            fader.GetComponentInChildren<Image>().fillAmount = Mathf.Lerp(fader.GetComponentInChildren<Image>().fillAmount, fillValue, Time.smoothDeltaTime * 4);
            yield return new WaitForEndOfFrame();
        }
        fader.GetComponentInChildren<Image>().fillAmount = fillValue;

        if (fillValue >= 1)
        {
            // Pobieranie parametrów areny rozgrywki
            Utils.Web.GetJSON(AppConstants.GetArenaParamsUrl, (success, json) =>
            {
                if (success)
                {
                    GoShared.Coordinates coordinate = new GoShared.Coordinates(double.Parse(Utils.JSON.GetString(json, "lat")), double.Parse(Utils.JSON.GetString(json, "lon")), 0);
                    Vector3 arenaCenter = coordinate.convertCoordinateToVector();
                    ArenaController.instance.Init(arenaCenter, float.Parse(Utils.JSON.GetString(json, "radius")) * (location.worldScale / 2));

                    if (!loggedIn)
                    {
                        nickPopup.Play("popupIn");

                        GeneratorsController.instance.gameObject.SetActive(true);

                        TreesController.instance.gameObject.SetActive(false);
                        AutomationsController.instance.gameObject.SetActive(false);
                        WellsController.instance.gameObject.SetActive(false);
                        ProjectObjectsController.instance.gameObject.SetActive(false);
                        DepositsController.instance.gameObject.SetActive(false);
                    }
                    else
                    {
                        faderAnim.Play("fadeOut");

                        if (PlayerPrefs.GetInt(AppConstants.PatchCreatedTag) == 0 || PlayerPrefs.GetInt(AppConstants.GeneratorCreatedTag) == 0)
                        {
                            int fraction = PlayerPrefs.GetInt(AppConstants.FractionTag);
                            patchCreationPopupFractionIcon.sprite = fraction == (int)AppConstants.Fraction.NATURE ? natureSprite : fraction == (int)AppConstants.Fraction.COMMERCY ? commercySprite : industrySprite;

                            if (PlayerPrefs.GetInt(AppConstants.GeneratorCreatedTag) == 0)
                                generatorCreationPopup.Play("popupIn");
                            else
                                patchCreationPopup.Play("popupIn");

                            GeneratorsController.instance.gameObject.SetActive(true);

                            TreesController.instance.gameObject.SetActive(false);
                            AutomationsController.instance.gameObject.SetActive(false);
                            WellsController.instance.gameObject.SetActive(false);
                            ProjectObjectsController.instance.gameObject.SetActive(false);
                            DepositsController.instance.gameObject.SetActive(false);
                        }
                        else
                        {
                            HUD.SetActive(true);
                            if (PlayerPrefs.GetInt(AppConstants.FirstLaunchTutorialCompleted) == 0)
                            {
                                FirstLaunchTutorialWindows();
                            }
                        }
                    }
                }
                else
                {
                    BannerController.instance.showBannerWithText(true, "Błąd serwera :(", true);
                }
            });
        }
    }

    // Nick input callback
    public void NickInputCallback()
    {
        nickInput.interactable = false;
        string nick = nickInput.text;

        // Adding user
        Utils.Web.PostValues(AppConstants.CreateUserUrl, new List<KeyValuePair<string, string>>(new KeyValuePair<string, string>[] {
            new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID),
            new KeyValuePair<string, string>("NICK", nick),
        }), (code, response) =>
        {
            Debug.Log(response);
            if (code == 200)
            {
                Dictionary<string, object> json = Utils.JSON.GetDictionary(response);
                PlayerPrefs.SetString(AppConstants.NickTag, Utils.JSON.GetString(json, "nick"));
                int fraction = (int)Utils.JSON.GetLong(json, "fraction");
                PlayerPrefs.SetInt(AppConstants.FractionTag, fraction);
                nickPopup.Play("popupOut");
                patchCreationPopupFractionIcon.sprite = fraction == (int)AppConstants.Fraction.NATURE ? natureSprite : fraction == (int)AppConstants.Fraction.COMMERCY ? commercySprite : industrySprite;
                fractionIcon.sprite = fraction == (int)AppConstants.Fraction.NATURE ? natureSprite : fraction == (int)AppConstants.Fraction.COMMERCY ? commercySprite : industrySprite;
                fractionText.text = fraction == (int)AppConstants.Fraction.NATURE ? "przyrodniczej" : fraction == (int)AppConstants.Fraction.COMMERCY ? "komercyjnej" : "przemysłowej";
                fractionPopup.Play("popupIn");

                loggedIn = true;

                PlayerPrefs.Save();
            }
            else
            {
                if (code == 400 && Utils.JSON.GetLong(Utils.JSON.GetDictionary(response), "status_code") == 4002)
                    BannerController.instance.showBannerWithText(true, "Ten nick jest zajęty", true);
                nickInput.interactable = true;
            }

        });
    }

    // Fraction popup button callback
    public void CloseFractionPopup()
    {
        fractionPopup.Play("popupOut");
        faderAnim.Play("fadeOut");
        generatorCreationPopup.Play("popupIn");
    }

    // Generator creation tutorial switch
    public void GeneratorTutorialSwitch(int index)
    {
        generatorCreationTutorialPages[index - 1].Play("tutorialSwitchOut");
        generatorCreationTutorialPages[index].Play("tutorialSwitchIn");
    }

    // Generator button
    public void GeneratorButtonShow()
    {
        generatorButton.Play("downButtonIn");
    }

    // Generator tutorial end button
    public void GeneratorTutorialPopupClose()
    {
        generatorCreationPopup.Play("popupOut");
    }

    // Place generator method
    public void PlaceGenerator()
    {
        GeneratorsController.instance.CreateGeneratorAtUserPosition(false);
    }

    // Place generator success callback
    public void PlaceGeneratorSuccess()
    {
        generatorButton.Play("downButtonOut");
        StartCoroutine(PlaceGeneratorSuccessCoroutine());
    }
    IEnumerator PlaceGeneratorSuccessCoroutine()
    {
        yield return new WaitForSeconds(3);
        patchCreationPopup.Play("popupIn");
    }

    // Patch creation tutorial switch
    public void PatchTutorialSwitch(int index)
    {
        patchCreationTutorialPages[index - 1].Play("tutorialSwitchOut");
        patchCreationTutorialPages[index].Play("tutorialSwitchIn");
    }

    // Patch flag button
    public void PatchFlagButtonShow()
    {
        patchFlagButtonCount.text = AppConstants.PatchMaximumFlagsCount.ToString();
        patchFlagButtonEnd.interactable = false;
        patchFlagButtonEnd.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = 0.5f;

        int fraction = PlayerPrefs.GetInt(AppConstants.FractionTag);
        patchFlagButton.transform.GetChild(0).GetComponent<Image>().sprite = fraction == (int)AppConstants.Fraction.NATURE ? natureSprite : fraction == (int)AppConstants.Fraction.COMMERCY ? commercySprite : industrySprite;
        patchFlagButton.Play("downButtonIn");

        foreach (SpriteRenderer sr in patchFlag.GetComponentsInChildren<SpriteRenderer>())
            sr.sprite = fraction == (int)AppConstants.Fraction.NATURE ? natureSprite : fraction == (int)AppConstants.Fraction.COMMERCY ? commercySprite : industrySprite;

        GameObject flag = Instantiate(patchFlag);
        flag.transform.SetParent(PlayerColliderController.instance.transform);
        flag.transform.SetSiblingIndex(0);
        flag.transform.localPosition = new Vector3(5f, 10f, 0f);
        flag.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);

        foreach (GeneratorController generator in GeneratorsController.instance.generators)
            generator.SetupVisualState(generator.generatorData.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));
    }

    // Patch tutorial end button
    public void PatchTutorialPopupClose()
    {
        patchCreationPopup.Play("popupOut");
    }

    // Patch flag placement
    List<Vector3> flagPositions = new List<Vector3>();
    List<GameObject> flagObjects = new List<GameObject>();
    public void PlacePatchFlag(bool confirmed = false)
    {
        if (!ArenaController.instance.CheckIfWithinArena(PlayerColliderController.instance.transform.GetChild(0).position))
        {
            BannerController.instance.showBannerWithText(true, "Jesteś poza obszarem rozgrywki!", true);
            return;
        }

        if (!PlayerColliderController.instance.CanPlacePatchFlagHere())
        {
            BannerController.instance.showBannerWithText(true, "Zbyt blisko wierzchołków rewiru innego gracza. Odejdź kilka metrów", true);
            return;
        }

        if (flagPositions.FindIndex(x => Vector3.Distance(x, PlayerColliderController.instance.transform.GetChild(0).position) < AppConstants.PatchMinimumFlagDistanceInMeters) > -1)
        {
            BannerController.instance.showBannerWithText(true, "Zbyt blisko poprzednich flag. Odejdź kilka metrów", true);
            return;
        }

        if (flagPositions.Count > 0 && Vector3.Distance(flagPositions[flagPositions.Count - 1], PlayerColliderController.instance.transform.GetChild(0).position) > AppConstants.PatchMaximumFlagDistanceInMeters)
        {
            BannerController.instance.showBannerWithText(true, "Zbyt duży odstęp. Cofnij się troszkę", true);
            return;
        }

        if (flagPositions.Count > 2)
        {
            for (int i = 1; i < flagPositions.Count; i++)
            {
                Vector2 point = new Vector2(PlayerColliderController.instance.transform.GetChild(0).position.x, PlayerColliderController.instance.transform.GetChild(0).position.z);
                Vector2 lineStart = new Vector2(flagPositions[i - 1].x, flagPositions[i - 1].z);
                Vector2 lineEnd = new Vector2(flagPositions[i].x, flagPositions[i].z);
                double distance = Geometry.DistanceFromPointToLine(point, lineStart, lineEnd);
                if (distance < AppConstants.PatchMinimumFlagDistanceInMeters / 5)
                {
                    BannerController.instance.showBannerWithText(true, "Twoje flagi są położone zbyt blisko jednej prostej. Postaraj się utworzyć wielokąt foremny.", true);
                    return;
                }
            }
        }

        if (flagPositions.Count == AppConstants.PatchMaximumFlagsCount - 1)
        {
            List<Vector2> patchPolygon = new List<Vector2>();
            flagPositions = Geometry.GetConvexHull(flagPositions);
            foreach (Vector3 flagPosition in flagPositions)
                patchPolygon.Add(new Vector2(flagPosition.x, flagPosition.z));
            Vector2 newFlagPos = new Vector2(PlayerColliderController.instance.transform.GetChild(0).position.x, PlayerColliderController.instance.transform.GetChild(0).position.z);
            patchPolygon.Add(newFlagPos);
            GeneratorController ownGenerator = GeneratorsController.instance.generators.Find(x => x.generatorData.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));
            if (!Geometry.PolygonContainsPoint(patchPolygon, new Vector2(ownGenerator.transform.position.x, ownGenerator.transform.position.z)))
            {
                BannerController.instance.showBannerWithText(true, "Twój generator nie leży wewnątrz wyznaczonego rewiru!", true);
                return;
            }
        }

        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { PlacePatchFlag(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz tutaj postawić flagę?");
            return;
        }

        flagPositions.Add(PlayerColliderController.instance.transform.GetChild(0).position);
        patchFlagButtonCount.text = (AppConstants.PatchMaximumFlagsCount - flagPositions.Count).ToString();

        flagObjects.Add(PlayerColliderController.instance.transform.GetChild(0).gameObject);
        PlayerColliderController.instance.transform.GetChild(0).transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
        PlayerColliderController.instance.transform.GetChild(0).transform.SetParent(map.transform.parent);

        if (flagPositions.Count == AppConstants.PatchMinimumFlagsCount)
        {
            patchFlagButtonEnd.interactable = true;
            patchFlagButtonEnd.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = 1f;
        }

        if (flagPositions.Count >= AppConstants.PatchMaximumFlagsCount)
        {
            PlacingPatchFlagEnd(true);
        }
        else
        {
            GameObject flag = Instantiate(patchFlag);
            flag.transform.SetParent(PlayerColliderController.instance.transform);
            flag.transform.SetSiblingIndex(0);
            flag.transform.localPosition = new Vector3(5f, 10f, 0f);
            flag.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
        }
    }

    // Reset patch construction
    public void ResetPatchConstruction(bool confirmed = false)
    {
        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { ResetPatchConstruction(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy chcesz zacząć od nowa?");
            return;
        }

        flagPositions.Clear();
        foreach (GameObject flagObject in flagObjects)
            Destroy(flagObject);
        patchFlagButtonCount.text = AppConstants.PatchMaximumFlagsCount.ToString();
        patchFlagButtonEnd.interactable = false;
        patchFlagButtonEnd.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = 0.5f;

    }

    // Patch construction end
    public void PlacingPatchFlagEnd(bool confirmed = false)
    {
        List<Vector2> patchPolygon = new List<Vector2>();
        flagPositions = Geometry.GetConvexHull(flagPositions);
        foreach (Vector3 flagPosition in flagPositions)
            patchPolygon.Add(new Vector2(flagPosition.x, flagPosition.z));
        GeneratorController ownGenerator = GeneratorsController.instance.generators.Find(x => x.generatorData.nick.Equals(PlayerPrefs.GetString(AppConstants.NickTag)));
        if (!Geometry.PolygonContainsPoint(patchPolygon, new Vector2(ownGenerator.transform.position.x, ownGenerator.transform.position.z)))
        {
            BannerController.instance.showBannerWithText(true, "Twój generator nie leży wewnątrz wyznaczonego rewiru!", true);
            return;
        }

        if (!confirmed)
        {
            ConfirmationPopup.instance.Open(() => { PlacingPatchFlagEnd(true); ConfirmationPopup.instance.Close(); }, () => { ConfirmationPopup.instance.Close(); }, "Czy uznać Twój rewir za ukończony?");
            return;
        }

        patchFlagButton.Play("downButtonOut");

        Vector3 centroid = Geometry.CalculateCentroid(flagPositions);
        float maxDiagonalLength = float.MinValue;
        foreach (Vector3 flagPosition1 in flagPositions)
        {
            foreach (Vector3 flagPosition2 in flagPositions)
            {
                if (Vector3.Distance(flagPosition1, flagPosition2) > maxDiagonalLength)
                    maxDiagonalLength = Vector3.Distance(flagPosition1, flagPosition2);
            }
        }

        StartCoroutine(SmoothPatchView(centroid, maxDiagonalLength));
    }

    // SkyView camera animation on patch
    IEnumerator SmoothPatchView(Vector3 centroid, float maxDiagonalLength)
    {
        fader.GetComponentInChildren<Image>().fillAmount = 0;
        faderAnim.Play("fadeIn");

        yield return new WaitWhile(() => faderAnim.isPlaying);
        Camera.main.gameObject.GetComponent<GoShared.GOOrbit>().enabled = false;
        Camera.main.transform.position = new Vector3(centroid.x, 100, centroid.z);
        Camera.main.transform.eulerAngles = new Vector3(90, 0, 0);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = maxDiagonalLength * AppConstants.PatchTopViewCameraOrthographicSizeConstant;
        Camera.main.transform.parent = null;

        PatchesController.instance.AddPatch(new Patch(-1, flagPositions, new List<bool>(new bool[flagPositions.Count]), PlayerPrefs.GetInt(AppConstants.FractionTag), PlayerPrefs.GetString(AppConstants.NickTag)), 50, true);

        yield return new WaitForSeconds(0.5f);
        faderAnim.Play("fadeOut");

        // Adding patch
        List<KeyValuePair<string, string>> addPatchParams = new List<KeyValuePair<string, string>>();
        addPatchParams.Add(new KeyValuePair<string, string>("USER_ID", AppConstants.UUID + AppConstants.PlayerDeviceIdentifierID));
        for (int i = 0; i < flagPositions.Count; i++)
        {
            addPatchParams.Add(new KeyValuePair<string, string>("LAT" + (i + 1), GoShared.Coordinates.convertVectorToCoordinates(flagPositions[i]).latitude.ToString()));
            addPatchParams.Add(new KeyValuePair<string, string>("LON" + (i + 1), GoShared.Coordinates.convertVectorToCoordinates(flagPositions[i]).longitude.ToString()));
        }

        bool popupReady = false;
        Utils.Web.PostValues(AppConstants.AddPatchUrl, addPatchParams, (code, response) =>
        {
            Debug.Log(response);
            if (code == 200)
            {
                patchCreatedConfirmationInfoText.text = "Twój rewir prezentuje się imponująco! Za chwilę dołączysz do rozgrywki!";
                patchCreatedConfirmationButtonText.text = "Dołącz!";
            }
            else
            {
                patchCreatedConfirmationInfoText.text = "Wystąpił błąd podczas wysyłania Twojego rewiru :( Spróbuj ponownie utworzyć rewir.";
                patchCreatedConfirmationButtonText.text = "Kontynuuj";
            }
            popupReady = true;
        });
        yield return new WaitForSeconds(AppConstants.PatchCreationPopupTimeoutAfterPatchTopViewShowed);
        yield return new WaitUntil(() => popupReady);
        patchCreatedConfirmation.Play("popupIn");
    }

    // Restart scene method
    public void RestartScene()
    {
        StartCoroutine(RestartSceneCoroutine());
    }

    // Restart scene coroutine
    IEnumerator RestartSceneCoroutine()
    {
        patchCreatedConfirmation.Play("popupOut");
        yield return new WaitWhile(() => patchCreatedConfirmation.isPlaying);
        fader.GetComponentInChildren<Image>().fillAmount = 0;
        faderAnim.Play("fadeIn");
        yield return new WaitWhile(() => faderAnim.isPlaying);
        Scene loadedLevel = SceneManager.GetActiveScene();
        SceneManager.LoadScene(loadedLevel.buildIndex);
    }

    // First launch tutorial windows
    void FirstLaunchTutorialWindows()
    {
        string fractionName = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? "przyrodniczej" : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? "komercyjnej" : "przemysłowej";
        string fractionColor = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? "zielonym" : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? "żółtym" : "czerwonym";
        string objectName = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? "drzewa" : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? "automaty" : "studnie";
        string goalName = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? "wiecznej wiosny" : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? "monopolu" : "statku kosmicznego";
        Sprite fractionObjectIcon = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? natureObjectSprite : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? commercyObjectSprite : industryObjectSprite;
        Sprite fractionIconTutorial = PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.NATURE ? natureSprite : PlayerPrefs.GetInt(AppConstants.FractionTag) == (int)AppConstants.Fraction.COMMERCY ? commercySprite : industrySprite;

        InfoPopupWithImage.instance.Open(() =>
        {
            InfoPopupWithImage.instance.Close();
            InfoPopupWithImage.instance.Open(() =>
            {
                InfoPopupWithImage.instance.Close();
                InfoPopupWithImage.instance.Open(() =>
                {
                    InfoPopupWithImage.instance.Close();
                    InfoPopupWithImage.instance.Open(() =>
                    {
                        InfoPopupWithImage.instance.Close();
                        InfoPopupWithImage.instance.Open(() =>
                        {
                            InfoPopupWithImage.instance.Close();
                            InfoPopupWithImage.instance.Open(() =>
                            {

                                InfoPopupWithImage.instance.Close();
                                PlayerPrefs.SetInt(AppConstants.FirstLaunchTutorialCompleted, 1);

                            }, "Pamiętaj! Każdy obiekt znajdujący się na mapie jest klikalny!", clickSprite);
                        }, "Jeśli masz wątpliwości do czego służą poszczególne elementy interfejsu, dotknij ich aby otworzyć okienko z opisem.", clickSprite);
                    }, "Do zbudowania tych obiektów, potrzebujesz całkiem sporej ilośći zasobów. Aby je pozyskać, ulepszaj swój generator, buduj " + objectName + " oraz odwiedzaj rewiry innych graczy.", generatorSprite);
                }, "Zadaniem Twojej frakcji jest stworzenie projektu specjalnego - " + goalName + ". Buduj obiekty oznaczone powyższą ikoną aby generować punkty do projektu specjalnego.", fractionObjectIcon);
            }, "Należysz do frakcji " + fractionName + ". Twój własny rewir jest zakreślony na różowo. Rewiry Twoich sojuszników są oznaczone kolorem " + fractionColor + ".", fractionIconTutorial);
        }, "Witaj w wersji testowej gry Terytoria!", welcomeSprite);
    }

}
