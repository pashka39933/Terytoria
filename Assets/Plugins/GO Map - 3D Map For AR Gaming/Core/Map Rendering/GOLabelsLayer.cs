using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

namespace GoMap {
	
	[System.Serializable]
	public class GOLabelsLayer {

		public string name {
			get {
				return "Labels";
			}
			set {
				
			}
		}
		public bool useLayerMask = false;

		public string tag;
		public GOStreetnamesSettings streetNames;

		public bool startInactive;
		public bool disabled = true;

		public GOFeatureEvent OnLabelLoad; 

		public string json () {  //Mapzen

			return "";
		}

		public string lyr () { //Mapbox
			return "road_label";
		}

		public string lyr_osm () { //OSM
			return "transportation_name";
		}

		public string lyr_esri () { //Esri
			return "Road/label,Railroad/label,Road tunnel/label,Water area/label,Park or farming/label,Building/label";	
//			return "Road/label,Railroad/label,Road tunnel/label";	
		}
			
		public float defaultLayerY() {
			return 1f;
		}

	}
}
