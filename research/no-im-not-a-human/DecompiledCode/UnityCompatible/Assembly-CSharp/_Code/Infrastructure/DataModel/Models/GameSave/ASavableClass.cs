namespace _Code.Infrastructure.DataModel.Models.GameSave
{
	public abstract class ASavableClass<T> where T : ASavableData
	{
		private IGameSaveDataHandler _dataSaver;

		protected abstract void OnSaveDataLoad(IGameSaveDataHandler saver);

		protected void LinkSaveDataKey(ASavableClass<T> savable, T data, IGameSaveDataHandler saver)
		{
			_dataSaver = saver;
			_dataSaver.ConnectKey(savable, data);
			_dataSaver.LoadedAll -= OnSaveDataLoad;
			_dataSaver.LoadedAll += OnSaveDataLoad;
		}

		public virtual void Dispose()
		{
			_dataSaver.LoadedAll -= OnSaveDataLoad;
		}
	}
}
