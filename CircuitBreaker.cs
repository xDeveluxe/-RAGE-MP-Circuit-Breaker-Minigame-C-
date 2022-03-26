using System;
using System.Collections.Generic;
using System.Linq;
using RAGE;

namespace CircuitBreaker
{
	class CircuitBreaker : Events.Script
	{
		public CircuitBreaker()
        {
			Events.Tick += Tick;
		}

		public static CbGameStatusEnum Status { get; private set; } = CbGameStatusEnum.None;

		private static List<List<Vector2>> BlockedAreas = new List<List<Vector2>>();
		private static readonly List<Vector2> GameBounds = new List<Vector2>
		{
			new Vector2( 0.159f, 0.153f ),	// Top Left
			new Vector2( 0.159f, 0.848f ),	// Bottom Left
			new Vector2( 0.841f, 0.848f ),	// Bottom Right
			new Vector2( 0.841f, 0.153f )	// Top Right
		};
		private static readonly string[] TextureDictionaries = { "MPCircuitHack", "MPCircuitHack2", "MPCircuitHack3" };

		private static float CurrentPointSpeed = 0.00085f;

		private static RAGE.Game.Scaleform Scaleform = null;

		private static DateTime StartTime = DateTime.MinValue;
		private static DateTime EndTime = DateTime.MinValue;

		private static int SoundID = -1;

		private static List<int> AvailableLevels = new List<int>()
		{
			1, 2, 3, 4, 5, 6
		};
		private static List<int> LevelsToComplete = new List<int>();
		private static int Level = 1;
		private static int LivesLeft = 1;
		private static CbDifficultyLevelEnum Difficulty = CbDifficultyLevelEnum.Beginner;

		private static bool Disconnected = false;
		private static float DisconnectChanse = 0f;
		private static int DisconnectCheckRateMs = 0;
		private static DateTime NextTimeCheckDisconnect = DateTime.MinValue;
		private static DateTime ReconnectIn = DateTime.MinValue;

		/// <summary>
		/// Запустить игру на определённое количество уровней
		/// </summary>
		/// <param name="lives">Количество жизней на игру (1-10)</param>
		/// <param name="difficulty">Уровень сложности  на игру (0-3)</param>
		/// <param name="levels">Количество уровней (1-6)</param>
		public static void StartMinigame(int lives, int difficulty, int levels = 1)
        {
			if (Status != CbGameStatusEnum.None) return;
			Status = CbGameStatusEnum.Starting;
			ResetEverything();

			levels = Math.Clamp(levels, 1, 6);
			FillLevels(levels);
			Level = GetLevel();
			LivesLeft = Math.Clamp(lives, 1, 10);
			Difficulty = (CbDifficultyLevelEnum)Math.Clamp(difficulty, 0, 4);
			CurrentPointSpeed = GetPointSpeedFromDifficulty(Difficulty);
			DisconnectChanse = GetDisconnectChanceFromDifficulty(Difficulty);
			DisconnectCheckRateMs = GetDisconnectCheckRateMsFromDifficulty(Difficulty);
			Init();
		}

		private static void ResetEverything()
        {
			BlockedAreas = new List<List<Vector2>>();
			ResetResources();
			StartTime = DateTime.MinValue;
			EndTime = DateTime.MinValue;
			AvailableLevels = new List<int>() { 1, 2, 3, 4, 5, 6 };
			LevelsToComplete = new List<int>();
			Level = 1;
			LivesLeft = 1;
			Difficulty = CbDifficultyLevelEnum.Beginner;
			CurrentPointSpeed = 0.00085f;
			Disconnected = false;
			DisconnectChanse = 0f;
			DisconnectCheckRateMs = 0;
			NextTimeCheckDisconnect = DateTime.MinValue;
			ReconnectIn = DateTime.MinValue;
		}

		private static void ResetResources()
        {
			ResetScaleform();
			ResetTextureDictionaries();
			ResetSounds();

		}

