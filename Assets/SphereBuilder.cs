using UnityEngine;
using System.Collections;
using AssemblyCSharp;

// <summary>
// Berechnet alle Punkte der Kugel über Tesselation.
// Setzt Bewegungen der Spielfigur auf der Kugel um.
// </summary>
public class SphereBuilder : MonoBehaviour {
	
	private Rink gameArea;							// 2D-Spielfläche;
	
	private int firstLeftCube = 0;					// Die Parzellennr. in gameArea der obersten, linken Parzelle auf dem Cube
													// ( wird stets korrektem Eintrag in 2d-Array gameArea zugeordnet)
	
	private int leftAngleBound, rightAngleBound;
	
	private GameObject camera;						// Kameraobjekt Nr.1, für BewegungsHack; in Update() verwendet
	private GameObject camera2;						// Kameraobjekt Nr.2, für BewegungsHack; in Update() verwendet
		
	public GameObject sphereCube;					// Cube-Prefab, von dem sich alle Cubes der Kugel ableiten

	
	private GameObject []spCubes;					// Array aller Würfel der Kugel


	public Vector3[/*r*/][/*n_L*/][/*n_B*/] sphereVertices;	// 3dim Array für alle Punkte auf der Kugel:
																				// r == 0: Punkte für Unterseite der Würfel
																				// r == 1: Punkte für (sichtbare) Oberseite der Würfel
																				// [n_L][n_B] geben für jeweiligen Längen- und Breitenkreis den Punkt im Raum an
	public float [/*r*/][/*n_L*/][/*n_B*/] vertexAngles;	// Wird _NUR_ für Update-Alternative II benötigt!
	
	
	public int n_B = 8; 							// Auflösung der Breitenkreise; !! >= 4 !!
	public int n_L = 8; 							// Auflösung der Längenkreise ; !! >= 4 !!
	
	public static GameObject deadPlayerPrefab;
	public static GameObject bombPrefab;
	public static GameObject explotionPrefab;
	//public static List<Explosion> explosions;
	
	public static GameObject []players = new GameObject[1];
	public static GameObject player;
	
	// Use this for initialization
	void Start () {
				
		leftAngleBound = 0;
		rightAngleBound = n_B;
		
		adjSouthPole = n_L-2;
		adjEast = 0;
		
		gameArea = new Rink(n_B, n_L, new Vector2(0,0));
		
		// Berechne Punkte und erzeuge Kugel 
		tesselateSphere();	
		
		// Gebe den Würfeln korrekte Höhen gemäß der Werte aus gameArea
		gameArea.updateHeight();

		// Prefabs
		deadPlayerPrefab = GameObject.Find("DeadPlayer");
		bombPrefab = GameObject.Find("bomb");
		explotionPrefab = GameObject.Find("Explotion");
		//explosions = new List<Explosion>();
		
		// Erstelle Pool aus Powerups
		// Mit dem Parameter lässt sich die Poolgröße variieren
		PowerupPool.createPool(1);

	}
	
	private void tesselateSphere(){
	
		// Init des Vertex- Vectors
		sphereVertices = new Vector3[2][][];
		vertexAngles = new float[2][][];
		for(int i = 0; i < 2; i++){
			sphereVertices[i] = new Vector3[n_L][];
			vertexAngles[i] = new float[n_L][];
			verticalOffset = new float[n_L][];
			horizontalOffset = new float[n_L][];
			
			for(int j = 0; j < n_L; j++){
				sphereVertices[i][j] = new Vector3[n_B];	
				vertexAngles[i][j] = new float[n_B];	
				verticalOffset[j] = new float[n_B];
				horizontalOffset[j] = new float[n_B];
			}
		}
		
		// Berechne Punkte
		calcSphereCoordinates();
		
		// Erzeuge Würfel
		initializeCubes();
	}
	
