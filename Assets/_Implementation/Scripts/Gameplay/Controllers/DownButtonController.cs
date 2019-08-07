using UnityEngine;
using UnityEngine.UI;

public class DownButtonController : MonoBehaviour
{

    // Instance
    public static DownButtonController instance;
    DownButtonController() { instance = this; }

    // Fractions sprites
    public Sprite natureSprite, commercySprite, industrySprite;

    // Fraction image
    public Image fractionImage;

    // All buttons
    public Button generatorButton, converterButton, batteryButton, removePatchButton, object1Button, object2Button, borderButton;

    // Object 1 sprites
    public Sprite treeSprite, automationSprite, wellSprite;

    // Object 2 sprites
    public Sprite climateRegulatorSprite, artificialIntelligenceSprite, refinerySprite;

    // Hint texts
    public Button generatorHint, batteryHint, converterHint, removePatchHint, object1Hint, object2Hint, wallHint;

    // Animation
    private Animation anim;

    // Help flags
    public bool standardMenuButtonsVisible = false;

    // Initialize
    private void Start()
    {
        anim = this.GetComponent<Animation>();

        int fraction = PlayerPrefs.GetInt(AppConstants.FractionTag);
        fractionImage.sprite = fraction == (int)AppConstants.Fraction.NATURE ? natureSprite : fraction == (int)AppConstants.Fraction.COMMERCY ? commercySprite : industrySprite;

        generatorHint.onClick.RemoveAllListeners();
        generatorHint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Generator</size></b>[n][n]<i>Każdy gracz zaczyna rozgrywkę od postawienia generatora. Podejdź do dowolnego generatora aby pozyskać z niego energię.</i>"); });

        batteryHint.onClick.RemoveAllListeners();
        batteryHint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Bateria</size></b>[n][n]<i>Można ją dołączyć do generatora. Jej zadaniem jest gromadzenie energii wyprodukowanej przez generator pod Twoją nieobecność. Energia zgromadzona w baterii zostanie automatycznie pobrana kiedy do niej podejdziesz.</i>[n][n]<b><color=#bbffffff>Cena:[n]" + AppConstants.GeneratorBatteryPurchaseCost + " energii</color></b>"); });

        converterHint.onClick.RemoveAllListeners();
        converterHint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Konwerter</size></b>[n][n]<i>Można go dołączyć do generatora. Podejdź do niego aby wymienić posiadaną energię na dowolny, wybrany zasób. Rodzaj konwersji możesz zmieniać wielokrotnie za opłatą.</i>[n][n]<b><color=#bbffffff>Cena:[n]" + AppConstants.GeneratorConverterPurchaseCost + " energii</color></b>"); });

        removePatchHint.onClick.RemoveAllListeners();
        removePatchHint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Usuń rewir</size></b>[n][n]<i>Możesz usunąć swój rewir, a następnie utworzyć całkiem nowy w innym miejscu. Otrzymasz 1/" + AppConstants.PatchRemoveReturnedResourcesCoeff + " zasobów których użyłeś do kupna oraz rozwoju swoich obiektów.</i>"); });

        object1Hint.onClick.RemoveAllListeners();
        if (fraction == (int)AppConstants.Fraction.NATURE)
            object1Hint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Drzewo</size></b>[n][n]<i>Rosną na nim owoce zawierające biomasę. Ty, jako właściciel możesz zebrać wyprodukowaną biomasę w dowolnym momencie. Inni gracze mogą pozyskiwać biomasę tylko gdy jej ilość osiągnie założony próg.</i>[n][n]<b><color=#bbffffff>Cena:[n]" + AppConstants.TreePurchaseCost + " biomasy</color></b>"); });
        else if (fraction == (int)AppConstants.Fraction.COMMERCY)
            object1Hint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Automat</size></b>[n][n]<i>Jest to depozyt, w którym gracze mogą zostawić ustalony zasób w zamian za gadżety. Dodatkowo, sam produkuje gadżety. Możesz go opróżnić gdy tylko znajdziesz się w pobliżu.</i>[n][n]<b><color=#bbffffff>Cena:[n]" + AppConstants.AutomationPurchaseCost + " gadżetów</color></b>"); });
        else if (fraction == (int)AppConstants.Fraction.INDUSTRY)
            object1Hint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Studnia</size></b>[n][n]<i>Dostarczy Ci paliwa jeśli znajdziesz się w pobliżu.</i>[n][n]<b><color=#bbffffff>Cena:[n]" + AppConstants.WellPurchaseCost + " paliwa</color></b>"); });

        object2Hint.onClick.RemoveAllListeners();
        if (fraction == (int)AppConstants.Fraction.NATURE)
            object2Hint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Regulator klimatu</size></b>[n][n]<i>Zbliż się do niego aby rozpocząć generację punktów projektu Wieczna Wiosna.</i>[n][n]<b><color=#bbffffff>Cena:[n]po " + AppConstants.ProjectObjectPurchaseCost + " każdego zasobu</color></b>"); });
        else if (fraction == (int)AppConstants.Fraction.COMMERCY)
            object2Hint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Sztuczna inteligencja</size></b>[n][n]<i>Zbliż się do niego aby rozpocząć generację punktów projektu Monopol.</i>[n][n]<b><color=#bbffffff>Cena:[n]po " + AppConstants.ProjectObjectPurchaseCost + " każdego zasobu</color></b>"); });
        else if (fraction == (int)AppConstants.Fraction.INDUSTRY)
            object2Hint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Rafineria</size></b>[n][n]<i>Zbliż się do niego aby rozpocząć generację punktów projektu Statek Kosmiczny.</i>[n][n]<b><color=#bbffffff>Cena:[n]po " + AppConstants.ProjectObjectPurchaseCost + " każdego zasobu</color></b>"); });

        wallHint.onClick.RemoveAllListeners();
        wallHint.onClick.AddListener(() => { InfoPopup.instance.OpenWithoutAction("<b><size=45>Granica</size></b>[n][n]<i>Możesz ją umieścić na dowolnej krawędzi swojego rewiru. Granice zakreślające części wspólne dwóch lub więcej rewirów z tej samej frakcji utworzą terytorium. Obiekty w jego wnętrzu będą miały podwyższoną wydajność.</i>[n][n]<b><color=#bbffffff>Cena:[n]po " + AppConstants.BorderPurchaseCost + " każdego zasobu</color></b>"); });
    }

