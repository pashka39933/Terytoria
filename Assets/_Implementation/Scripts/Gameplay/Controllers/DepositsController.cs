using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepositsController : MonoBehaviour
{

    public static DepositsController instance;
    DepositsController() { instance = this; }

    public GameObject depositPrefab;

    public List<DepositController> deposits = new List<DepositController>();

    // Adding automation method
    public void AddDeposit(Deposit deposit)
    {
        if (deposits.FindIndex(x => x.depositData.id.Equals(deposit.id)) > -1)
        {
            deposits.Find(x => x.depositData.id.Equals(deposit.id)).depositData = deposit;
            return;
        }

        DepositController depositController = Instantiate(depositPrefab, this.transform).GetComponent<DepositController>();
        depositController.depositData = deposit;
        deposits.Add(depositController);
        depositController.name = deposit.name;
        depositController.transform.position = new Vector3(deposit.position.x, 8, deposit.position.z);
        depositController.GetComponent<CircleLineRenderer>().CreatePoints(16, AppConstants.DepositRange / 8f, -0.75f);
        depositController.GetComponent<SphereCollider>().enabled = true;
    }

}