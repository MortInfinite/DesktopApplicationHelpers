using Bindings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestData;

namespace BindingUnitTest
{
	[TestClass]
	public class HierarchicalBindingUnitTest
	{
		/// <summary>
		/// Tests binding one object's property to one other object's property, when the binding is first created.
		/// 
		/// FirstClass.FirstProperty -> SecondClass.SecondProperty
		/// </summary>
		[TestMethod]
		public void OneToOne_OneWayBinding_InitialValue()
		{
			FirstClass	firstClass		= new FirstClass();
			firstClass.FirstProperty	= "First property";

			SecondClass secondClass		= new SecondClass();
			secondClass.SecondProperty	= "Second property";

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "FirstProperty", secondClass, "SecondProperty", BindingModes.OneWay);

			Assert.AreEqual(firstClass.FirstProperty, secondClass.SecondProperty);
		}

		/// <summary>
		/// Tests binding one object's property to one other object's property, after modifying the source property.
		/// 
		/// FirstClass.FirstProperty -> SecondClass.SecondProperty
		/// </summary>
		[TestMethod]
		public void OneToOne_OneWayBinding_AfterModifyingSourceProperty()
		{
			FirstClass	firstClass		= new FirstClass();
			firstClass.FirstProperty	= "First property";

			SecondClass secondClass		= new SecondClass();
			secondClass.SecondProperty	= "Second property";

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "FirstProperty", secondClass, "SecondProperty", BindingModes.OneWay);

			firstClass.FirstProperty	= "New first property";

