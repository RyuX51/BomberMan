using System;
using UnityEngine;
using System.Collections.Generic;

namespace AssemblyCSharp
{
	public class Player
	{
		private Parcel currentCell;	// current Parcel
		private float xpos, zpos;	// Player's position in the current Parcel
		
		private float verticalHelper		= 0.0f;
		private float horizontalHelper 	= 0.0f;
		
		private const float MAXSPEED = 0.8f;
		private const float MINDELAY = 0.04f;
		private const float MAXDELAY = 0.28f;
		public const int MAXFLAMEPOWER = 10;
		private const int MAXHP = 100;
		private bool SUPERBOMB = false;
		private bool TRIGGERBOMB = false;
		
		private int bombs = 1;
		private int bombsActive = 0;
		private int flamePower = 1;
		private float speed = 0.4f;
		private float delay = 0.2f;
		private int hp = MAXHP;
		
		private bool dead = false;
		
		private UnityEngine.Object[] icons;
		private String[] iconText;
		
		private List<Parcel> triggerBombs = new List<Parcel>();
		
		public void updateMenuStats() {
			
			/* returns integer array of player attributes
			 * 
			 * bombs = maximum Bombs, the player pocesses
			 *         related icon: Static.bombIconPrefab
			 * speed = player movement speed in ms
			 * 	       related icon: Static.playerSpeedIconPrefab
			 * flamepower = the size of the explosion
			 *         related icon: Static.flameIconPrefab
			 *         icon changes automatically if flame power reaches max and backwards
			 * delay = the shorter the delay the faster the explosion spreads in ms
			 *         related icon: Static.delaySpeedIconPrefab
			 * superbomb = 1 for true, 0 for false
			 *         related icon: Static.superBombIconPrefab
			 *         icon automatically when superbomb is collected and backwards
			 * extras = collected extra which uses the SHIFT key
			 *         related icon: Static.extraIconPrefab
			 *         icon is empty transparent at start and adapts
			 * 
			 * NOTE: icons are 32x32 and transparent
			 */

			icons[0] = Static.bombIconPrefab;
			icons[1] = Static.playerSpeedIconPrefab;
			icons[2] = Static.flameIconPrefab;
			icons[3] = Static.delaySpeedIconPrefab;
			icons[4] = Static.superBombIconPrefab;
			icons[5] = Static.extraIconPrefab;
			
			int extra = TRIGGERBOMB ? 1 : 0;
			//String[] stats = {bombs.ToString(), ((int) speed*1000).ToString(), flamePower.ToString(), ((int) delay*1000).ToString(), SUPERBOMB ? 1 : 0, extra};
			String[] iconText = {
				bombs.ToString(),
				((int) speed*1000).ToString() + " ms",
				flamePower.ToString(),
				((int) delay*1000).ToString() + " ms"
			};
			
			// UPDATE MENU BAR
			// SomeStrangeMenu.updateStats(stats);
			// SomeStrangeMenu.updatePrefabs(prefabs);
		}
		
		public String[] getIconText() {
			return iconText;
		}

		public UnityEngine.Object[] getIcons() {
			return icons;
		}

		public void addTriggerBomb(Parcel cell) {
			triggerBombs.Add(cell);
		}
		
		public List<Parcel> getTriggerBombs() {
			return triggerBombs;
		}
		
		public void powerupCollected(PowerupType type)
		{
			if (type == PowerupType.BOMB_UP) {
				bombs++;
			} else if (type == PowerupType.BOMB_DOWN) {
				if (bombs > 1)
					bombs--;
			} else if (type == PowerupType.FLAME_UP) {
				if (flamePower < MAXFLAMEPOWER)
					flamePower++;
				else {
					Static.setGoldenFlame(true);
					updateMenuStats();
				}
			} else if (type == PowerupType.FLAME_DOWN) {
				if (flamePower == MAXFLAMEPOWER) {
					Static.setGoldenFlame(false);
					updateMenuStats();
				}
				if (flamePower > 1)
					flamePower--;
			} else if (type == PowerupType.PLAYER_SPEED_UP) {
				if (speed < MAXSPEED)
					speed += 0.05f;
			} else if (type == PowerupType.PLAYER_SPEED_DOWN) {
				if (speed > 1.0f)
					speed -= 0.05f;
			} else if (type == PowerupType.DELAY_SPEED_UP) {
				if (delay > MINDELAY)
					delay -= 0.02f;
			} else if (type == PowerupType.DELAY_SPEED_DOWN) {
				if (delay < MAXDELAY)
					speed += 0.02f;
			} else if (type == PowerupType.GOLDEN_FLAME) {
				flamePower = MAXFLAMEPOWER;
				Static.setGoldenFlame(true);
				updateMenuStats();
			} else if (type == PowerupType.SUPERBOMB) {
				SUPERBOMB = true;
				Static.setSuperbomb(true);
				updateMenuStats();
			} else if (type == PowerupType.TRIGGERBOMB) {
				TRIGGERBOMB = true;
				Static.setExtra(1);
				updateMenuStats();
			}
			Debug.Log("bombs: " + bombs + ", flamePower: " + flamePower + ", speed: " + speed*1000 + " ms, delay: " + delay*1000 + " ms");
		}
		
