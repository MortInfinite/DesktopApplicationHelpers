using Bindings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestData;

namespace BindingUnitTest
{
	[TestClass]
	public class HierarchicalBindingDependencyUnitTest
	{
		/// <summary>
		/// Tests binding one object's property to one other object's property, when the binding is first created.
		/// 
		/// FirstDependencyClass.FirstProperty -> SecondDependencyClass.SecondProperty
		/// </summary>
		[TestMethod]
		public void OneToOne_OneWayBinding_InitialValue()
		{
			FirstDependencyClass	firstDependencyClass		= new FirstDependencyClass();
			firstDependencyClass.FirstProperty					= "First property";

			SecondDependencyClass secondDependencyClass			= new SecondDependencyClass();
			secondDependencyClass.SecondProperty				= "Second property";

			HierarchicalBinding binding = new HierarchicalBinding(firstDependencyClass, "FirstProperty", secondDependencyClass, "SecondProperty", BindingModes.OneWay);

			Assert.AreEqual(firstDependencyClass.FirstProperty, secondDependencyClass.SecondProperty);
		}

		/// <summary>
		/// Tests binding one object's property to one other object's property, after modifying the source property.
		/// 
		/// FirstDependencyClass.FirstProperty -> SecondDependencyClass.SecondProperty
		/// </summary>
		[TestMethod]
		public void OneToOne_OneWayBinding_AfterModifyingSourceProperty()
		{
			FirstDependencyClass	firstDependencyClass		= new FirstDependencyClass();
			firstDependencyClass.FirstProperty					= "First property";

			SecondDependencyClass secondDependencyClass			= new SecondDependencyClass();
			secondDependencyClass.SecondProperty				= "Second property";

			HierarchicalBinding binding = new HierarchicalBinding(firstDependencyClass, "FirstProperty", secondDependencyClass, "SecondProperty", BindingModes.OneWay);

			firstDependencyClass.FirstProperty	= "New first property";

			Assert.AreEqual(firstDependencyClass.FirstProperty, secondDependencyClass.SecondProperty);
		}