		private static int GetLevel()
        {
			if (LevelsToComplete.Count == 0) return 1;
			int Level = LevelsToComplete[0];
			LevelsToComplete.Remove(Level);
			return Level;
        }

		private static void FillLevels(int count)
        {
			if (count < 1) return;
			for (int i = 0; i != count; i++) LevelsToComplete.Add(GetRandomLevel());
        }

		private static int GetRandomLevel()
        {
			if (AvailableLevels.Count == 0) return 1;

			int number = Helpers.GetRange(0, AvailableLevels.Count);
			int randomLevel = AvailableLevels.ElementAt(number);
			AvailableLevels.Remove(randomLevel);
			return randomLevel;
		}

		private static bool GameDraw()
        {
			if (Scaleform == null) return false;
			DrawMapSprite(Level);
			bool CollisionHit = DrawPointAndPortSprites();
			Scaleform.Render2D();
			return CollisionHit;
		}

		private static bool CheckDisconnect(DateTime now)
        {
			if (now >= NextTimeCheckDisconnect)
            {
				Disconnected = Helpers.GetRange() <= DisconnectChanse;
				return Disconnected;
            }
			return false;
        }

		private static void InProcessLogic(bool collisionHit)
        {
			if (ExitButtonPressed())
			{
				EndGame(true);
				return;
			}

			DateTime now = DateTime.Now;

			if (now < StartTime) return;

			if (DisconnectChanse > 0f && CheckDisconnect(now)) 
			{
				Status = CbGameStatusEnum.Disconnected;
				return;
			}

			if (CbGenericPorts.IsPointInGameWinningPosition(CbPoint.Position))
            {
				Status = CbGameStatusEnum.Success;
				return;
            }

			if (IsPointOutOfBounds(BlockedAreas, GameBounds))
            {
				Status = CbGameStatusEnum.FailureOutOfBounds;
				return;
            }

			if (CbGenericPorts.IsCollisionWithPort(CbPoint.Position))
            {
				Status = CbGameStatusEnum.FailureCollisionWithPort;
				return;
            }

			if (collisionHit)
            {
				Status = CbGameStatusEnum.FailureTrailCollision;
				return;
            }

			if (CbPoint.IsAlive)
            {
				CbPoint.GetPointInputFromPlayer();
				CbPoint.MovePoint(CurrentPointSpeed);
            }
		}

		private static void SuccessLogic()
        {
			DateTime now = DateTime.Now;
			if (EndTime == DateTime.MinValue)
			{
				ShowSuccessScreenAndPlaySound();
				EndTime = DateTime.Now.AddSeconds(3);
				return;
			}

			if (now < EndTime) return;

			if (CheckLevelsToPlay())
			{
				ContinueGame();
				return;
			}
			Status = CbGameStatusEnum.Quit;
			Events.CallLocal("CircuitBreakerWIN");
		}

		private static void RestartSameLevel()
        {
			Status = CbGameStatusEnum.Death;
			ShowDeathScreenAndPlaySound(LivesLeft);
			CbPoint.Initialize();
			StartTime = DateTime.Now.AddSeconds(3);
			NextTimeCheckDisconnect = StartTime.AddMilliseconds(DisconnectCheckRateMs);
		}

		/// <summary>
		/// reason just for debug
		/// </summary>
		/// <param name="reason">1 - Out of map or blocked areas, 2 - wrong port collision, 3 - hit in trail</param>
		private static void FailureLogic(int reason)
        {
			LivesLeft--;

			if (LivesLeft > 0)
            {
				RestartSameLevel();
				return;
            }

			if (CbPoint.IsAlive) CbPoint.StartPointDeathAnimation();
			if (!CbPoint.IsVisible)
            {
				DateTime now = DateTime.Now;
				if (EndTime == DateTime.MinValue)
                {
					ShowFailureScreenAndPlaySound();
					EndTime = DateTime.Now.AddSeconds(3);
					return;
				}
				if (now >= EndTime)
				{
					Status = CbGameStatusEnum.Quit;
					Events.CallLocal("CircuitBreakerLOSE");
					return;
				}
			}
        }

