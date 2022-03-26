using System;
using System.Collections.Generic;
using System.Linq;

namespace CircuitBreaker
{
	public static class CbPoint
	{
		public const float PointHeadSize = 0.0125f;

		private static readonly List<Vector2> History = new List<Vector2>();
		private static int Alpha { get; set; } = 255;
		public static bool IsAlive { get; private set; } = true;
		public static bool IsVisible { get; private set; } = true;
		public static CbDirectionsEnum LastDirection { get; private set; } = CbDirectionsEnum.Debug;
		public static Vector2 Position { get; private set; } = Vector2.Zero;

		/// <summary>
		/// Сбрасываем данные луча до стандартных
		/// </summary>
		private static void ResetData()
        {
			Alpha = 255;
			IsAlive = true;
			IsVisible = true;
		}

		/// <summary>
		/// Инициализируем переменные для игры
		/// </summary>
		public static void Initialize() 
		{
			ResetData();

			History.Clear();
			SetPointStartPosition();
			History.Add(CbGenericPorts.StartPortPos);
			SetStartDirection(CbGenericPorts.StartPortHeading);
		}

		/// <summary>
		/// Отрисовываем точку (голову луча)
		/// </summary>
		public static void DrawPoint() 
		{
			if (!IsAlive) RAGE.Game.Graphics.DrawSprite("MPCircuitHack", "spark", Position.X, Position.Y, PointHeadSize, PointHeadSize, 0, 255, 255, 255, Alpha, 0);

			switch (CircuitBreaker.Status)
			{
				case CbGameStatusEnum.Starting:
				case CbGameStatusEnum.InProcess:
				case CbGameStatusEnum.Success:
					RAGE.Game.Graphics.DrawSprite("MPCircuitHack", "head", Position.X, Position.Y, PointHeadSize, PointHeadSize, 0, CbColors.GreenColor.R, CbColors.GreenColor.G, CbColors.GreenColor.B, Alpha, 0);
					return;

				default:
					RAGE.Game.Graphics.DrawSprite("MPCircuitHack", "head", Position.X, Position.Y, PointHeadSize, PointHeadSize, 0, CbColors.RedColor.R, CbColors.RedColor.G, CbColors.RedColor.B, Alpha, 0);
					return;

			}
		}

		private static Vector2 GetCenterPoint(Vector2 pos, bool xDeltaOveryDelta, float xDelta, float yDelta, float distance)
        {
			if (xDeltaOveryDelta) return xDelta < 0 ? new Vector2(pos.X + distance / 2, pos.Y) : new Vector2(pos.X - distance / 2, pos.Y);
			return yDelta < 0 ? new Vector2(pos.X, pos.Y + distance / 2) : new Vector2(pos.X, pos.Y - distance / 2);
		}

		/// <summary>
		/// Отрисовываем тело луча по ширине
		/// </summary>
		private static void DrawTailSpriteWidth(Vector2 center, float distance)
        {
			switch (CircuitBreaker.Status)
			{
				case CbGameStatusEnum.Starting:
				case CbGameStatusEnum.InProcess:
				case CbGameStatusEnum.Success:
					RAGE.Game.Graphics.DrawSprite("MPCircuitHack", "tail", center.X, center.Y, distance + 0.0018f, 0.003f, 0, CbColors.GreenColor.R, CbColors.GreenColor.G, CbColors.GreenColor.B, Alpha, 0);
					return;

				default:
					RAGE.Game.Graphics.DrawSprite("MPCircuitHack", "tail", center.X, center.Y, distance + 0.0018f, 0.003f, 0, CbColors.RedColor.R, CbColors.RedColor.G, CbColors.RedColor.B, Alpha, 0);
					return;

			}
		}

		/// <summary>
		/// Отрисовываем тело луча по высоте
		/// </summary>
		private static void DrawTailSpriteHeight(Vector2 center, float distance)
        {
			switch(CircuitBreaker.Status)
            {
				case CbGameStatusEnum.Starting:
				case CbGameStatusEnum.InProcess:
				case CbGameStatusEnum.Success:
					RAGE.Game.Graphics.DrawSprite("MPCircuitHack", "tail", center.X, center.Y, 0.0018f, distance + 0.003f, 0, CbColors.GreenColor.R, CbColors.GreenColor.G, CbColors.GreenColor.B, Alpha, 0);
					return;

				default:
					RAGE.Game.Graphics.DrawSprite("MPCircuitHack", "tail", center.X, center.Y, 0.0018f, distance + 0.003f, 0, CbColors.RedColor.R, CbColors.RedColor.G, CbColors.RedColor.B, Alpha, 0);
					return;

            }
		}

