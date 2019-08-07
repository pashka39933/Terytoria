using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

namespace GoMap {

	public class GOEnvironmentPro : MonoBehaviour {

		public GOMap goMap;
		public GOEnvironmentKind[] floatingEnvironment;
		public GOEnvironmentKind[] featureEnvironment;

		// Use this for initialization
		void Awake () {
			
			goMap.OnTileLoad.AddListener((GOTile) => {OnTileLoad(GOTile);});
			foreach (GOLayer layer in goMap.layers) {
				layer.OnFeatureLoad.AddListener((GOFeature,GameObject) => {OnFeatureLoad(GOFeature,GameObject);});
			}
		}

		public void OnTileLoad (GOTile tile) {

			foreach (GOEnvironmentKind kind in floatingEnvironment) {
			
				int spawn = 0;
				if (spawn == 0) {
					if (Application.isPlaying)
						StartCoroutine (SpawnPrefabsInTile (tile, tile.gameObject, kind));
					else
						GORoutine.start (SpawnPrefabsInTile (tile, tile.gameObject, kind), this);
				}
			}
		}

		public void OnFeatureLoad (GOFeature feature, GameObject featureObject) {

			foreach (GOEnvironmentKind kind in featureEnvironment) {

				int spawn = 0;
				if (spawn == 0 && kind.kind == feature.kind) {
					if (Application.isPlaying)
						StartCoroutine (SpawnPrefabsInMesh (feature, featureObject, kind));
					else
						GORoutine.start (SpawnPrefabsInMesh (feature, featureObject, kind), this);
				}
			}
		}


		public IEnumerator SpawnPrefabsInTile (GOTile tile, GameObject parent, GOEnvironmentKind kind) {

			if (tile == null)
				yield break;

			float rate = Mathf.FloorToInt(tile.goTile.diagonalLenght * (kind.density/100));


			for (int i = 0 ; i<=rate; i++) {
				
				float randomX = UnityEngine.Random.Range (tile.goTile.getXRange().x, tile.goTile.getXRange().y);
				float randomZ = UnityEngine.Random.Range (tile.goTile.getZRange().x, tile.goTile.getZRange().y);
				float randomY = UnityEngine.Random.Range (200, 320);

				int n = UnityEngine.Random.Range (0, kind.prefabs.Length);
				Vector3 pos = new Vector3 (randomX,randomY,randomZ);

				pos.y += kind.prefabs [n].transform.position.y;

				pos.y += GOMap.AltitudeForPoint (pos);

				var rotation = kind.prefabs [n].transform.eulerAngles;
				var randomRotation =  new Vector3( 0 , UnityEngine.Random.Range(0, 360) , 0);

				GameObject obj =  (GameObject)GameObject.Instantiate (kind.prefabs[n], pos, Quaternion.Euler(rotation+randomRotation));
				obj.transform.parent = parent.transform;
				obj.transform.position = pos;
				if (Application.isPlaying)
					yield return null;
			}

			yield return null;
		}

		public IEnumerator SpawnPrefabsInMesh (GOFeature feature, GameObject parent, GOEnvironmentKind kind) {

			if (feature.preloadedMeshData == null)
				yield break;

			int rate = 100 / kind.density;

			foreach (Vector3 vertex in feature.preloadedMeshData.vertices) {

				try {
					int spawn = UnityEngine.Random.Range (0, rate);
					if (spawn != 0)
						continue;

					int n = UnityEngine.Random.Range (0, kind.prefabs.Length);
					Vector3 pos = vertex;

					if(GOMap.IsPointAboveWater(pos))
						continue;

					pos.y += kind.prefabs [n].transform.position.y;


					var rotation = kind.prefabs [n].transform.eulerAngles;
					var randomRotation =  new Vector3( 0 , UnityEngine.Random.Range(0, 360) , 0);

					GameObject obj =  (GameObject)GameObject.Instantiate (kind.prefabs[n], pos, Quaternion.Euler(rotation+randomRotation));
					obj.transform.parent = parent.transform;

				} catch {
				}

				if (Application.isPlaying)
					yield return null;
			}

			yield return null;
		}


	}

	[ExecuteInEditMode]
	[System.Serializable]
	public class GOEnvironmentKind {

		public GOFeatureKind kind;
		public GameObject[] prefabs;
		[Range(0, 100)] public int density;

	}
}