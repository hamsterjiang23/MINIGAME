namespace _Code.Infrastructure.Sound.Settings
{
	public sealed class SoundSettingRandomPitch : ASoundSetting
	{
		public float MinPitch { get; }

		public float MaxPitch { get; }

		public SoundSettingRandomPitch(float minPitch, float maxPitch)
		{
			MinPitch = minPitch;
			MaxPitch = maxPitch;
		}

		public SoundSettingRandomPitch(float radius)
		{
			MinPitch = 1f - radius;
			MaxPitch = 1f + radius;
		}
	}
}