		public bool addBomb() {
			if (bombsActive < bombs) {
				bombsActive++;
				return true;
			} else {
				return false;
			}
		}
		
		public void setXPos(float x){
			if ( x > 1) xpos = 1;
			xpos = x;	
		}
		
		public float getXPos(){
			return xpos;	
		}
		
		public void setZPos(float z){
			if ( zpos > 1) zpos = 1;
			zpos = z;	
		}
		
		public float getZPos(){
			return zpos;	
		}
		
		public void removeBomb() {
			bombsActive--;
			//Debug.Log ("Bombs: " + bombsActive + "/" + bombs);
		}

		public int getFlamePower() {
			return flamePower;
		}

		public int getBombs() {
			return bombs;
		}

		public float getSpeed() {
			return speed;
		}
		
		public void setDead(bool d) {
			dead = d;
			if (d) {
				// Verteile Powerups über das Spielfeld
				List<Parcel> parcelPool = new List<Parcel>();
				Parcel[][] gameArea = Static.gameArea;
				for (int i = 0; i < gameArea.Length; i++) {
					for (int j = 0; j < gameArea[i].Length; j++) {
						if (gameArea[i][j].getType() == 0) {
							parcelPool.Add(gameArea[i][j]);
						}
					}
				}
				parcelPool = shuffleList(parcelPool);
				if (SUPERBOMB) {
					parcelPool[0].addPowerup(new Powerup(PowerupType.SUPERBOMB));
					parcelPool.RemoveAt(0);
					SUPERBOMB = false;
					Static.setSuperbomb(false);
					updateMenuStats();
				}
				if (TRIGGERBOMB) {
					parcelPool[0].addPowerup(new Powerup(PowerupType.TRIGGERBOMB));
					parcelPool.RemoveAt(0);
					TRIGGERBOMB = false;
					Static.setExtra(0);
					updateMenuStats();
				}
				if (flamePower == MAXFLAMEPOWER && parcelPool.Count > 0) {
					parcelPool[0].addPowerup(new Powerup(PowerupType.GOLDEN_FLAME));
					parcelPool.RemoveAt(0);
					bombs = 1;
					Static.setGoldenFlame(false);
					updateMenuStats();
				}
				while (bombs > 1 && parcelPool.Count > 0) {
					parcelPool[0].addPowerup(new Powerup(PowerupType.BOMB_UP));
					parcelPool.RemoveAt(0);
					bombs--;
				}
				while (flamePower > 1 && parcelPool.Count > 0) {
					parcelPool[0].addPowerup(new Powerup(PowerupType.FLAME_UP));
					parcelPool.RemoveAt(0);
					flamePower--;
				}
				while (speed > 0.4f && parcelPool.Count > 0) {
					parcelPool[0].addPowerup(new Powerup(PowerupType.PLAYER_SPEED_UP));
					parcelPool.RemoveAt(0);
					speed -= 0.05f;
				}
				while (delay < 0.2f && parcelPool.Count > 0) {
					parcelPool[0].addPowerup(new Powerup(PowerupType.DELAY_SPEED_UP));
					parcelPool.RemoveAt(0);
					delay += 0.02f;
				}
			}
		}
		
		public bool isDead() {
			return dead;
		}
		
		public void decreaseHP() {
			hp--;
			Debug.Log("Life: " + hp);
			if (hp == 0)
				dead = true;
		}

		public void increaseHP() {
			if (hp < 100) {
				hp++;
				Debug.Log("Life: " + hp);
			}
		}
		
		public int getHP() {
			return hp;
		}

		public int getMaxHP() {
			return MAXHP;
		}
		
		public void setCurrentParcel(Parcel parcel){
			
			/*
			if ( currentCell != null){
				currentCell.hightlightColor(false);	
			}
			*/
			
			currentCell = parcel;	
			
			//currentCell.setColor(Color.cyan);
			//currentCell.hightlightColor(true);
		}
		
		public Parcel getCurrentParcel(){
			return currentCell;	
		}
		
		private List<Parcel> shuffleList(List<Parcel> sortedList)
		{
			List<Parcel> randomList = new List<Parcel>();
			
			System.Random r = new System.Random();
		    int randomIndex = 0;
		    while (sortedList.Count > 0)
		    {
		    	randomIndex = r.Next(0, sortedList.Count);
		    	randomList.Add(sortedList[randomIndex]);
				sortedList.RemoveAt(randomIndex);
			}
			return randomList;
		}
		
		public bool getSuperbomb() {
			return SUPERBOMB;
		}

		public bool getTriggerbomb() {
			return TRIGGERBOMB;
		}

		public float getDelay() {
			return delay;
		}

	}
}

