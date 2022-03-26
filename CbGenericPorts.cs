using System;
using System.Collections.Generic;
using System.Linq;

namespace CircuitBreaker
{
    public static class CbGenericPorts 
	{
		/// <summary>
		/// Входной порт (начальный)
		/// </summary>
		public static Vector2 StartPortPos { get; private set; }
		/// <summary>
		/// Выходной порт (конечный)
		/// </summary>
		private static Vector2 FinishPortPos { get; set; }
		/// <summary>
		/// Направление входного порта
		/// </summary>
		public static float StartPortHeading { get; private set; }
		/// <summary>
		/// Направление выходного порта
		/// </summary>
		private static  float FinishPortHeading { get; set; }
		/// <summary>
		/// Лампочки у входного порта
		/// </summary>
		private static CbPortLights StartPortLights { get; set; }
		/// <summary>
		/// Лампочки у выходного порта
		/// </summary>
		private static CbPortLights FinishPortLights { get; set; }
		/// <summary>
		/// Границы входного порта
		/// </summary>
		private static Vector2[] StartPortBounds { get; set; }
		/// <summary>
		/// Границы выходного порта
		/// </summary>
		private static Vector2[] FinishPortBounds { get; set; }
		/// <summary>
		/// Победные границы выходного порта
		/// </summary>
		private static Vector2[] WinBounds { get; set; }

		/// <summary>
		/// Инициализируем все нужные для работы переменные 
		/// </summary>
		/// <param name="level">Номер карты (1-6)</param>
		public static void Initialize(int level)
        {
			StartPortPos = GetStartPortPosition(level);
			FinishPortPos = GetFinishPortPosition(level, StartPortPos);

			StartPortHeading = GetPortHeading(StartPortPos);
			FinishPortHeading = GetPortHeading(FinishPortPos);

			StartPortLights = new CbPortLights(StartPortPos, StartPortHeading, CbPortTypeEnum.Start);
			FinishPortLights = new CbPortLights(FinishPortPos, FinishPortHeading, CbPortTypeEnum.Finish);

			StartPortBounds = GetPortCollisionBounds(StartPortPos, StartPortHeading, true);
			FinishPortBounds = GetPortCollisionBounds(FinishPortPos, FinishPortHeading, false);
			WinBounds = GetWinBounds();
		}

		/// <summary>
		/// Отрисовываем входной и выходной порты
		/// </summary>
		public static void DrawPorts()
        {
			if (StartPortPos == Vector2.Zero || FinishPortPos == Vector2.Zero || StartPortHeading == -1 || FinishPortHeading == -1) return;

			DrawPortSprite(StartPortPos, StartPortHeading);
			DrawPortSprite(FinishPortPos, FinishPortHeading);

			StartPortLights.DrawLights();
			FinishPortLights.DrawLights();
		}

		/// <summary>
		/// Находится ли точка в зоне границы порта
		/// </summary>
		/// <param name="pointPosition">Точка</param>
		/// <returns>
		/// <c>true</c> если соприкасается с границами портов, в противном случае <c>false</c>
		/// </returns>
		public static bool IsCollisionWithPort(Vector2 pointPosition)
		{
			return CbHelper.IsInPoly(StartPortBounds, pointPosition) ||
				   CbHelper.IsInPoly(FinishPortBounds, pointPosition) &&
				   !IsPointInGameWinningPosition(pointPosition);
		}

		/// <summary>
		/// Определяет находится ли курсор в точке победных границ конечного порта
		/// </summary>
		/// <param name="pointPosition">Точка</param>
		/// <returns>
		/// <c>true</c> если соприкасается с победными границами, в противном случае <c>false</c>
		/// </returns>
		public static bool IsPointInGameWinningPosition(Vector2 pointPosition) 
		{
			return CbHelper.IsInPoly(WinBounds, pointPosition);
		}

