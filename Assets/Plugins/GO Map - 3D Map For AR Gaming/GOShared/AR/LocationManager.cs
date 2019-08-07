using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace GoShared {

	public class LocationManager : MonoBehaviour {

		public enum DemoLocation{
			NewYork, 
			Rome,
			NewYork2,
			Venice,
			SanFrancisco,
			Berlin,
			RioDeJaneiro,
			Paris,
			Budapest,
			GrandCanyon,
			Matterhorn,
			London,
			SearchMode,
			NoGPSTest,
			Custom
		};

		public enum MotionPreset{
			Run, 
			Bike,
			Car
		};

		public enum MotionMode{
			Avatar, 
			GPS
		};

		public enum MotionState{
			Idle, 
			Walk,
			Run
		};
		[HideInInspector] public MotionState currentMotionState = MotionState.Idle;
		float currentSpeed = 0;
		List<Coordinates> lastLocations  = new List<Coordinates>();

		public bool useLocationServices;
		public int zoomLevel = 16;
		public float worldScale = 1;

		public DemoLocation demoLocation;
		public Coordinates demo_CenterWorldCoordinates;
		[HideInInspector]
		public Vector2 demo_CenterWorldTile;

	//	[HideInInspector]
		public Coordinates currentLocation;

		[HideInInspector]
		public static Coordinates CenterWorldCoordinates;

		public float desiredAccuracy = 50;
		public float updateDistance = 0.1f;

		[HideInInspector]
		public float updateEvery = 1 / 1000f;

		public MotionPreset simulateMotion = MotionPreset.Run;
		float demo_WASDspeed = 20;
		public bool useWsadInEditor = true;

		public MotionMode motionMode = MotionMode.GPS;
		public GameObject avatar;

		public bool useBannerInsideEditor;
		public GameObject banner;
		public Text bannerText;

		public static bool IsOriginSet;
		public static bool UseLocationServices;
		public static LocationServiceStatus status;

		public GOLocationEvent onOriginSet;
//		public delegate void OnOriginSet(Coordinates origin);

		public GOLocationEvent onLocationChanged;
//		public delegate void OnLocationChanged(Coordinates current);

		public GOMotionStateEvent OnMotionStateChanged;

		// Use this for initialization
		void Start () {

			if (Application.isEditor || !Application.isMobilePlatform) {
				useLocationServices = false;
			}

			switch (motionMode)
			{
			case MotionMode.Avatar:
				LoadDemoLocation ();
				updateEvery = 1;
				StartCoroutine(LateStart(0.01f));
				break;
			case MotionMode.GPS:

				if (useLocationServices) {
					Input.location.Start (desiredAccuracy, updateDistance);
				} else { //Demo origin
					LoadDemoLocation ();
				}
				UseLocationServices = useLocationServices;
				updateEvery = 0.1f;
				StartCoroutine(LateStart(0.01f));
				break;
			default:
				break;
			}
		}
			
		IEnumerator LateStart(float waitTime)
		{
			yield return new WaitForSeconds(waitTime);
			if (!useLocationServices && demoLocation != DemoLocation.NoGPSTest && demoLocation != DemoLocation.SearchMode) {
				adjust (); //This adjusts the current location just after the initialization
			}
		}


		float tempTime;
		public void Update () {

			Profiler.BeginSample("[LocationManager] Update");
			tempTime += Time.deltaTime;
			if (tempTime > updateEvery ) {
				tempTime -= updateEvery;
				switch (motionMode)
				{
				case MotionMode.Avatar:
					AvatarPositionCheck ();
					break;
				case MotionMode.GPS:
					GPSLocationCheck ();
					break;
				default:
					break;
				}
			}
			Profiler.EndSample ();

		}
			


		void SetOrigin(Coordinates coords) {
			Debug.Log ("SET ORIGIN "+coords.toLatLongString());
			IsOriginSet = true;
			CenterWorldCoordinates = coords.tileCenter(zoomLevel);
			demo_CenterWorldTile = coords.tileCoordinates(zoomLevel);
			Coordinates.setWorldOrigin (CenterWorldCoordinates,worldScale);
			if (onOriginSet != null) {
				onOriginSet.Invoke (CenterWorldCoordinates);
			}
		}

		public IEnumerator WaitForOriginSet()
		{
			while(!IsOriginSet)
			{
				yield return null;
			}
		}

		void adjust () {

			Vector3 current = currentLocation.convertCoordinateToVector ();
			Vector3 v = current;
			currentLocation = Coordinates.convertVectorToCoordinates (v);
//			v = current + new Vector3(0, 0 , 0.1f)*worldScale;
			currentLocation = Coordinates.convertVectorToCoordinates (v);

			switch (motionMode) {
			case MotionMode.Avatar:
				if (onOriginSet != null) {
					onOriginSet.Invoke (currentLocation);
				}
				break;
			case MotionMode.GPS:
				if (onLocationChanged != null) {
					onLocationChanged.Invoke (currentLocation);
				}
				break;
			default:
				break;
			}
		}

		#region Location Updates

		void GPSLocationCheck () {

			status = Input.location.status;

			if (!useLocationServices) {
				if (Application.isEditor && useBannerInsideEditor)
					showBannerWithText (true, "GPS jest wyłączony");
			}
			else if (status == LocationServiceStatus.Failed) {
				showBannerWithText (true, "Brak sygnału GPS");
			}
			else if (status == LocationServiceStatus.Stopped) {
                showBannerWithText (true, "Brak sygnału GPS");
			}
			else if (status == LocationServiceStatus.Initializing) {
				showBannerWithText (true, "Czekam na sygnał GPS");
			} 
			else if (status == LocationServiceStatus.Running) {

				if (Input.location.lastData.horizontalAccuracy > desiredAccuracy) {
					showBannerWithText (true, "Słaby sygnał GPS");
				} else {
					showBannerWithText (false, "Sygnał GPS ok!");

					if (!IsOriginSet) {
						SetOrigin (new Coordinates (Input.location.lastData));
					}
					LocationInfo info = Input.location.lastData;
					if (info.latitude != currentLocation.latitude || info.longitude != currentLocation.longitude) {
						currentLocation.updateLocation (Input.location.lastData);
						if (onLocationChanged != null) {
							onLocationChanged.Invoke (currentLocation);
						}
					}
					CheckMotionState (new Coordinates(Input.location.lastData));

				}
			}

			if (Application.platform == RuntimePlatform.WebGLPlayer)
				changeLocationWASD ();
			if (!useLocationServices && (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer) && demoLocation != DemoLocation.NoGPSTest && demoLocation != DemoLocation.SearchMode && !GOUtils.IsPointerOverUI() && motionMode != MotionMode.Avatar) {
				changeLocationWASD ();
			}

		}

		void AvatarPositionCheck () {

			if (CenterWorldCoordinates != null && !CenterWorldCoordinates.isZeroCoordinates()) {
				currentLocation = Coordinates.convertVectorToCoordinates (avatar.transform.position);
				if (onLocationChanged != null) {
					onLocationChanged.Invoke (currentLocation);
				}
			}

		}

		#endregion;

		#region MOTION STATE

		void CheckMotionState (Coordinates lastLocation) {

			MotionState state = currentMotionState;

			if (lastLocations.Count>0 && lastLocation.Equals(lastLocations[lastLocations.Count-1])){
				state = MotionState.Idle;
			} else {

				lastLocations.Add (lastLocation);
				int max = 10;
				if (lastLocations.Count == max+1) {
					lastLocations.RemoveAt (0);
				} 
			
				//Speed is returned in m/s
				currentSpeed = GPSSpeedUtils.GetSpeedFromCoordinatesList (lastLocations);
				if (currentSpeed < 0.5f) {
					state = MotionState.Idle;
				} else if (currentSpeed < 3f) {
					state = MotionState.Walk;
				} else {
					state = MotionState.Run;
				}
			}
				
			if (state != currentMotionState) {
				currentMotionState = state;
				if (OnMotionStateChanged != null) {
					OnMotionStateChanged.Invoke (currentMotionState);
				}			
			}

		}

		#endregion


		#region UI

		////UI
		void showBannerWithText(bool show, string text) {

			if (banner == null || bannerText == null) {
				return;
			}

			bannerText.text = text;

			RectTransform bannerRect = banner.GetComponent<RectTransform> ();
			bool alreadyOpen = bannerRect.anchoredPosition.y != bannerRect.sizeDelta.y;

			if (show != alreadyOpen) {
				StartCoroutine (Slide (show, 1));
			}

		}

		private IEnumerator Slide(bool show, float time) {

//			Debug.Log ("Toggle banner");

			Vector2 newPosition;
			RectTransform bannerRect = banner.GetComponent<RectTransform> ();

			if (show) {//Open
				newPosition = new Vector2 (bannerRect.anchoredPosition.x, 0);
			} else { //Close
				newPosition = new Vector2 (bannerRect.anchoredPosition.x, bannerRect.sizeDelta.y);
			} 

			float elapsedTime = 0;
			while (elapsedTime < time)
			{
				bannerRect.anchoredPosition = Vector2.Lerp(bannerRect.anchoredPosition, newPosition, (elapsedTime / time));
				elapsedTime += Time.deltaTime;
				yield return new WaitForEndOfFrame();
			}
				
		}

//		public void OnGUI () {
//
//			GUIStyle guiStyle = new GUIStyle(); 
//			guiStyle.fontSize = 30; 
//			GUILayout.Label(currentSpeed + " "+currentMotionState.ToString(), guiStyle);
//
//		}

		#endregion


		#region GPS MOTION TEST

		void changeLocationWASD (){

			if (!useWsadInEditor)
				return;

			switch (simulateMotion)
			{
			case MotionPreset.Car:
				demo_WASDspeed = 4;
				break;
			case MotionPreset.Bike:
				demo_WASDspeed = 2;
				break;
			case MotionPreset.Run:
				demo_WASDspeed = 0.8f;
				break;
			default:
				break;
			}


			Vector3 current = currentLocation.convertCoordinateToVector ();
			Vector3 v = current;

			if (Input.GetKey (KeyCode.W)){
				v = current + new Vector3(0, 0 , demo_WASDspeed);
			}
			if (Input.GetKey (KeyCode.S)){
				v = current + new Vector3(0, 0 , -demo_WASDspeed);
			}
			if (Input.GetKey (KeyCode.A)){
				v = current + new Vector3(-demo_WASDspeed, 0 , 0);
			}
			if (Input.GetKey (KeyCode.D)){
				v = current + new Vector3(demo_WASDspeed, 0 , 0);
			}

			if (!v.Equals(current)) {
				currentLocation = Coordinates.convertVectorToCoordinates (v);
				if (onLocationChanged != null) {
					onLocationChanged.Invoke (currentLocation);
				}
			}
			CheckMotionState (currentLocation);

		}

		#endregion

		#region Search Mode

		public void SetLocation (Coordinates newLocation) {
		
			SetOrigin (newLocation);
			currentLocation = newLocation;
			adjust ();
		}

		#endregion

		#region DEMO LOCATIONS

		public void LoadDemoLocation () {

			switch (demoLocation)
			{
			case DemoLocation.NewYork:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (40.783435,-73.966249,0);
				break;
			case DemoLocation.NewYork2:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (40.70193632375534,-74.01628977185595,0);
				break;
			case DemoLocation.Rome:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (41.910509366663945,12.476284503936768,0);
				break;
			case DemoLocation.Venice:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (45.433184, 12.336831,0);
				break;
			case DemoLocation.SanFrancisco:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (37.8019180297852, -122.419631958008,0);
				break;
			case DemoLocation.Berlin:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (52.521123, 13.409396,0);
				break;
			case DemoLocation.RioDeJaneiro:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (-22.9638023376465, -43.1685562133789,0);
				break;
//			case DemoLocation.Dubai:
//				demo_CenterWorldCoordinates = currentLocation = new Coordinates (25.197469, 55.274366,0);
//				break;
			case DemoLocation.Budapest:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (47.50261987827267,19.039907455444336,0);
				break;
			case DemoLocation.Paris:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (48.873769, 2.294745,0);
				break;
			case DemoLocation.GrandCanyon:
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (36.0979385375977, -112.066040039063,0);
				break;
			case DemoLocation.Matterhorn :
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (45.976574,7.6562632,0);
				break;
			case DemoLocation.London :
				demo_CenterWorldCoordinates = currentLocation = new Coordinates (51.5129522,-0.0982975,0);
				break;
			case DemoLocation.NoGPSTest:
				currentLocation = demo_CenterWorldCoordinates = null;
				return;
			case DemoLocation.SearchMode:
				currentLocation = demo_CenterWorldCoordinates = null;
				return;

			case DemoLocation.Custom:
				currentLocation = demo_CenterWorldCoordinates;
				break;
			default:
				break;
			}

			if (demo_CenterWorldCoordinates != null)
				SetOrigin(demo_CenterWorldCoordinates);

		}

		#endregion

	}

	[Serializable]
	public class GOMotionStateEvent : UnityEvent <GoShared.LocationManager.MotionState> {


	}

	[Serializable]
	public class GOLocationEvent : UnityEvent <GoShared.Coordinates> {


	}
}