		private static void DeathLogic()
        {
			DateTime now = DateTime.Now;
			if (now >= StartTime)
            {
				PlayStartSound();
				ResetDisplayScaleform();
				Status = CbGameStatusEnum.InProcess;
				return;
            }
        }

		private static void DisconnectLogic()
        {
			if (ReconnectIn == DateTime.MinValue) 
			{
				RAGE.Game.Audio.PlaySoundFrontend(-1, "Power_Down", "DLC_HEIST_HACKING_SNAKE_SOUNDS", true);
				ShowDisplayScaleform("CONNECTION LOST", "Reconnecting...", CbColors.RedColor.R, CbColors.RedColor.G, CbColors.RedColor.B, false);
				ReconnectIn = DateTime.Now.AddMilliseconds(Helpers.GetRange(500, 5000));
				return;
			}
			DateTime now = DateTime.Now;
			if (now >= ReconnectIn)
			{
				PlayStartSound();
				ResetDisplayScaleform();
				Status = CbGameStatusEnum.InProcess;
				ReconnectIn = DateTime.MinValue;
				NextTimeCheckDisconnect = now.AddMilliseconds(DisconnectCheckRateMs);
				return;
			}
		}

		private static void Tick(List<Events.TickNametagData> _)
        {
			if (Status == CbGameStatusEnum.None) return;

			DisableControls();
			bool CollisionHit = GameDraw();

			switch (Status)
            {
				case CbGameStatusEnum.InProcess:
					InProcessLogic(CollisionHit);
					break;
				case CbGameStatusEnum.Success:
					SuccessLogic();
					break;
				case CbGameStatusEnum.FailureOutOfBounds:
					FailureLogic(1);
					break;
				case CbGameStatusEnum.FailureCollisionWithPort:
					FailureLogic(2);
					break;
				case CbGameStatusEnum.FailureTrailCollision:
					FailureLogic(3);
					break;
				case CbGameStatusEnum.Death:
					DeathLogic();
					break;
				case CbGameStatusEnum.Disconnected:
					DisconnectLogic();
					break;
				case CbGameStatusEnum.Quit:
					EndGame(false);
					break;
				default:
					break;
            }
        }

		private static void EndGame(bool exit)
        {
			if (exit) Events.CallLocal("CircuitBreakerLOSE");
			Status = CbGameStatusEnum.None;
			ResetEverything();
        }

		private static bool CheckLevelsToPlay()
        {
			if (LevelsToComplete.Count == 0) return false;
			return true;
        }

		private static void ContinueGame()
        {
			ResetDisplayScaleform();
			Level = GetLevel();

			BlockedAreas = CbMapBoundaries.GetBoxBounds(Level);
			CbGenericPorts.Initialize(Level);
			CbPoint.Initialize();
			StartTime = DateTime.Now.AddSeconds(3);
			NextTimeCheckDisconnect = StartTime.AddMilliseconds(DisconnectCheckRateMs);
			EndTime = DateTime.MinValue;

			PlayStartSound();
			Status = CbGameStatusEnum.InProcess;
		}

		private static void Init() 
		{
			if (Status != CbGameStatusEnum.Starting) return;
			RAGE.Task.Run(async () =>
            {
				await LoadResources();

				SoundID = RAGE.Game.Audio.GetSoundId();
				RAGE.Game.Audio.PlaySoundFrontend(SoundID, "Background", "DLC_HEIST_HACKING_SNAKE_SOUNDS", true);

				BlockedAreas = CbMapBoundaries.GetBoxBounds(Level);
				CbGenericPorts.Initialize(Level);
				CbPoint.Initialize();
				StartTime = DateTime.Now.AddSeconds(3);
				NextTimeCheckDisconnect = StartTime.AddMilliseconds(DisconnectCheckRateMs);

				PlayStartSound();
				Status = CbGameStatusEnum.InProcess;
			});
		}

		private static async System.Threading.Tasks.Task LoadResources()
		{
			await LoadTextures();
			await LoadScaleform();
		}