		/// <summary>
		/// Рисует порты на экране
		/// </summary>
		/// <param name="position">Точка порта</param>
		/// <param name="heading">Направление порта</param>
		private static void DrawPortSprite(Vector2 position, float heading)
		{
			float portHeight = heading == 0 || heading == 180 ? 0.055f : 0.0325f;
			float portWidth = heading == 0 || heading == 180 ? 0.02f : 0.0325f;
			RAGE.Game.Graphics.DrawSprite("MPCircuitHack", "genericport", position.X, position.Y, portWidth, portHeight, heading, 255, 255, 255, 255, 0);
		}

		/// <summary>
		/// Получить величину порта
		/// </summary>
		/// <param name="heading">Направление</param>
		/// <param name="isStartPort">Является ли этот порт стартовым</param>
		/// <returns></returns>
		private static float GetMagnitude(float heading, bool isStartPort)
        {
			if (heading == 0f || heading == 180f)
            {
				if (isStartPort) return 0.0279f;
				return 0.0266f;
			}
			if (isStartPort) return 0.0211f;
			return 0.0173f;

		}

		/// <summary>
		/// Получить углы порта
		/// </summary>
		/// <param name="heading">Направление</param>
		/// <param name="isStartPort">Является ли этот порт стартовым</param>
		/// <returns></returns>
		private static float[] GetAngles(float heading, bool isStartPort)
        {
			if (heading == 0f || heading == 180f)
			{
				if (isStartPort) return new float[4] { 289.75f, 250.75f, 109.75f, 70f };
				return new float[4] { 277.75f, 259.25f, 100.75f, 82.5f };
			}
			if (isStartPort) return new float[4] { 313.25f, 227.75f, 132.25f, 48.5f };
			return new float[4] { 111f, 66.5f, 293.25f, 249.25f };
		}

		/// <summary>
		/// Определяет границы порта
		/// </summary>
		/// <param name="position">Точка порта</param>
		/// <param name="heading">Направление порта</param>
		/// <param name="isStartPort">Является ли этот порт стартовым</param>
		/// <returns></returns>
		private static Vector2[] GetPortCollisionBounds(Vector2 position, float heading, bool isStartPort) 
		{
			float magnitude = GetMagnitude(heading, isStartPort);
			int mult = heading == 0f || heading == 180f ? 1 : -1;
			float[] angles = GetAngles(heading, isStartPort);

            Vector2[] portBounds = new Vector2[4];

			int i = 0;
			foreach(float angle in angles) 
			{
				portBounds[i] = CbHelper.GetOffsetPosition(position, magnitude, (heading + angle) % 360, mult);
				i++;
			}

			return portBounds;
		}

		private static Tuple<float, float>[] GetMagnitudeAngleOffsetPairs(float heading)
        {
			return heading == 0f || heading == 180f ? 
				new Tuple<float, float>[]
				{
					new Tuple<float, float>( 0.0278f, 70.25f ), new Tuple<float, float>( 0.02807f, 289.5f ),
					new Tuple<float, float>( 0.02708f, 282f ), new Tuple<float, float>( 0.02665f, 77.75f )
				} 
				: 
				new Tuple<float, float>[]
				{
					new Tuple<float, float>( 0.02088f, 228.5f ), new Tuple<float, float>( 0.01827f, 238.75f ),
					new Tuple<float, float>( 0.01806f, 121.75f ), new Tuple<float, float>( 0.02061f, 131.75f )
				};
		}

		/// <summary>
		/// Получаем победные границы
		/// </summary>
		/// <returns></returns>
		private static Vector2[] GetWinBounds() 
		{
			int mult = FinishPortHeading == 0f || FinishPortHeading == 180 ? 1 : -1;
			Tuple<float, float>[] magnitudeAngleOffsetPairs = GetMagnitudeAngleOffsetPairs(FinishPortHeading);

            Vector2[] portBounds = new Vector2[4];
			int i = 0;
			foreach(Tuple<float, float> pair in magnitudeAngleOffsetPairs ) 
			{
				portBounds[i] = CbHelper.GetOffsetPosition( FinishPortPos, pair.Item1, (FinishPortHeading + pair.Item2) % 360, mult );
				i++;
			}

			return portBounds;
		}