		/// <summary>
		/// Tests binding two objects' properties to one other object's property, when the binding is first created.
		/// 
		/// FirstDependencyClass.Other.SecondProperty -> ThirdDependencyClass.ThirdProperty
		/// </summary>
		[TestMethod]
		public void TwoToOne_OneWayBinding_InitialValue()
		{
			SecondDependencyClass secondDependencyClass		= new SecondDependencyClass();
			secondDependencyClass.SecondProperty			= "Second property";

			FirstDependencyClass firstDependencyClass		= new FirstDependencyClass();
			firstDependencyClass.Second						= secondDependencyClass;

			ThirdDependencyClass thirdDependencyClass		= new ThirdDependencyClass();
			thirdDependencyClass.ThirdProperty				= "Third property";

			HierarchicalBinding binding = new HierarchicalBinding(firstDependencyClass, "Second.SecondProperty", thirdDependencyClass, "ThirdProperty", BindingModes.OneWay);

			Assert.AreEqual(secondDependencyClass.SecondProperty, thirdDependencyClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding two objects' properties to one other object's property, after modifying a source property.
		/// 
		/// FirstDependencyClass.Other.SecondProperty -> ThirdDependencyClass.ThirdProperty
		/// </summary>
		[TestMethod]
		public void TwoToOne_OneWayBinding_AfterModifyingSourceProperty()
		{
			SecondDependencyClass secondDependencyClass		= new SecondDependencyClass();
			secondDependencyClass.SecondProperty			= "Second property";

			FirstDependencyClass firstDependencyClass		= new FirstDependencyClass();
			firstDependencyClass.Second						= secondDependencyClass;

			ThirdDependencyClass thirdDependencyClass		= new ThirdDependencyClass();
			thirdDependencyClass.ThirdProperty				= "Third property";

			HierarchicalBinding binding = new HierarchicalBinding(firstDependencyClass, "Second.SecondProperty", thirdDependencyClass, "ThirdProperty", BindingModes.OneWay);
			secondDependencyClass.SecondProperty	= "New second property";

			Assert.AreEqual(secondDependencyClass.SecondProperty, thirdDependencyClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding two objects' properties to one other object's property, after modifying a source property.
		/// 
		/// FirstDependencyClass.Other.SecondProperty -> ThirdDependencyClass.ThirdProperty
		/// </summary>
		[TestMethod]
		public void TwoToOne_OneWayBinding_AfterModifyingSourcePropertyPart()
		{
			SecondDependencyClass secondDependencyClass		= new SecondDependencyClass();
			secondDependencyClass.SecondProperty			= "Second property";

			FirstDependencyClass firstDependencyClass		= new FirstDependencyClass();
			firstDependencyClass.Second						= secondDependencyClass;

			ThirdDependencyClass thirdDependencyClass		= new ThirdDependencyClass();
			thirdDependencyClass.ThirdProperty				= "Third property";

			HierarchicalBinding binding = new HierarchicalBinding(firstDependencyClass, "Second.SecondProperty", thirdDependencyClass, "ThirdProperty", BindingModes.OneWay);

			SecondDependencyClass newSecondClass	= new SecondDependencyClass();
			newSecondClass.SecondProperty			= "New second property";
			firstDependencyClass.Second				= newSecondClass;

			Assert.AreEqual(newSecondClass.SecondProperty, thirdDependencyClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding one object's property to two other objects' properties, when the binding is first created.
		/// 
		/// FirstDependencyClass.FirstProperty -> SecondDependencyClass.Other.ThirdProperty
		/// </summary>
		[TestMethod]
		public void OneToTwo_OneWayBinding_InitialValue()
		{
			FirstDependencyClass firstDependencyClass		= new FirstDependencyClass();
			firstDependencyClass.FirstProperty				= "First property";

			ThirdDependencyClass thirdDependencyClass		= new ThirdDependencyClass();
			thirdDependencyClass.ThirdProperty				= "Third property";

			SecondDependencyClass secondDependencyClass		= new SecondDependencyClass();
			secondDependencyClass.Third						= thirdDependencyClass;

			HierarchicalBinding binding = new HierarchicalBinding(firstDependencyClass, "FirstProperty", secondDependencyClass, "Third.ThirdProperty", BindingModes.OneWay);

			Assert.AreEqual(firstDependencyClass.FirstProperty, thirdDependencyClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding one object's property to two other objects' properties, after modifying a source property.
		/// 
		/// FirstDependencyClass.FirstProperty -> SecondDependencyClass.Other.ThirdProperty
		/// </summary>
		[TestMethod]
		public void OneToTwo_OneWayBinding_AfterModifyingSourceProperty()
		{
			FirstDependencyClass firstDependencyClass		= new FirstDependencyClass();
			firstDependencyClass.FirstProperty				= "First property";

			ThirdDependencyClass thirdDependencyClass		= new ThirdDependencyClass();
			thirdDependencyClass.ThirdProperty				= "Third property";

			SecondDependencyClass secondDependencyClass		= new SecondDependencyClass();
			secondDependencyClass.Third						= thirdDependencyClass;

			HierarchicalBinding binding = new HierarchicalBinding(firstDependencyClass, "FirstProperty", secondDependencyClass, "Third.ThirdProperty", BindingModes.OneWay);
			firstDependencyClass.FirstProperty	= "New first property";

			Assert.AreEqual(firstDependencyClass.FirstProperty, thirdDependencyClass.ThirdProperty);
		}

		/// <summary>
		/// Tests binding one object's property to two other objects' properties, when the binding is first created.
		/// 
		/// sourceFirst.Second.SecondProperty -> destinationFirst.Second.SecondProperty
		/// </summary>
		[TestMethod]
		public void TwoToTwo_OneWayBinding_InitialValue()
		{
			SecondDependencyClass sourceSecond			= new SecondDependencyClass();
			sourceSecond.SecondProperty					= "Source second property";

			FirstDependencyClass sourceFirst			= new FirstDependencyClass();
			sourceFirst.Second							= sourceSecond;

			SecondDependencyClass destinationSecond		= new SecondDependencyClass();
			destinationSecond.SecondProperty			= "Destination second property";

			FirstDependencyClass destinationFirst		= new FirstDependencyClass();
			destinationFirst.Second						= destinationSecond;

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
			SecondDependencyClass sourceSecond			= new SecondDependencyClass();
			sourceSecond.SecondProperty					= "Source second property";

			FirstDependencyClass sourceFirst			= new FirstDependencyClass();
			sourceFirst.Second							= sourceSecond;

			SecondDependencyClass destinationSecond		= new SecondDependencyClass();
			destinationSecond.SecondProperty			= "Destination second property";

			FirstDependencyClass destinationFirst		= new FirstDependencyClass();
			destinationFirst.Second						= destinationSecond;

			HierarchicalBinding binding = new HierarchicalBinding(sourceFirst, "Second.SecondProperty", destinationFirst, "Second.SecondProperty", BindingModes.OneWay);

			SecondDependencyClass newDestinationSecond	= new SecondDependencyClass();
			destinationFirst.Second						= newDestinationSecond;

			SecondDependencyClass newSourceSecond		= new SecondDependencyClass();
			newSourceSecond.SecondProperty				= "New destination second property";
			sourceFirst.Second							= newSourceSecond;

			Assert.AreEqual(newSourceSecond.SecondProperty, newDestinationSecond.SecondProperty);
		}

		/// <summary>
		/// Tests binding one object's non-existing property to one other object's property, when the binding is first created.
		/// 
		/// FirstDependencyClass.DoesNotExist -> SecondDependencyClass.SecondProperty
		/// </summary>
		[TestMethod]
		public void OneToOne_WrongSourceBinding_OneWayBinding_InitialValue()
		{
			FirstDependencyClass	firstDependencyClass	= new FirstDependencyClass();
			firstDependencyClass.FirstProperty				= "First property";

			SecondDependencyClass secondDependencyClass		= new SecondDependencyClass();
			secondDependencyClass.SecondProperty			= "Second property";

			HierarchicalBinding binding = new HierarchicalBinding(firstDependencyClass, "DoesNotExist", secondDependencyClass, "SecondProperty", BindingModes.OneWay, "FallbackValue");

			Assert.AreEqual("FallbackValue", secondDependencyClass.SecondProperty);
		}

		/// <summary>
		/// Tests binding two objects' properties to one other object's property, when the binding is first created.
		/// 
		/// FirstDependencyClass.Second.SecondProperty -> ThirdDependencyClass.ThirdProperty
		/// </summary>
		[TestMethod]
		public void TwoToOne_NullSourcePart_OneWayBinding_InitialValue()
		{
			FirstDependencyClass firstDependencyClass		= new FirstDependencyClass();
			firstDependencyClass.Second						= null;

			ThirdDependencyClass thirdDependencyClass		= new ThirdDependencyClass();
			thirdDependencyClass.ThirdProperty				= "Third property";

			HierarchicalBinding binding = new HierarchicalBinding(firstDependencyClass, "Second.SecondProperty", thirdDependencyClass, "ThirdProperty", BindingModes.OneWay, "FallbackValue");

			Assert.AreEqual("FallbackValue", thirdDependencyClass.ThirdProperty);
		}

		/// <summary>
		/// Test binding to a source property that hasn't been initialized yet.
		/// </summary>
		[TestMethod]
		public void OneWayBinding_InvalidInitialSourceValue()
		{
			const string fallbackValue = "FallbackValue";
			const string targetValue = "Third property";

			TestDependencyObject first		= new TestDependencyObject();
			TestDependencyObject second		= new TestDependencyObject();
			TestDependencyObject third		= new TestDependencyObject();
			TestDependencyObject target		= new TestDependencyObject();

			third.Value			= targetValue;

			HierarchicalBinding binding = new HierarchicalBinding(first, "Other.Other.Value", target, "Value", BindingModes.OneWay, fallbackValue);
			Assert.AreEqual(fallbackValue, target.Value);

			first.Other		= second;
			Assert.AreEqual(fallbackValue, target.Value);

			second.Other	= third;
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

			TestDependencyObject first        = new TestDependencyObject();
			TestDependencyObject second		= new TestDependencyObject();
			TestDependencyObject third		= new TestDependencyObject();
			TestDependencyObject target		= new TestDependencyObject();

			third.Value			= targetValue;

			HierarchicalBinding binding = new HierarchicalBinding(first, "Other.Other.Value", target, "Value", BindingModes.OneWay, fallbackValue);
			Assert.AreEqual(fallbackValue, target.Value);

			second.Other	= third;
			Assert.AreEqual(fallbackValue, target.Value);

			first.Other		= second;
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

			TestDependencyObject source		= new TestDependencyObject();
			source.Value			= targetValue;

			TestDependencyObject first		= new TestDependencyObject();
			TestDependencyObject second		= new TestDependencyObject();
			TestDependencyObject third		= new TestDependencyObject();
			third.Value			= initialValue;

			HierarchicalBinding binding = new HierarchicalBinding(source, "Value", first, "Other.Other.Value", BindingModes.OneWay, fallbackValue);

			// Destination binding not yet available. Expect third target's ThirdProperty to remain unmodified.
			Assert.AreEqual(initialValue, third.Value);

			// Destination binding not yet available. Expect third target's ThirdProperty to remain unmodified.
			first.Other	= second;
			Assert.AreEqual(initialValue, third.Value);

			// Destination binding now available. Expect third target's ThirdProperty to equal the first source's FirstProperty.
			second.Other	= third;
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

			TestDependencyObject source		= new TestDependencyObject();
			source.Value			= targetValue;

			TestDependencyObject first		= new TestDependencyObject();
			TestDependencyObject second		= new TestDependencyObject();
			TestDependencyObject third		= new TestDependencyObject();
			third.Value			= initialValue;

			HierarchicalBinding binding = new HierarchicalBinding(source, "Value", first, "Other.Other.Value", BindingModes.OneWay, fallbackValue);

			// Destination binding not yet available. Expect third target's ThirdProperty to remain unmodified.
			Assert.AreEqual(initialValue, third.Value);

			// Destination binding not yet available. Expect third target's ThirdProperty to remain unmodified.
			second.Other	= third;
			Assert.AreEqual(initialValue, third.Value);

			// Destination binding now available. Expect third target's ThirdProperty to equal the first source's FirstProperty.
			first.Other	= second;
			Assert.AreEqual(targetValue, third.Value);
		}
	}
}
