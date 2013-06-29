using System;
using UnityEngine;

namespace AssemblyCSharp
{
	public static class Static
	{
		public static SphereBuilder sphereHandler;
		public static Rink rink;
		public static Parcel[][] gameArea;
		public static MeshManipulator[][] drawnArea;
		//public static GameObject bombPrefab;
		public static GameObject bombUpPowerupPrefab;
		public static GameObject bombDownPowerupPrefab;
		public static GameObject flameUpPowerupPrefab;
		public static GameObject flameDownPowerupPrefab;
		public static GameObject goldenFlamePowerupPrefab;
		public static GameObject playerSpeedUpPowerupPrefab;
		public static GameObject playerSpeedDownPowerupPrefab;
		public static GameObject explosionPrefab;
		public static GameObject deadPlayerPrefab;
		
		static Static() {
			//bombPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/bombPrefab"));
			bombUpPowerupPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/bombUpPrefab"));
			bombDownPowerupPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/bombDownPrefab"));
			flameUpPowerupPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/flameUpPrefab"));
			flameDownPowerupPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/flameDownPrefab"));
			goldenFlamePowerupPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/goldenFlamePrefab"));
			playerSpeedUpPowerupPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/playerSpeedUpPrefab"));
			playerSpeedDownPowerupPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/playerSpeedDownPrefab"));
			explosionPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/explosionPrefab"));
			deadPlayerPrefab = (GameObject) UnityEngine.Object.Instantiate(Resources.Load("Prefabs/deadPlayerPrefab"));
		}
		
		public static void setSphereBuilder(SphereBuilder s) {
			sphereHandler = s;
		}
		
		
		public static void setRink(Rink r) {
			rink = r;
			gameArea = rink.getGameArea();
			drawnArea = rink.drawnArea;
		}
	}
}

