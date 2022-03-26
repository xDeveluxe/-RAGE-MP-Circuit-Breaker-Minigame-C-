using System;

namespace CircuitBreaker
{
	public class CbPortLights
	{
		/// <summary>
		/// Инициализируем новый порт и его подсветку
		/// </summary>
		/// <param name="portPos">Точка порта</param>
		/// <param name="portHeading">Направление порта</param>
		/// <param name="type">Тип порта (начальный или конечный)</param>
		public CbPortLights( Vector2 portPos, float portHeading, CbPortTypeEnum type ) 
		{
			Light0Position = GetLightPosition(portPos, portHeading, 0);
			Light1Position = GetLightPosition(portPos, portHeading, 1);
			PortType = type;
			Alpha = 255;
		}

		/// <summary>
		/// Точка первой лампочки
		/// </summary>
		private Vector2 Light0Position { get; }
		/// <summary>
		/// Точка второй лампочки
		/// </summary>
		private Vector2 Light1Position { get; }
		/// <summary>
		/// Тип порта (начальный или конечный)
		/// </summary>
		private CbPortTypeEnum PortType { get; }
		/// <summary>
		/// Время последней вспышки лампочек
		/// </summary>
		private DateTime LastBlink { get; set; }
		/// <summary>
		/// Прозрачность
		/// </summary>
		private int Alpha { get; set; }

		/// <summary>
		/// Отрисовать лампочки на порте
		/// </summary>
		public void DrawLights() 
		{
			if (PortType == CbPortTypeEnum.Start) 
			{
				DrawLightSprite( Light0Position, CbColors.GreenColor.R, CbColors.GreenColor.G, CbColors.GreenColor.B);
				DrawLightSprite( Light1Position, CbColors.GreenColor.R, CbColors.GreenColor.G, CbColors.GreenColor.B);
				return;
			}
			if (DateTime.Now.CompareTo(LastBlink.AddMilliseconds(500)) >= 0)
			{
				Alpha = Alpha == 255 ? 0 : 255;
				LastBlink = DateTime.Now;
			}

			DrawLightSprite(Light0Position, CbColors.RedColor.R, CbColors.RedColor.G, CbColors.RedColor.B, Alpha);
			DrawLightSprite(Light1Position, CbColors.RedColor.R, CbColors.RedColor.G, CbColors.RedColor.B, Alpha);
		}

		/// <summary>
		/// Отрисовать лампочки на порте
		/// </summary>
		/// <param name="position">Точка лампочки</param>
		/// <param name="red">Красный цвет</param>
		/// <param name="green">Зелёный цвет</param>
		/// <param name="blue">Синий цвет</param>
		/// <param name="alpha">Прозрачность</param>
		private void DrawLightSprite( Vector2 position, int red, int green, int blue, int alpha = 255 ) 
		{
			RAGE.Game.Graphics.DrawSprite( "MPCircuitHack", "light", position.X, position.Y, 0.00775f, 0.00775f, 0, red, green, blue, alpha, 0 );
		}

		private float GetAngleOffset(float portHeading, int lightNum)
        {
			if (portHeading == 90f || portHeading == 270f)
            {
				if (lightNum > 0) return 128.75f;
				return 232f;
			}
			if (lightNum > 0) return 73f;
			return 287.25f;
		}

		private Vector2 GetLightPosition( Vector2 portPos, float portHeading, int lightNum) 
		{
			float magnitude = portHeading == 90f || portHeading == 270f ? 0.0164f : 0.0228f;
			float angleOffset = GetAngleOffset(portHeading, lightNum);
			int multiplier = portHeading == 90f || portHeading == 270f ? -1 : 1;

			return CbHelper.GetOffsetPosition( portPos, magnitude, (angleOffset + portHeading) % 360, multiplier );
		}
	}
}