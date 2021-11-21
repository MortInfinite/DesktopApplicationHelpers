using System;
using System.Collections.Generic;

namespace Collections
{
	/// <summary>
	/// Provides methods for searching a list for a value.
	/// </summary>
	/// <typeparam name="T">Data type stored in the list.</typeparam>
	public static class Search<T> where T: IComparable
	{
		/// <summary>
		/// Perform a binary search on the specified list for the specified value.
		/// </summary>
		/// <param name="list">List to search.</param>
		/// <param name="value">Value to search for.</param>
		/// <returns>Index of a result that matches the specified value.</returns>
		public static int BinarySearch(IList<T> list, T value)
		{
			return BinarySearch(list, value, 0, list.Count-1);
		}

		/// <summary>
		/// Perform a binary search on the specified list for the specified value.
		/// </summary>
		/// <param name="list">List to search.</param>
		/// <param name="value">Value to search for.</param>
		/// <param name="startIndex">Index of first element, in the list, to search.</param>
		/// <param name="endIndex">Index of last element, in the list, to search.</param>
		/// <returns>Index of a result that matches the specified value.</returns>
		public static int BinarySearch(IList<T> list, T value, int startIndex, int endIndex)
		{
			int middle = (endIndex-startIndex)/2+startIndex;
			
			int compareResult = value.CompareTo(list[middle]);
			if(compareResult == 0)
				return middle;
			else if(startIndex >= endIndex)
				return -1;
			else if(compareResult < 0 && middle > 0)
				return BinarySearch(list, value, startIndex, middle-1);
			else if(compareResult > 0 && middle < list.Count-1)
				return BinarySearch(list, value, middle+1, endIndex);
			else 
				return -1;
		}
	}

	/// <summary>
	/// Provides methods for searching a list for a value, where the value is extracted from an element in the list.
	/// </summary>
	/// <typeparam name="ListT">Data type stored in the list.</typeparam>
	/// <typeparam name="ValueT">Data type to retrieve from each item in the list.</typeparam>
	public static class Search<ListT, ValueT> where ValueT: IComparable
	{
		/// <summary>
		/// Perform a binary search on the specified list for the specified value.
		/// </summary>
		/// <param name="list">List to search.</param>
		/// <param name="value">Value to search for.</param>
		/// <param name="listAccessDelegate">Delegate called to retrieve the value of list elements.</param>
		/// <returns>Index of a result that matches the specified value.</returns>
		public static int BinarySearch(IList<ListT> list, ValueT value, ListAccessDelegate listAccessDelegate)
		{
			return BinarySearch(list, value, 0, list.Count-1, listAccessDelegate);
		}

		/// <summary>
		/// Perform a binary search on the specified list for the specified value.
		/// </summary>
		/// <param name="list">List to search.</param>
		/// <param name="value">Value to search for.</param>
		/// <param name="startIndex">Index of first element, in the list, to search.</param>
		/// <param name="endIndex">Index of last element, in the list, to search.</param>
		/// <param name="listAccessDelegate">Delegate called to retrieve the value of list elements.</param>
		/// <returns>Index of a result that matches the specified value.</returns>
		public static int BinarySearch(IList<ListT> list, ValueT value, int startIndex, int endIndex, ListAccessDelegate listAccessDelegate)
		{
			// Get the middle between the start and the end index.
			int middle = (endIndex-startIndex)/2+startIndex;

			// Retrieve the value from the list access delegate.
			ValueT delegateValue = listAccessDelegate(list, middle);
	
			// Compare the middle value to the specified value.
			int compareResult = value.CompareTo(delegateValue);

			if(compareResult == 0)
				return middle;
			else if(startIndex >= endIndex)
				return -1;
			else if(compareResult < 0 && middle > 0)
				return BinarySearch(list, value, startIndex, middle-1, listAccessDelegate);
			else if(compareResult > 0 && middle < list.Count-1)
				return BinarySearch(list, value, middle+1, endIndex, listAccessDelegate);
			else 
				return -1;
		}

		/// <summary>
		/// Delegate called to retrieve the value of a list element.
		/// Useful for retrieving properties within the objects stored in the list.
		/// </summary>
		/// <param name="list">List to retrieve value from.</param>
		/// <param name="index">Index of value to retrieve.</param>
		/// <returns>Value we want to retrieve from the list.</returns>
		public delegate ValueT ListAccessDelegate(IList<ListT> list, int index);
	}
}
