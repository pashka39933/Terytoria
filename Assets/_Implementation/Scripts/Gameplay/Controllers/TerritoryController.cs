using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerritoryController : MonoBehaviour
{

    // Territory data
    public Territory territoryData;

    // Initialization
    private void OnEnable()
    {
        StartCoroutine(UpdateCoroutine());
    }

    // Update coroutine
    private IEnumerator UpdateCoroutine()
    {
        yield return new WaitUntil(() => territoryData != null && territoryData.corners != null && territoryData.corners.Count > 0);
        for (int i = 1; i <= territoryData.corners.Count; i++)
        {
            int vertex1 = (i - 1) % territoryData.corners.Count;
            int vertex2 = i % territoryData.corners.Count;

            Vector3 vertex1Vec = new Vector3(territoryData.corners[vertex1].x, 0, territoryData.corners[vertex1].y);
            Vector3 vertex2Vec = new Vector3(territoryData.corners[vertex2].x, 0, territoryData.corners[vertex2].y);

            Vector3 particlePosition = new Vector3((vertex1Vec.x + vertex2Vec.x) / 2, 0, (vertex1Vec.z + vertex2Vec.z) / 2);
            float particleRadius = Vector3.Distance(vertex1Vec, vertex2Vec) / 2;

            bool condition = vertex1Vec.x < vertex2Vec.x;
            Vector3 particleRotation = new Vector3(
                0,
                Vector3.Angle(
                    condition ? Vector3.forward : Vector3.back,
                    vertex2Vec - vertex1Vec
                ) + 90,
                0
            );

            ParticleSystem territoryBorderParticle = Instantiate(TerritoriesController.instance.TerritoryBorderParticle, this.transform).GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = territoryBorderParticle.shape;
            shape.position = new Vector3(particlePosition.x, 0, particlePosition.z);
            shape.rotation = particleRotation;
            shape.radius = particleRadius;
            ParticleSystem.MainModule main = territoryBorderParticle.main;
            ParticleSystem.MinMaxGradient color = main.startColor;
            color.color = territoryData.fraction == (int)AppConstants.Fraction.NATURE ? TerritoriesController.instance.natureParticleColor : territoryData.fraction == (int)AppConstants.Fraction.COMMERCY ? TerritoriesController.instance.commercyParticleColor : TerritoriesController.instance.industryParticleColor;
            main.startColor = color;
            Material material = territoryData.fraction == (int)AppConstants.Fraction.NATURE ? TerritoriesController.instance.natureParticleMaterial : territoryData.fraction == (int)AppConstants.Fraction.COMMERCY ? TerritoriesController.instance.commercyParticleMaterial : TerritoriesController.instance.industryParticleMaterial;
            territoryBorderParticle.GetComponent<Renderer>().material = material;
        }
    }

    // Destroy smoothly
    public void Destroy()
    {
        StartCoroutine(SmoothDestroy());
    }
    IEnumerator SmoothDestroy()
    {
        float destroyWaitTime = 0;
        ParticleSystem[] particles = this.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particle in particles)
        {
            destroyWaitTime = particle.main.startLifetime.constant;
            particle.Stop();
        }
        yield return new WaitForSeconds(destroyWaitTime);
        TerritoriesController.instance.territories.Remove(this);
        Destroy(this.gameObject);
    }

}
