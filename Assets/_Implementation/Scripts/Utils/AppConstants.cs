using UnityEngine;

public class AppConstants : MonoBehaviour
{

    //
    // Variables
    //
    public static string UUID = "";
    public static string OneSignalID = "";

    //
    // Fractions
    //
    public enum Fraction
    {
        NATURE = 1,
        COMMERCY = 2,
        INDUSTRY = 3
    }

    //
    // Resources
    //
    public enum ResourceType
    {
        ENERGY = 0,
        BIOMASS = 1,
        GADGETS = 2,
        FUEL = 3,
    }

    //
    //   URLs
    //

    // API
    public static readonly string ApiUrl = "http://34.77.150.120/API/";
    public static readonly string ApiAuthHeader = "Basic cGtvc3plbGU6a3B3ZDE5OTQ=";

    // General
    public static readonly string CreateUserUrl = ApiUrl + "addUser.php?";
    public static readonly string GetArenaParamsUrl = ApiUrl + "getArenaParams.php?";
    public static readonly string LoginUrl = ApiUrl + "loginUser.php?";
    public static readonly string UpdatePlayerStateUrl = ApiUrl + "updatePlayerState.php?";

    // Patches
    public static readonly string AddPatchUrl = ApiUrl + "addPatch.php?";
    public static readonly string RemovePatchUrl = ApiUrl + "removePatch.php?";

    // Generators
    public static readonly string AddGeneratorUrl = ApiUrl + "addGenerator.php?";
    public static readonly string LevelUpGeneratorUrl = ApiUrl + "levelupGenerator.php?";
    public static readonly string AddBatteryUrl = ApiUrl + "addBattery.php?";
    public static readonly string LevelUpBatteryUrl = ApiUrl + "levelupBattery.php?";
    public static readonly string AddConverterUrl = ApiUrl + "addConverter.php?";
    public static readonly string LevelUpConverterUrl = ApiUrl + "levelupConverter.php?";
    public static readonly string ChangeGeneratorConversionUrl = ApiUrl + "changeConverter.php?";

    // Trees
    public static readonly string AddTreeUrl = ApiUrl + "addTree.php?";
    public static readonly string CollectOwnerFruitUrl = ApiUrl + "collectOwnerFruit.php?";
    public static readonly string CollectCommonFruitUrl = ApiUrl + "collectCommonFruit.php?";

    // Automations
    public static readonly string AddAutomationUrl = ApiUrl + "addAutomation.php?";
    public static readonly string CollectAutomationGadgetsUrl = ApiUrl + "collectAutomationGadgets.php?";
    public static readonly string CollectAutomationEnergyUrl = ApiUrl + "collectAutomationEnergy.php?";
    public static readonly string CollectAutomationBiomassUrl = ApiUrl + "collectAutomationBiomass.php?";
    public static readonly string CollectAutomationFuelUrl = ApiUrl + "collectAutomationFuel.php?";
    public static readonly string ChangeAutomationConversionUrl = ApiUrl + "changeAutomationConversion.php?";

    // Wells
    public static readonly string AddWellUrl = ApiUrl + "addWell.php?";
    public static readonly string LevelUpWellUrl = ApiUrl + "levelupWell.php?";

    // Project objects
    public static readonly string AddProjectObjectUrl = ApiUrl + "addProjectObject.php?";

    // Borders
    public static readonly string AddBorderUrl = ApiUrl + "addBorder.php?";

    // Deposits
    public static readonly string DepositReceiveResourcesUrl = ApiUrl + "receiveResources.php?";
    public static readonly string DepositSendResourcesUrl = ApiUrl + "sendResources.php?";