    // Toggling standard menu buttons
    public void ToggleStandardMenu()
    {
        int fraction = PlayerPrefs.GetInt(AppConstants.FractionTag);

        // Generator
        generatorButton.interactable = PlayerPrefs.GetInt(AppConstants.GeneratorCreatedTag) == 0;

        // Converter
        converterButton.interactable = PlayerPrefs.GetInt(AppConstants.GeneratorCreatedTag) == 1 && PlayerPrefs.GetInt(AppConstants.ConverterCreatedTag) == 0;

        // Battery
        batteryButton.interactable = PlayerPrefs.GetInt(AppConstants.GeneratorCreatedTag) == 1 && PlayerPrefs.GetInt(AppConstants.BatteryCreatedTag) == 0;

        // Remove Patch
        removePatchButton.interactable = true;

        // Object 1
        object1Button.transform.GetChild(0).GetComponent<Image>().sprite = fraction == (int)AppConstants.Fraction.NATURE ? treeSprite : fraction == (int)AppConstants.Fraction.COMMERCY ? automationSprite : wellSprite;
        object1Button.onClick.AddListener(() =>
        {
            switch (fraction)
            {
                case (int)AppConstants.Fraction.NATURE:
                    TreesController.instance.CreateTreeAtUserPosition();
                    break;
                case (int)AppConstants.Fraction.COMMERCY:
                    AutomationsController.instance.CreateAutomationAtUserPosition();
                    break;
                case (int)AppConstants.Fraction.INDUSTRY:
                    WellsController.instance.CreateWellAtUserPosition();
                    break;
            }
        });
        object1Button.interactable = true;

        // Object 2
        object2Button.transform.GetChild(0).GetComponent<Image>().sprite = fraction == (int)AppConstants.Fraction.NATURE ? climateRegulatorSprite : fraction == (int)AppConstants.Fraction.COMMERCY ? artificialIntelligenceSprite : refinerySprite;
        object2Button.interactable = true;

        // Border
        borderButton.interactable = true;

        if (anim.isPlaying)
            return;
        anim.clip = anim.GetClip(anim.clip.name == "downButtonStandardMenuIn" ? "downButtonStandardMenuOut" : "downButtonStandardMenuIn");
        anim.Play();
        standardMenuButtonsVisible = !standardMenuButtonsVisible;
    }

}
