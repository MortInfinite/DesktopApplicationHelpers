namespace Bindings
{
	public enum BindingModes
	{
		/// <summary>
		/// When the property changes, on the source object, its value is copied to the destination object.
		/// </summary>
		OneWay,

		/// <summary>
		/// When the property changes, on the source object, its value is copied to the destination object.
		/// When the property changes, on the destination object, its value is copied to the source object.
		/// </summary>
		TwoWay
	}
}