    //
    //   PlayerPrefs tags
    //
    public static readonly string NickTag = "UserNick_PlayerPrefs_tag";
    public static readonly string FractionTag = "UserFraction_PlayerPrefs_tag";
    public static readonly string PatchCreatedTag = "PatchCreatedTag_PlayerPrefs_tag";
    public static readonly string GeneratorCreatedTag = "GeneratorCreatedTag_PlayerPrefs_tag";
    public static readonly string GeneratorVisitTimestampTag = "GeneratorVisitTimestampTag_PlayerPrefs_tag";
    public static readonly string ConverterCreatedTag = "ConverterCreatedTag_PlayerPrefs_tag";
    public static readonly string BatteryCreatedTag = "BatteryCreatedTag_PlayerPrefs_tag";
    public static readonly string ProjectObjectsCountTag = "ProjectObjectsCountTag_PlayerPrefs_tag";
    public static readonly string EnergyTag = "Energy_PlayerPrefs_tag";
    public static readonly string BiomassTag = "Biomass_PlayerPrefs_tag";
    public static readonly string GadgetsTag = "Gadgets_PlayerPrefs_tag";
    public static readonly string FuelTag = "Fuel_PlayerPrefs_tag";
    public static readonly string NaturePoints = "NaturePoints_PlayerPrefs_tag";
    public static readonly string CommercyPoints = "CommercyPoints_PlayerPrefs_tag";
    public static readonly string IndustryPoints = "IndustryPoints_PlayerPrefs_tag";
    public static readonly string NatureDeltaPoints = "NatureDeltaPoints_PlayerPrefs_tag";
    public static readonly string CommercyDeltaPoints = "CommercyDeltaPoints_PlayerPrefs_tag";
    public static readonly string IndustryDeltaPoints = "IndustryDeltaPoints_PlayerPrefs_tag";
    public static readonly string AutomationsEnergyDelta = "AutomationsEnergyDelta_PlayerPrefs_tag";
    public static readonly string AutomationsBiomassDelta = "AutomationsBiomassDelta_PlayerPrefs_tag";
    public static readonly string AutomationsFuelDelta = "AutomationsFuelDelta_PlayerPrefs_tag";
    public static readonly string UniqueIdentifier = "UniqueIdentifier_PlayerPrefs_tag";
    public static readonly string FirstLaunchTutorialCompleted = "FirstLaunchTutorialCompleted_PlayerPrefs_tag";

    //
    //  Technical app params
    //
    public static readonly int PlayerDeviceIdentifierID = 0;
    public static readonly int TargetFPS = 30;
    public static readonly int ServerSynchronizationIntervalInSeconds = 6;
    public static readonly int InitialWorldDataDownloadRangeTileDivisionConstant = 2000;
    public static readonly int SyncWorldDataDownloadRangeTileDivisionConstant = 1000;
    public static readonly int TopInfoBannerAutoHideTimeInSeconds = 2;
    public static readonly float PatchTopViewCameraOrthographicSizeConstant = 1.2f;
    public static readonly int PatchCreationPopupTimeoutAfterPatchTopViewShowed = 3;

    //
    //  Gameplay params
    //

    // General
    public static readonly float MinObjectsDistance = 12f;                  // - minimalna odległość pomiędzy obiektami
    public static readonly float MinGeneratorsDistance = 85f;               // - minimalna odległość pomiędzy generatorami
    public static readonly float GeneratorRange = 36f;                      // - zasięg generatorów
    public static readonly float Object1Range = 30f;                        // - zasięg obiektów drzewo, automat i studnia
    public static readonly float ProjectObjectRange = 30f;                  // - zasięg obiektów specjalnych generujących punkty celu wspólnego
    public static readonly float DepositRange = 30f;                        // - zasięg depozytów
    public static readonly float PatchFlagsMinDistance = 12f;               // - minimalna odległość pomiędzy dowolnymi dwiema flagami

    // Patches
    public static readonly int PatchMinimumFlagsCount = 4;                  // - minimalna ilość wierzchołków rewiru
    public static readonly int PatchMaximumFlagsCount = 6;                  // - maksymalna ilość wierzchołków rewiru
    public static readonly float PatchMinimumFlagDistanceInMeters = 32f;    // - minimalna odległość między wierzchołkami rewiru
    public static readonly float PatchMaximumFlagDistanceInMeters = 144f;   // - maksymalna odległość między wierzchołkami rewiru
    public static readonly int PatchRemoveReturnedResourcesCoeff = 2;       // - podzielnik zwracanych zasobów podczas usuwania rewiru

    // Generators
    public static readonly int GeneratorOwnerEnergyConstant = 40;           // - stała urobku z generatora dla właściciela (na pierwszym poziomie 4 E/s)
    public static readonly int GeneratorCommonEnergyConstant = 10;          // - stała urobku generatora dla obcych użytkowników (na pierwszym poziomie 1 E/s)
    public static readonly int GeneratorUpgradeConstant = 100;              // - stała kosztu podnoszenia poziomu generatora
    public static readonly int GeneratorBatteryPurchaseCost = 1000;         // - koszt dołączenia akumulatora
    public static readonly int GeneratorBatteryCapacityConstant = 10000;    // - pojemność początkowa akumulatora
    public static readonly int GeneratorBatteryUpgradeConstant = 1000;      // - stała kosztu podnoszenia poziomu akumulatora
    public static readonly int GeneratorConverterPurchaseCost = 5000;       // - koszt dołączenia konwertera
    public static readonly int GeneratorConverterEfficiencyConstant = 10;   // - stała wydajności konwertera (na pierwszym poziomie 1/s)
    public static readonly int GeneratorConverterUpgradeConstant = 5000;    // - stała kosztu podnoszenia poziomu konwertera
    public static readonly int GeneratorConversionChangeConstant = 5000;    // - koszt zmiany sposobu konwersji

