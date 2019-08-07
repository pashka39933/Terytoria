using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchController : MonoBehaviour
{

    // Patch data
    public Patch patchData;

    // Patch edge highlighter
    GameObject CurrentPatchHighlight = null;

    // Initialization
    private void OnEnable()
    {
        StartCoroutine(UpdateCoroutine());
    }

    // Update coroutine
    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => patchData != null && patchData.flags != null && patchData.flags.Count > 0 && patchData.flags.Count == this.transform.childCount);
            for (int i = 0; i < patchData.flags.Count; i++)
            {
                Vector3 scale = this.transform.GetChild(i).GetChild(0).localScale;
                this.transform.GetChild(i).GetChild(0).localScale = new Vector3(scale.x, patchData.borders[i] ? 10f : 3f, scale.z);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    // Highlight edge method - returning edge index
    public void HighlightNextEdge()
    {
        if (CurrentPatchHighlight == null)
        {
            CurrentPatchHighlight = Instantiate(PatchesController.instance.patchHighlightPrefab, this.transform.GetChild(0));
        }
        LineRenderer lineRenderer = CurrentPatchHighlight.GetComponent<LineRenderer>();

        int counter = 0;
        int index = CurrentPatchHighlight.transform.parent.GetSiblingIndex();
        while (counter < patchData.flags.Count)
        {
            index = (index + 1) % patchData.flags.Count;
            if (!patchData.borders[index])
            {
                CurrentPatchHighlight.transform.parent = this.transform.GetChild(index);
                lineRenderer.SetPosition(0, new Vector3(patchData.flags[index].x, 5, patchData.flags[index].z));
                lineRenderer.SetPosition(1, new Vector3(patchData.flags[(index + 1) % patchData.flags.Count].x, 5, patchData.flags[(index + 1) % patchData.flags.Count].z));
                return;
            }

            counter++;
        }

        return;
    }

    // Get currently highlighted edge index
    public int GetHighlightedEdgeIndex()
    {
        return CurrentPatchHighlight.transform.parent.GetSiblingIndex();
    }

    // Remove highlight method
    public void RemoveHighlight()
    {
        Destroy(CurrentPatchHighlight);
        CurrentPatchHighlight = null;
    }

    // Destroy smoothly
    public void Destroy()
    {
        StartCoroutine(SmoothDestroy());
    }
    IEnumerator SmoothDestroy()
    {
        float destroyWaitTime = 0;
        Animation[] anims = this.GetComponentsInChildren<Animation>();
        foreach (Animation anim in anims)
        {
            destroyWaitTime = anim[anim.clip.name].length;
            anim[anim.clip.name].time = anim[anim.clip.name].length;
            anim[anim.clip.name].speed = -1;
            anim.Play();
        }
        yield return new WaitForSeconds(destroyWaitTime);
        PatchesController.instance.patches.Remove(this);
        Destroy(this.gameObject);
    }

}
