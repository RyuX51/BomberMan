using UnityEngine;
using System.Collections;

namespace AssemblyCSharp
{
	public class inGame : MonoBehaviour {
		
		void Update () {
			if (Application.loadedLevelName != "SphereCreate")
				return;
			
			if (Input.GetKeyDown(KeyCode.Escape)) {
				Menu.showGUI = !Menu.showGUI;
				if (Menu.showGUI) {
					Static.inputHandler.lockCursor(false);
					Static.camera.GetComponent<MouseLookGame>().setRotatable(false);
				}
				else {
					Static.inputHandler.lockCursor(true);
					Static.camera.GetComponent<MouseLookGame>().setRotatable(true);
				}
			}
		}
		
		public static bool focusToChat = true;
		void OnGUI () {
			if (Application.loadedLevelName != "SphereCreate")
				return;
			
			if (GUI.Button(new Rect(10,10,80,20), "MENU")) {
				Menu.showGUI = !Menu.showGUI;
			}
			
			if (Event.current.type == EventType.KeyDown && (
					Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)) {
				if (focusToChat) {
					GUI.FocusControl("chat");
				} else {
					GUI.SetNextControlName("game");
	            	GUI.Label(new Rect(-100, -100, 1, 1), "");
	            	GUI.FocusControl("game");
				}
				focusToChat = !focusToChat;
			}
			
			Menu.instance.chatArea();
		}
	}
}