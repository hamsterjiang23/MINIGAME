using _Scripts.Services.Sound.Instance;

namespace _Scripts.Services.Sound.Service
{
	public interface ISoundServiceInstanceProvider
	{
		SoundServiceInstance SoundServiceInstance { get; }
	}
}