		private static async System.Threading.Tasks.Task LoadTextures()
		{
			foreach (string dict in TextureDictionaries)
			{
				RAGE.Game.Graphics.RequestStreamedTextureDict(dict, false);
				while (!RAGE.Game.Graphics.HasStreamedTextureDictLoaded(dict)) await RAGE.Game.Invoker.WaitAsync(5);
			}
		}
		private static void ResetTextureDictionaries()
		{
			foreach (string dict in TextureDictionaries) RAGE.Game.Graphics.SetStreamedTextureDictAsNoLongerNeeded(dict);
		}

		private static void ResetSounds()
		{
			if (SoundID == -1) return;

			RAGE.Game.Audio.StopSound(SoundID);
			RAGE.Game.Audio.ReleaseSoundId(SoundID);
			SoundID = -1;
		}
		private static void PlayStartSound()
		{
			RAGE.Game.Audio.PlaySoundFrontend(-1, "Start", "DLC_HEIST_HACKING_SNAKE_SOUNDS", true);
		}

		private static void ResetDisplayScaleform()
        {
			if (Scaleform == null) return;
			Scaleform.CallFunction("SET_DISPLAY", -1);
		}

		private static void ShowDisplayScaleform(string title, string msg, int r, int g, int b, bool stagePassed)
		{
			if (Scaleform == null) return;
			Scaleform.CallFunction("SET_DISPLAY", 0, title, msg, r, g, b, stagePassed);
		}
		private static void ShowSuccessScreenAndPlaySound()
		{
			RAGE.Game.Audio.PlaySoundFrontend(-1, "Success", "DLC_HEIST_HACKING_SNAKE_SOUNDS", true);
			//RAGE.Game.Audio.PlaySoundFrontend(-1, "Goal", "DLC_HEIST_HACKING_SNAKE_SOUNDS", true);
			ShowDisplayScaleform("CIRCUIT COMPLETE", "Decryption Execution x86 Tunneling", CbColors.GreenColor.R, CbColors.GreenColor.G, CbColors.GreenColor.B, true);
		}

		private static void ShowFailureScreenAndPlaySound()
		{
			RAGE.Game.Audio.PlaySoundFrontend(-1, "Crash", "DLC_HEIST_HACKING_SNAKE_SOUNDS", true);
			ShowDisplayScaleform("CIRCUIT FAILED", $"Security Tunnel Detected", CbColors.RedColor.R, CbColors.RedColor.G, CbColors.RedColor.B, false);
		}

		private static void ShowDeathScreenAndPlaySound(int lives)
		{
			RAGE.Game.Audio.PlaySoundFrontend(-1, "Crash", "DLC_HEIST_HACKING_SNAKE_SOUNDS", true);
			ShowDisplayScaleform("CIRCUIT FAILED", $"{lives} Attempts Left", CbColors.RedColor.R, CbColors.RedColor.G, CbColors.RedColor.B, false);
		}

		private static void ResetScaleform()
		{
			if (Scaleform == null) return;
			Scaleform.CallFunction("SET_DISPLAY", -1);
			Scaleform.Dispose();
			Scaleform = null;
		}

		private static async System.Threading.Tasks.Task LoadScaleform()
		{
			ResetScaleform();
			await RAGE.Game.Invoker.WaitAsync(50);

			Scaleform = new RAGE.Game.Scaleform("HACKING_MESSAGE");

			int loadAttempt = 0;
			while (!Scaleform.IsLoaded)
			{
				await RAGE.Game.Invoker.WaitAsync(5);
				if (loadAttempt++ > 50) break;
			}
		}

		private static void DrawMapSprite(int currentMap)
		{
			RAGE.Game.Graphics.DrawSprite(currentMap > 3 ? "MPCircuitHack3" : "MPCircuitHack2", $"cblevel{Level}", 0.5f, 0.5f, 1, 1, 0, 255, 255, 255, 255, 0);
		}

