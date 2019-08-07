using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{

    // Instance
    public static UIController instance;
    UIController() { instance = this; }

    public bool uiActive = false;

}
