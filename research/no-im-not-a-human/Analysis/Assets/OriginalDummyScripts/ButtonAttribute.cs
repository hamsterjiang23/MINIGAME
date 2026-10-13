using System;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : Attribute
{
	public string Name;

	public ButtonAttribute(string name = "")
	{
	}
}