			Assert.AreEqual(firstClass.FirstProperty, secondClass.SecondProperty);
		}

		/// <summary>
		/// Tests binding two objects' properties to one other object's property, when the binding is first created.
		/// 
		/// FirstClass.Other.SecondProperty -> ThirdClass.ThirdProperty
		/// </summary>
		[TestMethod]
		public void TwoToOne_OneWayBinding_InitialValue()
		{
			SecondClass secondClass		= new SecondClass();
			secondClass.SecondProperty	= "Second property";

			FirstClass firstClass		= new FirstClass();
			firstClass.Second			= secondClass;

			ThirdClass thirdClass		= new ThirdClass();
			thirdClass.ThirdProperty	= "Third property";

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "Second.SecondProperty", thirdClass, "ThirdProperty", BindingModes.OneWay);

			Assert.AreEqual(secondClass.SecondProperty, thirdClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding two objects' properties to one other object's property, after modifying a source property.
		/// 
		/// FirstClass.Other.SecondProperty -> ThirdClass.ThirdProperty
		/// </summary>
		[TestMethod]
		public void TwoToOne_OneWayBinding_AfterModifyingSourceProperty()
		{
			SecondClass secondClass		= new SecondClass();
			secondClass.SecondProperty	= "Second property";

			FirstClass firstClass		= new FirstClass();
			firstClass.Second			= secondClass;

			ThirdClass thirdClass		= new ThirdClass();
			thirdClass.ThirdProperty	= "Third property";

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "Second.SecondProperty", thirdClass, "ThirdProperty", BindingModes.OneWay);
			secondClass.SecondProperty	= "New second property";

			Assert.AreEqual(secondClass.SecondProperty, thirdClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding two objects' properties to one other object's property, after modifying a source property.
		/// 
		/// FirstClass.Other.SecondProperty -> ThirdClass.ThirdProperty
		/// </summary>
		[TestMethod]
		public void TwoToOne_OneWayBinding_AfterModifyingSourcePropertyPart()
		{
			SecondClass secondClass		= new SecondClass();
			secondClass.SecondProperty	= "Second property";

			FirstClass firstClass		= new FirstClass();
			firstClass.Second			= secondClass;

			ThirdClass thirdClass		= new ThirdClass();
			thirdClass.ThirdProperty	= "Third property";

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "Second.SecondProperty", thirdClass, "ThirdProperty", BindingModes.OneWay);

			SecondClass newSecondClass		= new SecondClass();
			newSecondClass.SecondProperty	= "New second property";
			firstClass.Second				= newSecondClass;

			Assert.AreEqual(newSecondClass.SecondProperty, thirdClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding one object's property to two other objects' properties, when the binding is first created.
		/// 
		/// FirstClass.FirstProperty -> SecondClass.Other.ThirdProperty
		/// </summary>
		[TestMethod]
		public void OneToTwo_OneWayBinding_InitialValue()
		{
			FirstClass firstClass		= new FirstClass();
			firstClass.FirstProperty	= "First property";

			ThirdClass thirdClass		= new ThirdClass();
			thirdClass.ThirdProperty	= "Third property";

			SecondClass secondClass		= new SecondClass();
			secondClass.Third			= thirdClass;

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "FirstProperty", secondClass, "Third.ThirdProperty", BindingModes.OneWay);

			Assert.AreEqual(firstClass.FirstProperty, thirdClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding one object's property to two other objects' properties, after modifying a source property.
		/// 
		/// FirstClass.FirstProperty -> SecondClass.Other.ThirdProperty
		/// </summary>
		[TestMethod]
		public void OneToTwo_OneWayBinding_AfterModifyingSourceProperty()
		{
			FirstClass firstClass		= new FirstClass();
			firstClass.FirstProperty	= "First property";

			ThirdClass thirdClass		= new ThirdClass();
			thirdClass.ThirdProperty	= "Third property";

			SecondClass secondClass		= new SecondClass();
			secondClass.Third			= thirdClass;

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "FirstProperty", secondClass, "Third.ThirdProperty", BindingModes.OneWay);
			firstClass.FirstProperty	= "New first property";

			Assert.AreEqual(firstClass.FirstProperty, thirdClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding one object's property to two other objects' properties, when the binding is first created.
		/// 
		/// sourceFirst.Second.SecondProperty -> destinationFirst.Second.SecondProperty
		/// </summary>
		[TestMethod]
		public void TwoToTwo_OneWayBinding_InitialValue()
		{
			SecondClass sourceSecond			= new SecondClass();
			sourceSecond.SecondProperty			= "Source second property";

			FirstClass sourceFirst				= new FirstClass();
			sourceFirst.Second					= sourceSecond;

			SecondClass destinationSecond		= new SecondClass();
			destinationSecond.SecondProperty	= "Destination second property";

			FirstClass destinationFirst			= new FirstClass();
			destinationFirst.Second				= destinationSecond;

			HierarchicalBinding binding = new HierarchicalBinding(sourceFirst, "Second.SecondProperty", destinationFirst, "Second.SecondProperty", BindingModes.OneWay);

			Assert.AreEqual(sourceSecond.SecondProperty, destinationSecond.SecondProperty);
		}

		/// <summary>
		/// Tests binding one object's property to two other objects' properties, after modifying a source property.
		/// 
		/// sourceFirst.Second.SecondProperty -> destinationFirst.Second.SecondProperty
		/// </summary>
		[TestMethod]
		public void TwoToTwo_OneWayBinding_AfterModifyingSourceProperty()
		{
			SecondClass sourceSecond			= new SecondClass();
			sourceSecond.SecondProperty			= "Source second property";

			FirstClass sourceFirst				= new FirstClass();
			sourceFirst.Second					= sourceSecond;

			SecondClass destinationSecond		= new SecondClass();
			destinationSecond.SecondProperty	= "Destination second property";

			FirstClass destinationFirst			= new FirstClass();
			destinationFirst.Second				= destinationSecond;

			HierarchicalBinding binding = new HierarchicalBinding(sourceFirst, "Second.SecondProperty", destinationFirst, "Second.SecondProperty", BindingModes.OneWay);

			SecondClass newDestinationSecond	= new SecondClass();
			destinationFirst.Second				= newDestinationSecond;

			SecondClass newSourceSecond			= new SecondClass();
			newSourceSecond.SecondProperty		= "New destination second property";
			sourceFirst.Second					= newSourceSecond;

			Assert.AreEqual(newSourceSecond.SecondProperty, newDestinationSecond.SecondProperty);
		}

		/// <summary>
		/// Tests binding one object's non-existing property to one other object's property, when the binding is first created.
		/// 
		/// FirstClass.DoesNotExist -> SecondClass.SecondProperty
		/// </summary>
		[TestMethod]
		public void OneToOne_WrongSourceBinding_OneWayBinding_InitialValue()
		{
			FirstClass	firstClass		= new FirstClass();
			firstClass.FirstProperty	= "First property";

			SecondClass secondClass		= new SecondClass();
			secondClass.SecondProperty	= "Second property";

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "DoesNotExist", secondClass, "SecondProperty", BindingModes.OneWay, "FallbackValue");

			Assert.AreEqual("FallbackValue", secondClass.SecondProperty);
		}

		/// <summary>
		/// Tests binding two objects' properties to one other object's property, when the binding is first created.
		/// 
		/// FirstClass.Second.SecondProperty -> ThirdClass.ThirdProperty
		/// </summary>
		[TestMethod]
		public void TwoToOne_NullSourcePart_OneWayBinding_InitialValue()
		{
			FirstClass firstClass		= new FirstClass();
			firstClass.Second			= null;

			ThirdClass thirdClass		= new ThirdClass();
			thirdClass.ThirdProperty	= "Third property";

			HierarchicalBinding binding = new HierarchicalBinding(firstClass, "Second.SecondProperty", thirdClass, "ThirdProperty", BindingModes.OneWay, "FallbackValue");

			Assert.AreEqual("FallbackValue", thirdClass.ThirdProperty);
		}

		/// <summary>
		/// Test binding to a source property that hasn't been initialized yet.
		/// </summary>
		[TestMethod]
		public void OneWayBinding_InvalidInitialSourceValue()
		{
			const string fallbackValue = "FallbackValue";
			const string targetValue = "Third property";

			TestObject first		= new TestObject();
			TestObject second		= new TestObject();
			TestObject third		= new TestObject();
			TestObject target		= new TestObject();

			third.Value	= targetValue;

			HierarchicalBinding binding = new HierarchicalBinding(first, "Other.Other.Value", target, "Value", BindingModes.OneWay, fallbackValue);
			Assert.AreEqual(fallbackValue, target.Value);

			first.Other	= second;
			Assert.AreEqual(fallbackValue, target.Value);

			second.Other = third;
			Assert.AreEqual(targetValue, target.Value);
		}

		/// <summary>
		/// Test binding to a source property that hasn't been initialized yet.
		/// 
		/// This time initializing the source values in reverse order.
		/// </summary>
		[TestMethod]
		public void OneWayBinding_InvalidInitialSourceValueReverse()
		{
			const string fallbackValue = "FallbackValue";
			const string targetValue = "Third property";

			TestObject first        = new TestObject();
			TestObject second		= new TestObject();
			TestObject third		= new TestObject();
			TestObject target		= new TestObject();

			third.Value	= targetValue;

			HierarchicalBinding binding = new HierarchicalBinding(first, "Other.Other.Value", target, "Value", BindingModes.OneWay, fallbackValue);
			Assert.AreEqual(fallbackValue, target.Value);

			second.Other = third;
			Assert.AreEqual(fallbackValue, target.Value);

			first.Other	= second;
			Assert.AreEqual(targetValue, target.Value);
		}

		/// <summary>
		/// Test binding to a source property that hasn't been initialized yet.
		/// </summary>
		[TestMethod]
		public void OneWayBinding_InvalidInitialTargetValue()
		{
			const string initialValue	= "Initial value";
			const string fallbackValue	= "FallbackValue";
			const string targetValue	= "Source property";

			TestObject source		= new TestObject();
			source.Value			= targetValue;

			TestObject first		= new TestObject();
			TestObject second		= new TestObject();
			TestObject third		= new TestObject();
			third.Value	= initialValue;

			HierarchicalBinding binding = new HierarchicalBinding(source, "Value", first, "Other.Other.Value", BindingModes.OneWay, fallbackValue);

			// Destination binding not yet available. Expect third target's ThirdProperty to remain unmodified.
			Assert.AreEqual(initialValue, third.Value);

			// Destination binding not yet available. Expect third target's ThirdProperty to remain unmodified.
			first.Other	= second;
			Assert.AreEqual(initialValue, third.Value);

			// Destination binding now available. Expect third target's ThirdProperty to equal the first source's FirstProperty.
			second.Other = third;
			Assert.AreEqual(targetValue, third.Value);
		}

		/// <summary>
		/// Test binding to a source property that hasn't been initialized yet.
		/// 
		/// This time initializing the target values in reverse order.
		/// </summary>
		[TestMethod]
		public void OneWayBinding_InvalidInitialTargetValueReverse()
		{
			const string initialValue	= "Initial value";
			const string fallbackValue	= "FallbackValue";
			const string targetValue	= "Source property";

			TestObject source		= new TestObject();
			source.Value			= targetValue;

			TestObject first		= new TestObject();
			TestObject second		= new TestObject();
			TestObject third		= new TestObject();
			third.Value			= initialValue;

			HierarchicalBinding binding = new HierarchicalBinding(source, "Value", first, "Other.Other.Value", BindingModes.OneWay, fallbackValue);

			// Destination binding not yet available. Expect third target's ThirdProperty to remain unmodified.
			Assert.AreEqual(initialValue, third.Value);

			// Destination binding not yet available. Expect third target's ThirdProperty to remain unmodified.
			second.Other = third;
			Assert.AreEqual(initialValue, third.Value);

			// Destination binding now available. Expect third target's ThirdProperty to equal the first source's FirstProperty.
			first.Other	= second;
			Assert.AreEqual(targetValue, third.Value);
		}
	}
}
