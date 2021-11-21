using System.Collections.Generic;

namespace Collections
{
	/// <summary>
	/// Provides extension methods for <see cref="IDictionary"/>.
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		/// Returns a value from the dictionary.
		/// If the specified key wasn't found in the dictionary, return the specified defaultValue.
		/// </summary>
		/// <typeparam name="TKey">Type of key used in the dictionary.</typeparam>
		/// <typeparam name="TValue">Type of value used in the dictionary.</typeparam>
		/// <param name="dictionary">Dictionary in which to look up the value.</param>
		/// <param name="key">Key to look for in the dictionary.</param>
		/// <param name="defaultValue">Value to return, if the key wasn't found in the dictionary.</param>
		/// <returns>Returns the value matching the specified key or the defaultValue if the key wasn't found.</returns>
		public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue=default(TValue))
		{
			TValue value;
			bool exists = dictionary.TryGetValue(key, out value);
			if(!exists)
				return defaultValue;

			return value;
		}
	}
}
