namespace Code.Utils
{
	public sealed class AppendPrefixEnumCreatorParameter : IEnumCreatorParameter
	{
		public string Prefix { get; }

		public AppendPrefixEnumCreatorParameter(string prefix)
		{
		}
	}
}
