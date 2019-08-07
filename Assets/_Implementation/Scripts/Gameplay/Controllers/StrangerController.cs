using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrangerController : MonoBehaviour
{

    // Stranger data
    public Stranger strangerData;

    // Stranger nick text
    public TextMesh strangerNickText;

    // Stranger nick colors (fractions)
    public Color natureColor, commercyColor, industryColor;

    // Stranger material (fractions)
    public Material natureMaterial, commercyMaterial, industryMaterial;

    // Starting controller coroutine in OnEnable
    private void OnEnable()
    {
        StartCoroutine(StrangerExistanceCoroutine());
        StartCoroutine(StrangerMovementCoroutine());
    }

    // Update data method
    public void SetData(Stranger data)
    {
        this.strangerData = data;
        this.strangerNickText.text = data.nick;
        this.strangerNickText.color = data.fraction == (int)AppConstants.Fraction.NATURE ? natureColor : data.fraction == (int)AppConstants.Fraction.COMMERCY ? commercyColor : industryColor;
        MeshRenderer[] renderers = this.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer render in renderers)
            if (render.GetComponent<TextMesh>() == null)
                render.material = data.fraction == (int)AppConstants.Fraction.NATURE ? natureMaterial : data.fraction == (int)AppConstants.Fraction.COMMERCY ? commercyMaterial : industryMaterial;
    }

    // Coroutine controlling stranger idle time
    IEnumerator StrangerExistanceCoroutine()
    {
        yield return new WaitForEndOfFrame();

        while (true)
        {
            Stranger strangerDataTmp = strangerData;
            yield return new WaitForSeconds(AppConstants.ServerSynchronizationIntervalInSeconds * 3);
            if (strangerDataTmp.updateTimestamp == strangerData.updateTimestamp)
            {
                StrangersController.instance.strangers.Remove(this);
                Destroy(this.gameObject);
                StopAllCoroutines();
            }
        }
    }

    // Coroutine controlling stranger movement
    IEnumerator StrangerMovementCoroutine()
    {

        Vector3 currentDestPosition = Vector3.zero, latestStrangerDataPosition = Vector3.zero;
        Quaternion lookRotation = Quaternion.identity;
        float movementSpeed = 0f;
        while (true)
        {
            if (latestStrangerDataPosition != strangerData.position)
            {
                latestStrangerDataPosition = strangerData.position;
                currentDestPosition = strangerData.position;
                lookRotation = Quaternion.LookRotation(currentDestPosition - this.transform.position);

                movementSpeed = Vector3.Distance(currentDestPosition, this.transform.position);
                movementSpeed = movementSpeed < 1f ? 1f : movementSpeed;
                movementSpeed = movementSpeed > 10f ? 10f : movementSpeed;
            }
            this.transform.position = Vector3.MoveTowards(this.transform.position, currentDestPosition, Time.deltaTime * movementSpeed);
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, lookRotation, 0.5f);

            if (Vector3.Distance(currentDestPosition, this.transform.position) < 0.001f)
            {
                Vector3 randomSpherePoint = Random.insideUnitSphere * 3;
                currentDestPosition = strangerData.position + new Vector3(randomSpherePoint.x, 0, randomSpherePoint.z);
                lookRotation = Quaternion.LookRotation(currentDestPosition - this.transform.position);

                movementSpeed = Vector3.Distance(currentDestPosition, this.transform.position);
                movementSpeed = movementSpeed < 1f ? 1f : movementSpeed;
                movementSpeed = movementSpeed > 10f ? 10f : movementSpeed;

                yield return new WaitForSeconds(1);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    // Destroy smoothly
    public void Destroy()
    {
        StartCoroutine(SmoothDestroy());
    }
    IEnumerator SmoothDestroy()
    {
        Animation anim = this.GetComponent<Animation>();
        anim[anim.clip.name].time = anim[anim.clip.name].length;
        anim[anim.clip.name].speed = -1;
        anim.Play();
        yield return new WaitWhile(() => anim.isPlaying);
        StrangersController.instance.strangers.Remove(this);
        Destroy(this.gameObject);
    }

}