		private static bool DrawPointAndPortSprites()
		{
			CbPoint.DrawPoint();
			bool collisionHit = CbPoint.DrawTailHistoryAndCheckCollisions();
			CbGenericPorts.DrawPorts();
			return collisionHit;
		}

		private static float GetPointSpeedFromDifficulty(CbDifficultyLevelEnum currentDifficulty)
		{
			return currentDifficulty switch
			{
				CbDifficultyLevelEnum.Beginner => 0.00085f,
				CbDifficultyLevelEnum.Easy => 0.001f,
				CbDifficultyLevelEnum.Medium => 0.002f,
				CbDifficultyLevelEnum.Hard => 0.003f,
				CbDifficultyLevelEnum.Expert => 0.01f,
				_ => 0.00085f
			};
		}

		private static float GetDisconnectChanceFromDifficulty(CbDifficultyLevelEnum currentDifficulty)
		{
			return currentDifficulty switch
			{
				CbDifficultyLevelEnum.Beginner => 0f,
				CbDifficultyLevelEnum.Easy => 0.15f,
				CbDifficultyLevelEnum.Medium => 0.30f,
				CbDifficultyLevelEnum.Hard => 0.45f,
				CbDifficultyLevelEnum.Expert => 0.6f,
				_ => 0f
			};
		}

		private static int GetDisconnectCheckRateMsFromDifficulty(CbDifficultyLevelEnum currentDifficulty)
		{
			return currentDifficulty switch
			{
				CbDifficultyLevelEnum.Beginner => 15000,
				CbDifficultyLevelEnum.Easy => 10000,
				CbDifficultyLevelEnum.Medium => 5000,
				CbDifficultyLevelEnum.Hard => 4000,
				CbDifficultyLevelEnum.Expert => 2000,
				_ => 10000
			};
		}

		private static bool ExitButtonPressed()
        {
			if (RAGE.Game.Pad.IsDisabledControlPressed(0, 44)) return true;
			return false;
		}

		private static void DisableControls()
		{
			RAGE.Game.Pad.DisableControlAction(0, 32, true); // W, Up
			RAGE.Game.Pad.DisableControlAction(0, 33, true); // S, Down
			RAGE.Game.Pad.DisableControlAction(0, 34, true); // A, Left
			RAGE.Game.Pad.DisableControlAction(0, 35, true); // D, Right
			RAGE.Game.Pad.DisableControlAction(0, 44, true); // Q, Cover
		}

		private static bool IsPointOutOfBounds(IEnumerable<IEnumerable<Vector2>> polyBounds, IEnumerable<Vector2> mapBounds)
		{
            Vector2 coord = new Vector2(CbPoint.Position);

            IEnumerable<Vector2> headPts = GetPointMaxPoints(coord, CbPoint.PointHeadSize + -0.375f * CbPoint.PointHeadSize);

            List<IEnumerable<Vector2>> polyList = polyBounds.ToList();
            Vector2[] mapList = mapBounds.ToArray();
			foreach (Vector2 pt in headPts)
			{
				foreach (IEnumerable<Vector2> bounds in polyList)
                {
					if (CbHelper.IsInPoly(bounds.ToArray(), pt)) return true;
				}
				if (!CbHelper.IsInPoly(mapList, pt)) return true;
			}
			return false;
		}

		private static IEnumerable<Vector2> GetPointMaxPoints(Vector2 pointCoord, float PointHeadSize)
		{
			float headHeight = PointHeadSize;
			float headWidth = PointHeadSize;

            Vector2 headPt1 = new Vector2(pointCoord.X - headWidth / 2, pointCoord.Y);
			Vector2 headPt2 = new Vector2(pointCoord.X + headWidth / 2, pointCoord.Y);
			Vector2 headPt3 = new Vector2(pointCoord.X, pointCoord.Y - headHeight / 2);
			Vector2 headPt4 = new Vector2(pointCoord.X, pointCoord.Y + headHeight / 2);

			return new[] { headPt1, headPt2, headPt3, headPt4, pointCoord };
		}
	}
}