		/// <summary>
		/// Проверяем на то пересекается ли точка (голова нашего луча) с телом
		/// </summary>
		/// <returns><c>true</c> если пересекается, в противном случае <c>false</c></returns>
		private static bool CheckForCollision(bool xDeltaOveryDelta, Vector2 center, float distance)
        {
			float distance2 = distance / 2f;
			if (xDeltaOveryDelta)
			{
				double roundedX = Math.Round(Position.X, 3);
				if (roundedX <= Math.Round(center.X - distance2, 3)) return false;
				if (roundedX >= Math.Round(center.X + distance2, 3)) return false;
				if (Math.Abs(Position.Y - center.Y) > 0.003f) return false;
				return true;
			}
			double roundedY = Math.Round(Position.Y, 3);
			if (roundedY <= Math.Round(center.Y - distance2, 3)) return false;
			if (roundedY >= Math.Round(center.Y + distance2, 3)) return false;
			if (Math.Abs(Position.X - center.X) > 0.003f) return false;
			return true;
		}

		/// <summary>
		/// Отрисовываем луч и сверяемся на столкновение точки с лучом
		/// </summary>
		/// <returns><c>true</c> если точка луча пересекается с самим лучом, в противном случае <c>false</c></returns>
		public static bool DrawTailHistoryAndCheckCollisions()
        {
			if (History.Count == 0) return false;

			float distance;
			float xDelta;
			float yDelta;
            Vector2 centerPoint;
            Vector2 historyPoint;
			Vector2 historyNextPoint;
			bool xDeltaOveryDelta;

            List<Vector2> HistoryCopy = History.ToList();

			for (int i = 0; i < HistoryCopy.Count; i++)
			{
				historyPoint = new Vector2(HistoryCopy[i]);
				if (i + 1 == HistoryCopy.Count) historyNextPoint = new Vector2(Position);
				else historyNextPoint = new Vector2(HistoryCopy[i + 1]);
				distance = Vector2.Distance(historyNextPoint, historyPoint);
				xDelta = historyNextPoint.X - historyPoint.X;
				yDelta = historyNextPoint.Y - historyPoint.Y;
				xDeltaOveryDelta = Math.Abs(xDelta) > Math.Abs(yDelta);
				centerPoint = GetCenterPoint(historyNextPoint, xDeltaOveryDelta, xDelta, yDelta, distance);
				if (CheckForCollision(xDeltaOveryDelta, centerPoint, distance)) return true;
				DrawTail(centerPoint, xDeltaOveryDelta, distance);
			}

			return false;
        }

		/// <summary>
		/// Отрисовываем луч
		/// </summary>
		private static void DrawTail(Vector2 center, bool xDeltaOveryDelta, float distance)
        {
			if (xDeltaOveryDelta)
            {
				DrawTailSpriteWidth(center, distance);
				return;
            }
			DrawTailSpriteHeight(center, distance);
        }

		/// <summary>
		/// Смещение точки (головы луча)
		/// </summary>
		/// <param name="pointSpeed">Скорость луча</param>
		public static void MovePoint(float pointSpeed) 
		{
			SetPosition(LastDirection, pointSpeed);
		}

		/// <summary>
		/// Добавление точки в историю луча
		/// </summary>
		public static void AddToTailHistory(Vector2 directionChangePoint) 
		{
			if (!History.Contains(directionChangePoint)) History.Add(directionChangePoint);
		}

		/// <summary>
		/// Устанавливаем стартовое направление луча
		/// </summary>
		/// <param name="startHeading">Направление</param>
		public static void SetStartDirection(float startHeading) 
		{
			LastDirection = startHeading switch
            {
				0f => CbDirectionsEnum.Right,
				90f => CbDirectionsEnum.Down,
				180f => CbDirectionsEnum.Left,
				_ => CbDirectionsEnum.Up
            };
		}

