namespace _Code.Utils.Extensions
{
	public static class BooleanExtension
	{
		public static void Switch(this ref bool value)
		{
			value = !value;
		}
	}
}