	// <summary>
	// Übergabe der aller Eckpunkte des Würfels über die  _POSITION_ im 3-dim Vertex-Array
	// der Kugel
	// </summary>
	private void calcSphereCoordinates(){
		
		float u,v;		
		Vector3 val;

		for( int j = 0; j < n_L; j++){		// Schleife über alle  Längengeraden
			
			v = (j*Mathf.PI/(n_L-1)) - Mathf.PI/2;
			
			for( int i = 0; i < n_B; i++){	// Schleife über alle Breitengeraden
			
				if ( n_B != 0){				// durch Null teilen ist böse...
					u = (i*2.0f/n_B) * Mathf.PI;
				} else {
					u = 0;	
				}
				
				val = F(u,v);				// berechne Punkt für Winkel u und v

				if ( val != Vector3.zero){
					sphereVertices[0][j][i] = val * 1;	
					sphereVertices[1][j][i] = val * 2;
					
					vertexAngles[0][j][i] = v;
					vertexAngles[1][j][i] = v;
				}				
			}
		}	
	}
	
	// <summary>
	// Tesselationsformel aus Computergrafik;
	// Winkel, der zwischen den Polen hin und her wandert: v in [-PI/2, PI/2]
	// Winkel, der auf einer Höhe um die Kugel wander: u in [0,PI]
	// </summary>
	private Vector3 F(float u, float v){
		
		if ( u < 0 || u > 2*Mathf.PI ||  v < (-1)*Mathf.PI/2 || v > Mathf.PI/2) {
			
			return Vector3.zero;
		}
		
		return new Vector3(	Mathf.Cos(u) * Mathf.Cos(v),
							Mathf.Sin(u) * Mathf.Cos(v),
							Mathf.Sin(v));
	}
	
	// <summary>
	// Belege die Kugeloberfläche mit Würfeln und weise diesen direkt ihren jeweilgs eigenen
	// MeshManipulator zu.
	// </summary>
	private void initializeCubes(){
		
		// Init der Arrays
		spCubes = new GameObject[n_B*(n_L+1)];
		
		// All Cubes in between
		for( int l = 0; l < n_L-1; l++){

			spCubes[l*n_B] = createSphereCube( Vector3.zero);
			spCubes[l*n_B].name = "cube " + l + "," +0;
			gameArea.drawnArea[l][0] = addMeshManipulator(spCubes[l*n_B],
															new Vector3(1,l,1), new Vector3(1,l,0), new Vector3(0,l,1), new Vector3(0,l,0),
															new Vector3(1,l+1,1), new Vector3(1,l+1,0), new Vector3(0,l+1,1), new Vector3(0,l+1,0));
			
			for( int i = 1; i < n_B; i++){
	
				spCubes[l*n_B+i] = createSphereCube( new Vector3(0.0f,0.0f,360.0f/n_B*i));
				spCubes[l*n_B+i].name = "cube " + l + "," +i;
				gameArea.drawnArea[l][i] = addMeshManipulator(spCubes[l*n_B+i],
																	new Vector3(1,l,1), new Vector3(1,l,0), new Vector3(0,l,1), new Vector3(0,l,0),
																	new Vector3(1,l+1,1), new Vector3(1,l+1,0), new Vector3(0,l+1,1), new Vector3(0,l+1,0));
			}
		}	
	}
	
	// <summary>
	// Methode zum erzeugen eines Würfels mit Rotation quaternionEuler
	// </summary>
	public GameObject createSphereCube(Vector3 quaternionEuler){
		
		GameObject c = GameObject.Instantiate(sphereCube, new Vector3(0,0,0), Quaternion.Euler(quaternionEuler)) as GameObject;
		c.renderer.receiveShadows = false;
		c.renderer.castShadows = false;
		
		return c;
	}
	