		/// <summary>
		/// Получаем точку начального порта
		/// </summary>
		/// <param name="levelNumber">Уровень</param>
		/// <returns></returns>
		private static Vector2 GetStartPortPosition(int levelNumber) 
		{
            List<List<Vector2>> potentialPortBounds = GetPortPositionBounds(levelNumber);
			if (!potentialPortBounds.Any()) return Vector2.Zero;

            List<Vector2> startPortBounds = potentialPortBounds[Helpers.GetRange( 0, potentialPortBounds.Count )];
            Vector2 startPos = Vector2.Zero;
			int attempts = 20;
			while (startPos == Vector2.Zero && attempts > 0) 
			{
				startPos = GetRandomPortPosition( startPortBounds );
				attempts--;
			}

			return startPos;
		}

		/// <summary>
		/// Получаем точку конечного порта
		/// </summary>
		/// <param name="levelNumber">Уровень</param>
		/// <param name="startPortPosition">Точка начального порта</param>
		/// <returns></returns>
		private static Vector2 GetFinishPortPosition(int levelNumber, Vector2 startPortPosition) 
		{
            List<List<Vector2>> potentialPortBounds = GetPortPositionBounds( levelNumber );

			float maxDist = 0f;
            Vector2 endPos = Vector2.Zero;
			foreach( var bounds in potentialPortBounds ) 
			{
                Vector2 potentialPos = Vector2.Zero;
				while( potentialPos == Vector2.Zero) potentialPos = GetRandomPortPosition( bounds );

				float startEndDistance = Vector2.Distance( startPortPosition, potentialPos );
				if( startEndDistance > maxDist ) 
				{
					maxDist = startEndDistance;
					endPos = potentialPos;
				}
			}

			return endPos;
		}

		/// <summary>
		/// Получаем направление порта
		/// </summary>
		/// <param name="portPosition">Точка порта</param>
		/// <returns></returns>
		private static float GetPortHeading( Vector2 portPosition ) 
		{
			float minX = 0.159f;
			float maxX = 0.841f;

			float minY = 0.153f;
			float maxY = 0.848f;

            List<float> xBounds = new List<float> {minX, maxX};
            List<float> yBounds = new List<float> {minY, maxY};

			float closestX = xBounds.OrderBy( x => Math.Abs( portPosition.X - x ) ).FirstOrDefault();
			float closestY = yBounds.OrderBy( y => Math.Abs( portPosition.Y - y ) ).FirstOrDefault();

			if(Math.Abs( portPosition.X - closestX ) < Math.Abs( portPosition.Y - closestY )) 
			{
				if(Math.Abs( closestX - minX ) < Math.Abs( closestX - maxX )) return 0f;
				return 180f;
			}

			if( Math.Abs( closestY - minY ) < Math.Abs( closestY - maxY ) ) return 90f;
			return 270f;
		}

		/// <summary>
		/// Получаем случайную точку порта
		/// </summary>
		/// <param name="portBounds">Границы порта</param>
		/// <returns></returns>
		private static Vector2 GetRandomPortPosition(List<Vector2> portBounds) 
		{
			if (portBounds == null || portBounds.Count < 2) return Vector2.Zero;

			float portX = Helpers.GetRange( (int)(portBounds[0].X * 1000), (int)(portBounds[1].X * 1000) ) / 1000f;
			float portY = Helpers.GetRange( (int)(portBounds[0].Y * 1000), (int)(portBounds[1].Y * 1000) ) / 1000f;

			return new Vector2(portX, portY);
		}

