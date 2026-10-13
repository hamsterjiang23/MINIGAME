namespace _Code.Infrastructure.DataModel.Models.GameSave
{
	public abstract class ASavableClass<T> where T : ASavableData
	{
		private IGameSaveDataHandler _dataSaver;

		protected abstract void OnSaveDataLoad(IGameSaveDataHandler saver);

		protected void LinkSaveDataKey(ASavableClass<T> savable, T data, IGameSaveDataHandler saver)
		{
		}

		public virtual void Dispose()
		{
		}
	}
}
