using Godot;
using System;

public class SampleBuild : Node
{
	// Make sure we can compile
	public override void _Ready()
	{
		GD.Print("Hello World");
		Console.WriteLine("Hello World");
	}
}
