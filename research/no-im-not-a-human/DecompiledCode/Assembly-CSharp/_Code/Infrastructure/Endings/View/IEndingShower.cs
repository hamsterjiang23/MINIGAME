using Cysharp.Threading.Tasks;

namespace _Code.Infrastructure.Endings.View;

public interface IEndingShower
{
	UniTaskVoid ShowEnding(EEnding endingType);
}
