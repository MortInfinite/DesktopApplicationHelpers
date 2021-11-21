using System;
using System.Collections.ObjectModel;
using Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CollectionTest
{
	[TestClass]
	public class DualCollectionTest
	{
		[TestMethod]
		public void Insert_UsingDefaultObjectFactory()
		{
			ObservableCollection<int>		primaryCollection	= new ObservableCollection<int>();
			ObservableCollection<object>	secondaryCollection = new ObservableCollection<object>();

			DualCollection dualCollection = new DualCollection(primaryCollection, secondaryCollection);

			primaryCollection.Add(10);

			Assert.IsTrue(primaryCollection.Count == 1);
			Assert.IsTrue(secondaryCollection.Count == 1);
		}

		[TestMethod]
		public void Replace_UsingDefaultObjectFactory()
		{
			ObservableCollection<int>		primaryCollection	= new ObservableCollection<int>();
			ObservableCollection<object>	secondaryCollection = new ObservableCollection<object>();

			DualCollection dualCollection = new DualCollection(primaryCollection, secondaryCollection);

			primaryCollection.Add(10);
			primaryCollection[0] = 14;

			Assert.IsTrue(primaryCollection.Count == 1);
			Assert.IsTrue(secondaryCollection.Count == 1);
		}

		[TestMethod]
		public void Insert_UsingCustomObjectFactory()
		{
			ObservableCollection<int>	primaryCollection	= new ObservableCollection<int>();
			ObservableCollection<bool>	secondaryCollection = new ObservableCollection<bool>();

			DualCollection dualCollection = new DualCollection(primaryCollection, secondaryCollection);
			dualCollection.CreateSecondary = (int index, object primaryValue) => {return false;};

			primaryCollection.Add(10);

			Assert.IsTrue(primaryCollection.Count == 1);
			Assert.IsTrue(secondaryCollection.Count == 1);
			Assert.IsTrue(primaryCollection[0] == 10);
			Assert.IsTrue(secondaryCollection[0] == false);
		}

		[TestMethod]
		public void InsertMultiple_UsingCustomObjectFactory()
		{
			ObservableCollection<int>	primaryCollection	= new ObservableCollection<int>();
			ObservableCollection<int>	secondaryCollection = new ObservableCollection<int>();

			DualCollection dualCollection = new DualCollection(primaryCollection, secondaryCollection);
			dualCollection.CreateSecondary	= (int index, object primaryValue) => {return ((int) primaryValue*2);};

			for(int count = 0; count<10; count++)
				primaryCollection.Add(count);

			Assert.IsTrue(secondaryCollection.Count == primaryCollection.Count);
			for(int count = 0; count<primaryCollection.Count; count++)
				Assert.IsTrue(secondaryCollection[count] == primaryCollection[count]*2);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void InsertIntoSecondary()
		{
			ObservableCollection<int>	primaryCollection	= new ObservableCollection<int>();
			ObservableCollection<int>	secondaryCollection = new ObservableCollection<int>();

			DualCollection dualCollection = new DualCollection(primaryCollection, secondaryCollection);
			dualCollection.CreateSecondary = (int index, object otherCollection) => {return ((int) otherCollection*2);};

			for(int count = 0; count<10; count++)
				primaryCollection.Add(count);

			// Must throw exception.
			secondaryCollection.Add(5);
		}

		[TestMethod]
		public void RemoveSingle()
		{
			ObservableCollection<int>	primaryCollection	= new ObservableCollection<int>();
			ObservableCollection<int>	secondaryCollection = new ObservableCollection<int>();

			DualCollection dualCollection = new DualCollection(primaryCollection, secondaryCollection);
			dualCollection.CreateSecondary	= (int index, object primaryValue) => {return ((int) primaryValue*2);};

			for(int count = 0; count<10; count++)
				primaryCollection.Add(count);

			primaryCollection.RemoveAt(5);

			Assert.IsTrue(secondaryCollection.Count == primaryCollection.Count);
			for(int count = 0; count<primaryCollection.Count; count++)
				Assert.IsTrue(secondaryCollection[count] == primaryCollection[count]*2);
		}

		[TestMethod]
		public void ReplaceSingle()
		{
			ObservableCollection<int>	primaryCollection	= new ObservableCollection<int>();
			ObservableCollection<int>	secondaryCollection = new ObservableCollection<int>();

			DualCollection dualCollection	= new DualCollection(primaryCollection, secondaryCollection);
			dualCollection.CreateSecondary	= (int index, object primaryValue) => {return ((int) primaryValue*2);};
			dualCollection.UpdateSecondary	= (int index, object primaryValue, object currentValue) => {return ((int) primaryValue*2);};

			for(int count = 0; count<10; count++)
				primaryCollection.Add(count);

			primaryCollection[5] = 14;

			Assert.IsTrue(secondaryCollection.Count == primaryCollection.Count);
			for(int count = 0; count<primaryCollection.Count; count++)
				Assert.IsTrue(secondaryCollection[count] == primaryCollection[count]*2);
		}

		[TestMethod]
		public void Move_UsingCustomObjectFactory()
		{
			ObservableCollection<int>	primaryCollection	= new ObservableCollection<int>();
			ObservableCollection<int>	secondaryCollection = new ObservableCollection<int>();

			DualCollection dualCollection = new DualCollection(primaryCollection, secondaryCollection);
			dualCollection.CreateSecondary = (int index, object primaryValue) => {return (int) primaryValue*2;};

			for(int count = 0; count<10; count++)
				primaryCollection.Add(count);

			primaryCollection.Move(1, 5);
			primaryCollection.Move(9, 7);

			Assert.IsTrue(secondaryCollection.Count == primaryCollection.Count);
			for(int count = 0; count<primaryCollection.Count; count++)
				Assert.IsTrue(secondaryCollection[count] == primaryCollection[count]*2);
		}
		[TestMethod]
		public void EnumerateDualCollection()
		{
			ObservableCollection<int>	primaryCollection	= new ObservableCollection<int>();
			ObservableCollection<int>	secondaryCollection = new ObservableCollection<int>();

			DualCollection dualCollection = new DualCollection(primaryCollection, secondaryCollection);
			dualCollection.CreateSecondary	= (int index, object primaryValue) => {return ((int) primaryValue*2);};

			for(int count = 0; count<10; count++)
				primaryCollection.Add(count);

			int iterations = 0;

			foreach(Tuple<object, object> currentValue in dualCollection)
			{
				Assert.IsTrue((int) (currentValue.Item2) == (int) (currentValue.Item1)*2);
				iterations++;
			}

			Assert.IsTrue(iterations == primaryCollection.Count);
		}
	}
}
