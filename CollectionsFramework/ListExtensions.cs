using System.Collections.Generic;

namespace Collections
{
	public static class ListExtensions
	{
		/// <summary>
		/// Performs a sort on the list, based on the MergeSort algorithm.
		/// </summary>
		/// <param name="list">List to sort.</param>
		/// <param name="startIndex">Index of first element, in the list, to sort.</param>
		/// <param name="endIndex">Index of last element, in the list, to sort.</param>
		/// <param name="compare">Comparison method used to compare two items.</param>
		public static void MergeSort<T>(this IList<T> list, int startIndex, int endIndex, CompareDelegate<T> compare)
		{
			int	startHighIndex,
				middleIndex,
				endLowIndex,
				count;
			T	swapValue;
			
			if (startIndex < endIndex) 
			{
				middleIndex = (startIndex + endIndex) / 2;
			
				MergeSort(list, startIndex, middleIndex, compare);
				MergeSort(list, middleIndex + 1, endIndex, compare);

				endLowIndex		= middleIndex;
				startHighIndex	= middleIndex + 1;

				while (startIndex <= endLowIndex & startHighIndex <= endIndex) 
				{
					if (compare(list[startIndex], list[startHighIndex]) < 0) 
						startIndex++;
					else 
					{
						swapValue = list[startHighIndex];
						for (count = startHighIndex - 1; count >= startIndex; count--)
							list[count+1] = list[count];

						list[startIndex] = swapValue;
						startIndex++;
						endLowIndex++;
						startHighIndex++;
					}
				}
			}
		}

		/// <summary>
		/// Perform a binary search on the specified list, to determine the index at which to insert the <paramref name="value"/>.
		/// 
		/// This method requires the list to be sorted. Calling this method on an unsorted list will return a meaningless value.
		/// </summary>
		/// <param name="list">List to determine insert index of.</param>
		/// <param name="value">Value to search for the sorted insert index of.</param>
		/// <param name="startIndex">Index of first element, in the list, to search.</param>
		/// <param name="endIndex">Index of last element, in the list, to search.</param>
		/// <returns>Index of a result that matches the specified value.</returns>
		public static int GetSortedInsertIndex<T>(this IList<T> list, T value, int startIndex, int endIndex, CompareDelegate<T> compare)
		{
			if(list.Count == 0)
				return 0;
			if(startIndex > endIndex)
				return -1;

			int middleIndex = (endIndex-startIndex)/2+startIndex;
			
			int compareResult = compare(value, list[middleIndex]);

			// Value is same value as value at middle index. New item must be inserted after the middle index.
			if(compareResult == 0)
				return middleIndex+1;

			// Value is less than value at middle index.
			if(compareResult < 0)
			{
				// Middle index is at the first item in the list. New item must be inserted before the first item.
				if(middleIndex == startIndex)
					return middleIndex;

				//if(startIndex == endIndex)
				//	return middleIndex-1;

				return GetSortedInsertIndex(list, value, startIndex, middleIndex-1, compare);
			}
			
			// Value is greater than value at middle index.
			if(compareResult > 0)
			{
				// Middle index is at the last item in the list. New item must be inserted after the last item.
				if(middleIndex == endIndex)
					return middleIndex+1;
				
				//if(startIndex == endIndex)
				//	return middleIndex+1;

				return GetSortedInsertIndex(list, value, middleIndex+1, endIndex, compare);
			}

			return -1;
		}
	}
}