	// <summary>
	// Methode zum Erzeugen eines MeshManipulators, der zum Würfel c gehört.
	// </summary>
	public MeshManipulator addMeshManipulator(GameObject c,
												Vector3 BNE, Vector3 BNW, Vector3 BSE, Vector3 BSW,
												Vector3 FNE, Vector3 FNW, Vector3 FSE, Vector3 FSW){
		MeshManipulator m = c.AddComponent<MeshManipulator>();
		m.setVertexPosition( BNE, BNW, BSE, BSW, FNE,  FNW,  FSE,  FSW);	// Eckpunkte	
		m.updateCoordinates();
		
		return m;
	}

//----------------------------------------------------------------------
//								 UPDATE aka MOVE
//---------------------------------------------------------------------
	
// Idee: Lasse Würfel tatsächlich zwischen Polen rotieren.
//
// Problem: Rotieren die Würfel beispielsweise vom Süd- zum Nordpol, so bildet sich eine Lücke am Südpol, die immer größer wird,
// je näher die Würfel dem Nordpol kommen. Zugleich ziehen sich die Würfel, wenn sie den Nordpol erreichen zusammen und verschwinden 
// praktisch.
//
// Lösung:
// 1. Würfel, die den Nordpol erreichen, werden an den Südpol verschoben und stellen dort die _gleichen_ Felder der Spielfläche dar;
//	  Wandern jedoch wieder in Richtung des Nordpols
// 2. Würfel, die den östlichen Horizont erreichen, werden analog an den westlichen Horizont verschoben und stellen dort _andere_ Felder dar.
	
	
	float speed = 0.3f;					// Rotationsgeschwindigkeit
	
	int vDirection = 0;					// vertikale Bewegungsrichtung
	int hDirection = 0;					// hoizontale Bewegungsrichtung
		
	float [/*n_L*/][/*n_B*/] verticalOffset;	
	float [/*n_L*/][/*n_B*/] horizontalOffset;	
	
	float offset = 0.0f;
	float hOffset = 0.0f;
	
	float deltaOffset = 0.0f;
	float hDeltaOffset = 0.0f;
	
	int adjSouthPole;
	int adjEast;
	
	bool vChange = false;
	
	// <summary>
	// Handling der Bewegung; Schnittstelle zu InputHandler
	//
	// Bestimme Bewegungsweite und lasse die Kugelpunkte entsprechend "rotieren" ( ist eher ne Verschiebung...)
	// Wenn ein Breiten-, oder Längengerad überschritten wird: 
	// - Schneide Würfel an einer Seite ab und hänge sie vorn wieder an.
	// - Schlage in Rink.cs nach, welche Höhe die Würfel haben müssen ( da sie ne neue Parzelle darstellen könnten)
	// </summary>
	public void move(float moveDirection){
		
		// Bestimme Bewegungsrichtungen und setze ggf. Startwerte für Winkelverschiebung
		
		//if ( vDirection != (int) Mathf.Sign(moveDirection.x) && (int) Mathf.Sign(moveDirection.x) != 0){
		//	vChange = true;	
		//}
		
		vDirection = (int) Mathf.Sign(moveDirection);
		if ( vDirection == 1 && offset == 0.0f){
			offset = -Mathf.PI/(2*(n_L-1));
		} else if ( vDirection == -1 && offset == 0.0f){
			offset = Mathf.PI/(2*(n_L-1));	
		}
		
		//if ( hDirection != (int) Mathf.Sign(moveDirection.y)) hOffset = 0;
		/*hDirection = (int) Mathf.Sign(moveDirection.y);
		if ( hDirection == 1 && hOffset == 0.0f){
			hOffset = -(1.0f/n_B) * Mathf.PI;
		} else if ( hDirection == -1 && hOffset == 0.0f){
			hOffset = (1.0f/n_B) * Mathf.PI;	
		}//*/
		
		
		// Rotiere den Würfel 	
		deltaOffset = speed * moveDirection * Time.deltaTime;
		offset = offset + deltaOffset;
		
		//hDeltaOffset = speed * (-1)*moveDirection.y * Time.deltaTime;
		//hOffset = hOffset + hDeltaOffset;
		
		rotateCubes	();
		
		
		// Würfel-Shift an Darstellungsrändern
		if ( vDirection == 0) return;
		if ( vDirection == 1){
			if (	Mathf.Abs(offset) >= Mathf.PI/(n_L-1) ){
				offset -= Mathf.PI/(n_L-1);		// UPWARD MOVEMENT
				churnOutCubesVertical();
			}
		} else{
			if (	Mathf.Abs(offset) >= Mathf.PI/(n_L-1) ){
				offset += Mathf.PI/(n_L-1);		// DOWNWARD MOVEMENT
				churnOutCubesVertical();
			}
		}
		
		/*if ( hDirection == 0) return;
		if ( hDirection == 1){
			if (	Mathf.Abs(hOffset) >= (2.0f/n_B) * Mathf.PI ){
				hOffset += (2.0f/n_B) * Mathf.PI ;		
				churnOutCubesHorizontal();
			}
		} else{
			if (	Mathf.Abs(hOffset) >= (2.0f/n_B) * Mathf.PI ){
				hOffset -= (2.0f/n_B) * Mathf.PI ;		
				churnOutCubesHorizontal();
			}
		}//*/
	}
	
