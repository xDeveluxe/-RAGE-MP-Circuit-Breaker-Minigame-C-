using System;
using System.Linq;

namespace CircuitBreaker
{
	public static class CbHelper
	{
		/// <summary>
		/// Определяет находится ли точка внутри полигонов
		/// </summary>
		/// <param name="poly">Полигоны</param>
		/// <param name="point">Точка</param>
		/// <returns>
		///   <c>true</c> если точка внутри полигонов, в противном случае <c>false</c>.
		/// </returns>
		public static bool IsInPoly(Vector2[] poly, Vector2 point)
		{
			double MinX = poly.Min(a => a.X);
			double MinY = poly.Min(a => a.Y);
			double MaxX = poly.Max(a => a.X);
			double MaxY = poly.Max(a => a.Y);

			if (point.X < MinX || point.X > MaxX || point.Y < MinY || point.Y > MaxY) return false;

			int I = 0;
			int J = poly.Count() - 1;
			bool IsMatch = false;

			for (; I < poly.Count(); J = I++)
			{
				if (poly[I].X == point.X && poly[I].Y == point.Y) return true;
				if (poly[J].X == point.X && poly[J].Y == point.Y) return true;

				if (poly[I].X == poly[J].X && point.X == poly[I].X && point.Y >= Math.Min(poly[I].Y, poly[J].Y) && point.Y <= Math.Max(poly[I].Y, poly[J].Y)) return true;
				if (poly[I].Y == poly[J].Y && point.Y == poly[I].Y && point.X >= Math.Min(poly[I].X, poly[J].X) && point.X <= Math.Max(poly[I].X, poly[J].X)) return true;

				if (poly[I].Y > point.Y != poly[J].Y > point.Y && point.X < (poly[J].X - poly[I].X) * (point.Y - poly[I].Y) / (poly[J].Y - poly[I].Y) + poly[I].X) IsMatch = !IsMatch;
			}

			return IsMatch;
		}
		public static Vector2 GetOffsetPosition(Vector2 startPosition, float magnitude, float heading, int multiplier)
		{
			double cosx = multiplier * Math.Cos(heading * (Math.PI / 180f));
			double siny = multiplier * Math.Sin(heading * (Math.PI / 180f));

			float x = startPosition.X;
			float y = startPosition.Y;

			float newX = (float)(x + magnitude * cosx);
			float newY = (float)(y + magnitude * siny);

			return new Vector2(newX, newY);
		}
	}
}