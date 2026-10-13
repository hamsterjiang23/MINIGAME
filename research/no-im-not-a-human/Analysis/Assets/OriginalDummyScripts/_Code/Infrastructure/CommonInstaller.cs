using UnityEngine;
using Zenject;

namespace _Code.Infrastructure
{
	public sealed class CommonInstaller : MonoInstaller
	{
		[SerializeField]
		private ResourceMother _resourceMother;

		public override void InstallBindings()
		{
		}
	}
}