    // Trees
    public static readonly int TreePurchaseCost = 2500;                     // - koszt początkowy drzewa (biomasa)
    public static readonly float TreesBonusDistance = 4f;                   // - zasięg premii zasadzonego drzewa
    public static readonly int TreeUpgradeTimeConstantInSeconds = 360;      // - czas potrzebny na zyskanie jednego poziomu przez drzewo (sekundy)
    public static readonly float TreeFruitGrowConstant = 0.9f;              // - tempo rośnięcia owocu na drzewie ((0.1 biomasy * poziom) / s)
    public static readonly int TreeFruitMaximumSizeConstant = 100;          // - maksymalny rozmiar owocu dla obcych użytkowników (100 biomasy * poziom)
    public static readonly float TreeSingleBonusValue = 0.01f;              // - wartość bonusu pochodzącego od jednego drzewa (1% * poziom)
    public static readonly float TreesBonusMinValue = 0.01f;                // - minimalna wartość mnożnika wydajnośći dla drzew (bonusy ze studni oraz innych drzew w pobliżu)
    public static readonly float TreesBonusMaxValue = 2f;                   // - maksymalna wartość mnożnika wydajnośći dla drzew (bonusy ze studni oraz innych drzew w pobliżu)

    // Wells
    public static readonly int WellPurchaseCost = 2500;                     // koszt początkowy studni (paliwo)
    public static readonly float WellOwnerFuelConstant = 10f;               // - stała tempa produkcji paliwa dla właściciela (na pierwszym poziomie 1/s)
    public static readonly float WellCommonFuelConstant = 2.5f;             // - stała tempa produkcji paliwa dla obcych użytkowników (na pierwszym poziomie 0.25/s)
    public static readonly float WellsBonusDistance = 4f;                   // - zasięg premii wybudowanej studnii
    public static readonly int WellUpgradeCostConstant = 2500;              // - stała kosztu podniesienia poziomu studnii
    public static readonly float WellSingleBonusValue = -0.05f;             // - bonus wydajności dla pobliskich obiektów
    public static readonly float WellsBonusMinValue = 0.05f;                // - minimalny mnożnik wydajnośći studnii (od bonusów)
    public static readonly float WellsBonusMaxValue = 2f;                   // - maksymalny mnożnik wydajności studnii (od bonusów)

    // Automations
    public static readonly int AutomationPurchaseCost = 2500;               // - koszt zakupu automatu (gadżety)
    public static readonly int AutomationGadgetsProductionConstant = 10;    // - stała produkcji gadżetów przez automat (domyślnie 1/s)
    public static readonly int AutomationSingleResourceCapacity = 10000;    // - pojemność automatu (w gadżetach)
    public static readonly int AutomationConversionDecreaseRatio = 1;       // - stała wzrostu ceny konwersji automatu (co sekundę)
    public static readonly int AutomationConversionRatioResetTime = 5;      // - czas po którym współczynnik konwersji automatu zostaje zresetowany (w sekundach)
    public static readonly int AutomationConversionDefaultInputValue = 10;  // - początkowa liczba zasobów na wejściu konwersji
    public static readonly int AutomationConversionDefaultOutputValue = 10; // - początkowa liczba gadżetów na wyjściu konwersji

    // Project objects
    public static readonly int ProjectObjectPurchaseCost = 100000;          // - koszt obiektu specjalnego (w dwóch zasobach, zależnie od frakcji)
    public static readonly float ProjectObjectInputEnergy = 10f;            // - koszt wygenerowania jednego punktu projektu specjalnego
    public static readonly int ProjectObjectOutputPoints = 10;              // - ilość generowanych punktów specjalnych (na sekundę)

    // Borders
    public static readonly int BorderPurchaseCost = 1250;                   // - koszt postawienia granicy (wszystkie zasoby)

}