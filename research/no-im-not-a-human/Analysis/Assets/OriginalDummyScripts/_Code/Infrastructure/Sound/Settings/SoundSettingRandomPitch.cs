namespace _Code.Infrastructure.Sound.Settings
{
	public sealed class SoundSettingRandomPitch : ASoundSetting
	{
		public float MinPitch { get; }

		public float MaxPitch { get; }

		public SoundSettingRandomPitch(float minPitch, float maxPitch)
		{
		}

		public SoundSettingRandomPitch(float radius)
		{
		}
	}
}