		/// <summary>
		/// Получаем возможные границы портов
		/// </summary>
		/// <param name="mapNumber">Уровень</param>
		/// <returns></returns>
		private static List<List<Vector2>> GetPortPositionBounds(int mapNumber) 
		{
            return mapNumber switch
            {
				1 => new List<List<Vector2>> {
					new List<Vector2> {
						new Vector2( 0.169f, 0.613f ),
						new Vector2( 0.169f, 0.816f )
					},
					new List<Vector2> {
						new Vector2( 0.179f, 0.837f ),
						new Vector2( 0.284f, 0.837f )
					},
					new List<Vector2> {
						new Vector2( 0.833f, 0.181f ),
						new Vector2( 0.833f, 0.277f )
					},
					new List<Vector2> {
						new Vector2( 0.751f, 0.163f ),
						new Vector2( 0.823f, 0.163f )
					}
				},
				2 => new List<List<Vector2>> {
					new List<Vector2> {
						new Vector2( 0.169f, 0.673f ),
						new Vector2( 0.169f, 0.818f )
					},
					new List<Vector2> {
						new Vector2( 0.18f, 0.838f ),
						new Vector2( 0.297f, 0.838f )
					},
					new List<Vector2> {
						new Vector2( 0.832f, 0.181f ),
						new Vector2( 0.832f, 0.324f )
					},
					new List<Vector2> {
						new Vector2( 0.778f, 0.16f ),
						new Vector2( 0.821f, 0.16f )
					}
				},
				3 => new List<List<Vector2>> {
					new List<Vector2> {
						new Vector2( 0.166f, 0.182f ),
						new Vector2( 0.166f, 0.263f )
					},
					new List<Vector2> {
						new Vector2( 0.166f, 0.745f ),
						new Vector2( 0.166f, 0.816f )
					},
					new List<Vector2> {
						new Vector2( 0.18f, 0.837f ),
						new Vector2( 0.31f, 0.837f )
					},
					new List<Vector2> {
						new Vector2( 0.184f, 0.164f ),
						new Vector2( 0.277f, 0.164f )
					}
				},
				4 => new List<List<Vector2>> {
					new List<Vector2> {
						new Vector2( 0.169f, 0.628f ),
						new Vector2( 0.169f, 0.817f )
					},
					new List<Vector2> {
						new Vector2( 0.183f, 0.838f ),
						new Vector2( 0.259f, 0.838f )
					},
					new List<Vector2> {
						new Vector2( 0.833f, 0.186f ),
						new Vector2( 0.833f, 0.359f )
					},
					new List<Vector2> {
						new Vector2( 0.797f, 0.161f ),
						new Vector2( 0.819f, 0.161f )
					}
				},
				5 => new List<List<Vector2>> {
					new List<Vector2> {
						new Vector2( 0.832f, 0.742f ),
						new Vector2( 0.832f, 0.811f )
					},
					new List<Vector2> {
						new Vector2( 0.761f, 0.839f ),
						new Vector2( 0.821f, 0.839f )
					},
					new List<Vector2> {
						new Vector2( 0.169f, 0.184f ),
						new Vector2( 0.169f, 0.383f )
					},
					new List<Vector2> {
						new Vector2( 0.184f, 0.162f ),
						new Vector2( 0.234f, 0.162f )
					}
				},
				6 => new List<List<Vector2>> {
					new List<Vector2> {
						new Vector2( 0.167f, 0.183f ),
						new Vector2( 0.167f, 0.3f )
					},
					new List<Vector2> {
						new Vector2( 0.18f, 0.162f ),
						new Vector2( 0.214f, 0.162f ),
					},
					new List<Vector2> {
						new Vector2( 0.833f, 0.186f ),
						new Vector2( 0.833f, 0.282f )
					},
					new List<Vector2> {
						new Vector2( 0.768f, 0.161f ),
						new Vector2( 0.82f, 0.161f )
					}
				},
				_ => new List<List<Vector2>>()
			};
		}
	}
}