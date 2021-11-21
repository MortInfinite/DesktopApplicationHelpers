using System;
using Bindings;

namespace BindingTestConsoleApplication
{
	class Program
	{
		static void Main(string[] args)
		{
			FirstClass	firstClass		= new FirstClass();
			firstClass.FirstProperty	= "First property";

			ThirdClass	thirdClass		= new ThirdClass();
			thirdClass.ThirdProperty	= "Third property";

			ThirdClass	anotherThirdClass		= new ThirdClass();
			anotherThirdClass.ThirdProperty		= "Another third property";

			SecondClass secondClass		= new SecondClass();
			secondClass.SecondProperty	= "Second property";
			secondClass.Other			= thirdClass;

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "FirstProperty", secondClass, "Other.ThirdProperty", Bindings.BindingModes.TwoWay);
			Console.WriteLine($"thirdClass.ThirdProperty = {thirdClass.ThirdProperty}");
			Console.WriteLine();

			secondClass.Other = anotherThirdClass;
			//Console.WriteLine($"thirdClass.ThirdProperty = {thirdClass.ThirdProperty}");
			Console.WriteLine($"firstClass.FirstProperty = {firstClass.FirstProperty}");
		}
	}
}
