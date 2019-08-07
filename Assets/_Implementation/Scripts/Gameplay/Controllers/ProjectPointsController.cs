using UnityEngine;
using UnityEngine.UI;

public class ProjectPointsController : MonoBehaviour
{

    // Instance
    public static ProjectPointsController instance;
    ProjectPointsController() { instance = this; }

    // Project points texts
    public Text naturePointsText, commercyPointsText, industryPointsText;

    // Initialization
    public void Start()
    {
        naturePointsText.text = ResourcesController.FormatNumber(PlayerPrefs.GetInt(AppConstants.NaturePoints) + PlayerPrefs.GetInt(AppConstants.NatureDeltaPoints));
        commercyPointsText.text = ResourcesController.FormatNumber(PlayerPrefs.GetInt(AppConstants.CommercyPoints) + PlayerPrefs.GetInt(AppConstants.CommercyDeltaPoints));
        industryPointsText.text = ResourcesController.FormatNumber(PlayerPrefs.GetInt(AppConstants.IndustryPoints) + PlayerPrefs.GetInt(AppConstants.IndustryDeltaPoints));
    }

    // Method to add nature project point
    public void AddNatureProjectPoints(int value)
    {
        PlayerPrefs.SetInt(AppConstants.NatureDeltaPoints, PlayerPrefs.GetInt(AppConstants.NatureDeltaPoints) + value);
        PlayerPrefs.Save();
        naturePointsText.text = ResourcesController.FormatNumber(PlayerPrefs.GetInt(AppConstants.NaturePoints) + PlayerPrefs.GetInt(AppConstants.NatureDeltaPoints));
    }

    // Method to add commercy project point
    public void AddCommercyProjectPoints(int value)
    {
        PlayerPrefs.SetInt(AppConstants.CommercyDeltaPoints, PlayerPrefs.GetInt(AppConstants.CommercyDeltaPoints) + value);
        PlayerPrefs.Save();
        commercyPointsText.text = ResourcesController.FormatNumber(PlayerPrefs.GetInt(AppConstants.CommercyPoints) + PlayerPrefs.GetInt(AppConstants.CommercyDeltaPoints));
    }

    // Method to add industry project point
    public void AddIndustryProjectPoints(int value)
    {
        PlayerPrefs.SetInt(AppConstants.IndustryDeltaPoints, PlayerPrefs.GetInt(AppConstants.IndustryDeltaPoints) + value);
        PlayerPrefs.Save();
        industryPointsText.text = ResourcesController.FormatNumber(PlayerPrefs.GetInt(AppConstants.IndustryPoints) + PlayerPrefs.GetInt(AppConstants.IndustryDeltaPoints));
    }

}