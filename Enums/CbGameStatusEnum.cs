namespace CircuitBreaker
{
	public enum CbGameStatusEnum
	{
		/// <summary>
		/// Пусто
		/// </summary>
		None,
		/// <summary>
		/// Ошибка
		/// </summary>
		Error,
		/// <summary>
		/// Столкновение V1
		/// </summary>
		FailureOutOfBounds,
		/// <summary>
		/// Столкновение V2
		/// </summary>
		FailureCollisionWithPort,
		/// <summary>
		/// Столкновение V3
		/// </summary>
		FailureTrailCollision,
		/// <summary>
		/// Выход
		/// </summary>
		Quit,
		/// <summary>
		/// Начало
		/// </summary>
		Starting,
		/// <summary>
		/// В игре
		/// </summary>
		InProcess,
		/// <summary>
		/// Смерть
		/// </summary>
		Death,
		/// <summary>
		/// Переподключение
		/// </summary>
		Disconnected,
		/// <summary>
		/// Успех
		/// </summary>
		Success
	}
}