		/// <summary>
		/// Получаем новое направление движения точки из нажатих клавиш
		/// </summary>
		/// <param name="current">Текущее направление</param>
		/// <returns>Получим новое или текущее (если не было изменено) направление движения</returns>
		private static CbDirectionsEnum GetDirectionFromInput(CbDirectionsEnum current)
        {
			if (RAGE.Game.Pad.IsDisabledControlPressed(0, 34)) return CbDirectionsEnum.Left;
			if (RAGE.Game.Pad.IsDisabledControlPressed(0, 35)) return CbDirectionsEnum.Right;
			if (RAGE.Game.Pad.IsDisabledControlPressed(0, 32)) return CbDirectionsEnum.Up;
			if (RAGE.Game.Pad.IsDisabledControlPressed(0, 33)) return CbDirectionsEnum.Down;
			return current;
		}

		/// <summary>
		/// Проверяем является ли новое выбранное движение ровно противоположным уже имеющемуся
		/// </summary>
		/// <param name="current">Текущее направление</param>
		/// <param name="newdir">Новое потенциальное направление</param>
		/// <returns><c>true</c> если новое движение ровно противоположно текущему, в противном случае <c>false</c></returns>
		private static bool IsOppositeOfCurrentDirection(CbDirectionsEnum current, CbDirectionsEnum newdir)
        {
			return current == CbDirectionsEnum.Up && newdir == CbDirectionsEnum.Down || current == CbDirectionsEnum.Down && newdir == CbDirectionsEnum.Up || current == CbDirectionsEnum.Left && newdir == CbDirectionsEnum.Right || current == CbDirectionsEnum.Right && newdir == CbDirectionsEnum.Left;
        }

		/// <summary>
		/// Получаем направление движения точки исходя из нажатия кнопок игроком
		/// </summary>
		public static void GetPointInputFromPlayer() 
		{
            CbDirectionsEnum newDirection = GetDirectionFromInput(LastDirection);
			Vector2 lastPos = new Vector2(Position);

			if (newDirection == LastDirection || IsOppositeOfCurrentDirection(LastDirection, newDirection)) return;

			LastDirection = newDirection;
			AddToTailHistory(lastPos);
			RAGE.Game.Audio.PlaySoundFrontend(-1, "Click", "DLC_HEIST_HACKING_SNAKE_SOUNDS", true);
		}

		/// <summary>
		/// Начинаем анимацию смерти луча
		/// </summary>
		public static async void StartPointDeathAnimation() 
		{
			if (!IsAlive) return;
			IsAlive = false;
			while( Alpha > 0 ) 
			{
				UpdateAlpha();
				await RAGE.Game.Invoker.WaitAsync( 0 );
			}
		}

		/// <summary>
		/// Обновляем прозрачность луча
		/// </summary>
		private static void UpdateAlpha() 
		{
			if (IsAlive) return;

			Alpha = Math.Clamp(Alpha - 5, 0, 255);
			if (Alpha <= 0) IsVisible = false;
		}

		/// <summary>
		/// Устанавливаем точку на стартовую позицию
		/// </summary>
		private static void SetPointStartPosition() 
		{
			float magnitude = CbGenericPorts.StartPortHeading == 0f || CbGenericPorts.StartPortHeading == 180f ? 0.0144f : 0.0210f;

			Position = CbHelper.GetOffsetPosition(CbGenericPorts.StartPortPos, magnitude, CbGenericPorts.StartPortHeading, 1);
		}

		/// <summary>
		/// Смещаем точку по направлению
		/// </summary>
		/// <param name="direction">Нужное направление смещения</param>
		/// <param name="pointSpeed">Скорость смещения</param>
		private static void SetPosition(CbDirectionsEnum direction, float pointSpeed) 
		{
			switch(direction) 
			{
				case CbDirectionsEnum.Up:
					Position.Y -= pointSpeed;
					break;
				case CbDirectionsEnum.Down:
					Position.Y += pointSpeed;
					break;
				case CbDirectionsEnum.Left:
					Position.X -= pointSpeed;
					break;
				case CbDirectionsEnum.Right:
					Position.X += pointSpeed;
					break;
			}

			Position.X = Math.Clamp( Position.X, 0f, 1f );
			Position.Y = Math.Clamp( Position.Y, 0f, 1f );
		}
	}
}