namespace _Code.Infrastructure.Rooms;

public interface IRoomsViewProvider
{
	ARoomView Kitchen { get; }

	ARoomView Office { get; }

	ARoomView Bedroom { get; }

	ARoomView BigRoom { get; }

	ARoomView Bathroom { get; }

	ARoomView Pantry { get; }

	ARoomView Entrance { get; }
}