	// <summary>
	// Verschiebe Würfel bei Vertikalbewegung:
	// Setze Winkel zurück, passe rinkPosition in Rink.cs an und frage die korrekten Höhenwerte nach.
	// </summary>
	private void churnOutCubesVertical(){
				
		
		if ( vDirection == -1){
			gameArea.decrPositionHeight();
		
			if (vChange){
				//adjSouthPole = (adjSouthPole + n_L-2);
				vChange = false;
			}
			Debug.Log("harra");
			// Verschiebe Würfel, die am Nordpol anliegen zum Südpol!
			for( int i = 0; i <  n_B; i++){

				verticalOffset[adjSouthPole] [ i] =  vertexAngles[0][adjSouthPole] [ i]  + Mathf.PI/2;//vertexAngles[0][adjSouthPole][ i] - Mathf.PI/2;	
				
				gameArea.updateHeight(adjSouthPole , i);
			}
			
			
			if ( --adjSouthPole < 0) {
				adjSouthPole = n_L-2;	
			}
		} else if ( vDirection == 1){
			
			//Debug.Log("lololol");
			
			gameArea.incrPositionHeight();
			// Verschiebe Würfel, die am Nordpol anliegen zum Südpol!
			for( int i = 0; i <  n_B; i++){
				
				//Debug.Log((n_L-1 + adjSouthPole+1)%(n_L-1));
				
				verticalOffset[(n_L-1 + adjSouthPole+1)%(n_L-1)] [ i] =  verticalOffset[(n_L-1 + adjSouthPole+1)%(n_L-1)] [ i]  - Mathf.PI;// + Mathf.PI/2;
				gameArea.updateHeight((n_L-1 + adjSouthPole+1)%(n_L-1), i);
			}
			
			
			if ( ++adjSouthPole >  n_L-2) {
				adjSouthPole = 0;	
			}
		}//*/
	}

	
	// <summary>
	// (s.o.) Berechnung der Kugelpunkte;
	// Hier werden sie jeweils um einen offset in Vertikal- oder Horizontalbewegung verschoben.
	// </summary>
	private void rotateCubes(){
		
		float u,v;		
		Vector3 val;

		for( int j = 0; j < n_L; j++){		// Schleife über alle  Längengeraden
			
			v = (j*Mathf.PI/(n_L-1)) - Mathf.PI/2;
						
			for( int i = 0; i < n_B; i++){	// Schleife über alle Breitengeraden
				
				if ( n_B != 0){					// durch Null teilen ist böse...
					u = (i*2.0f/n_B) * Mathf.PI;
				} else {
					u = 0;	
				}
				
				float vr,ur;
				
				verticalOffset[j][i] = ((verticalOffset[j][i]  + deltaOffset) );
				vr = v - verticalOffset[j][i];
				
				//horizontalOffset[j][i] = ((horizontalOffset[j][i]  + hDeltaOffset) );
				//ur = u - horizontalOffset[j][i];
				
				val = F(u,vr);					// berechne Punkt für Winkel u und v

				if ( val != Vector3.zero){
					sphereVertices[0][j][i] = val * 1;	
					sphereVertices[1][j][i] = val * 2;
				}
			}
		}	
		
		gameArea.renderAll();
	}	//*/
	
		public static GameObject[] getPlayers() {
		return players;
	}
}