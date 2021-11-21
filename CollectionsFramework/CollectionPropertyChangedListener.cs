using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Collections
{
	/// <summary>
	/// Listens to events raised by INotifyCollectionChanged and INotifyPropertyChanged, from a source collection,
	/// and then raises matching events using a specified synchronization context and optionally raises the events
	/// using asynchronous events.
	/// </summary>
	/// <typeparam name="T">Type of object to contain in the list.</typeparam>
	public class CollectionPropertyChangedListener<T> :	IDisposable,
														INotifyCollectionChanged, 
														INotifyPropertyChanged,
														INotifyCollectionPropertyChanged
	{
		#region Constructors
		/// <summary>
		/// Creates a new ConcurrentObservableCollection.
		/// </summary>
		/// <param name="notifyUsingContext">When true, raises events using the specified context.</param>
		/// <param name="notifyUsingAsync">
		/// When true, raises events asynchronously. 
		/// Please note that when setting this argument to true, notifyUsingContext must also be set to true.
		/// </param>
		/// <param name="context">
		/// Synchronization context used to raise events. 
		/// 
		/// When null is specified, the current thread's synchronization context is used.
		/// If the current thread doesn't have a synchronization context, one is created.
		/// </param>
		public CollectionPropertyChangedListener(ICollection<T> source, bool notifyUsingContext=false, bool notifyUsingAsync=false, SynchronizationContext context=null)
		{
			if(notifyUsingContext)
			{
				Context = context;

				// If no synchronization context is specified.
				if(Context == null)
				{
					// If the current thread doesn't have a SynchronizationContext yet, create one now.
					if(SynchronizationContext.Current == null)
						SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

					// Remember the SynchronizationContext to use.
					Context = SynchronizationContext.Current;
				}
				NotifyUsingAsync = notifyUsingAsync;
			}
			else if(notifyUsingAsync)
				throw new ArgumentException($"Can't set {nameof(notifyUsingAsync)} to true, when {nameof(notifyUsingContext)} is false.", nameof(notifyUsingAsync));

			Source = source;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Dispose of the object and its unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose pattern implementation.
		/// </summary>
		/// <param name="disposing">True if disposing, false if finalizing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			if(disposing)
			{
				lock(ListLockObject)
				lock(ObservedElementsLockObject)
				{
					// Unsubscribe from previous property changed event subscription.
					INotifyPropertyChanged notifyPropertyChanged = m_source as INotifyPropertyChanged;
					if(notifyPropertyChanged != null)
						notifyPropertyChanged.PropertyChanged -= Source_PropertyChanged;

					// Unsubscribe from previous collection changed event subscription.
					INotifyCollectionChanged notifyCollectionChanged = m_source as INotifyCollectionChanged;
					if(notifyCollectionChanged != null)
						notifyCollectionChanged.CollectionChanged -= Source_CollectionChanged;

					// Unsubscribe from observed elements.
					foreach(T oldItem in ObservedElements)
					{
						// Stop listening to the element's properties changing.
						notifyPropertyChanged = oldItem as INotifyPropertyChanged;
						if(notifyPropertyChanged != null)
							notifyPropertyChanged.PropertyChanged -= Element_PropertyChanged;
					}

					// Clear the list of observed elements.
					ObservedElements.Clear();
				}
			}

			Disposed = true;
		}

		/// <summary>
		/// Indicates if the object has been disposed.
		/// </summary>
		public bool Disposed
		{
			get;
			protected set;
		}
		#endregion

		#region INotifyPropertyChanged
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies subscribers that the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if(string.IsNullOrEmpty(propertyName))
                throw new ArgumentException($"The {nameof(propertyName)} argument wasn't specified.", nameof(propertyName));

			Action notifyDelegate = () =>
			{
	            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			};

			Notify(notifyDelegate);
        }
		#endregion

		#region INotifyCollectionChanged implementation
		/// <summary>
		/// Occurs when the collection changes.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Notifies subscribers that the collection changed.
		/// </summary>
		/// <param name="notifyCollectionChangedEventArgs">Information about the event.</param>
		protected virtual void NotifyCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			Action notifyDelegate = () =>
			{
				CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
			};

			Notify(notifyDelegate);
		}
		#endregion
		
		#region INotifyCollectionPropertyChanged
		/// <summary>
		/// Occurs when a property value changes, in an element contained in a collection.
		/// </summary>
		public event CollectionPropertyChangedDelegate CollectionPropertyChanged;

		/// <summary>
		/// Notifies subscribers that a property value has changed, in an element contained in the collection.
		/// </summary>
		/// <param name="notifyCollectionChangedEventArgs">Information about the event.</param>
		protected virtual void NotifyCollectionPropertyChanged(object element, string propertyName)
		{
			Action notifyDelegate = () =>
			{
				CollectionPropertyChanged?.Invoke(this, element, propertyName);
			};

			Notify(notifyDelegate);
		}

		#endregion
		
		#region Properties
		/// <summary>
		/// Collection containing elements to filter.
		/// </summary>
		public ICollection<T> Source
		{
			get
			{
				return m_source;
			}
			set
			{
				if(value == m_source)
					return;

				if(value == null)
					throw new ArgumentNullException(nameof(Source));

				if(Disposed)
					throw new ObjectDisposedException(GetType().Name);

				lock(ListLockObject)
				lock(ObservedElementsLockObject)
				{
					// Unsubscribe from previous property changed event subscription.
					INotifyPropertyChanged notifyPropertyChanged = m_source as INotifyPropertyChanged;
					if(notifyPropertyChanged != null)
						notifyPropertyChanged.PropertyChanged -= Source_PropertyChanged;

					// Unsubscribe from previous collection changed event subscription.
					INotifyCollectionChanged notifyCollectionChanged = m_source as INotifyCollectionChanged;
					if(notifyCollectionChanged != null)
						notifyCollectionChanged.CollectionChanged -= Source_CollectionChanged;

					foreach(T oldItem in ObservedElements)
					{
						// Stop listening to the element's properties changing.
						notifyPropertyChanged = oldItem as INotifyPropertyChanged;
						if(notifyPropertyChanged != null)
							notifyPropertyChanged.PropertyChanged -= Element_PropertyChanged;
					}

					// Clear the list of observed elements.
					ObservedElements.Clear();

					m_source = value;

					if(value != null)
					{
						// Subscribe to property changed event subscription.
						notifyPropertyChanged = m_source as INotifyPropertyChanged;
						if(notifyPropertyChanged != null)
							notifyPropertyChanged.PropertyChanged += Source_PropertyChanged;

						// Subscribe to collection changed event subscription.
						notifyCollectionChanged = m_source as INotifyCollectionChanged;
						if(notifyCollectionChanged != null)
							notifyCollectionChanged.CollectionChanged += Source_CollectionChanged;

						// Start listening to the element's properties changing 
						// (Regardless of whether it is included in the filtered list).
						foreach(T item in value)
							ObserveElement(item);
					}
				}

				// Notify subscribers that the collection has been replaced.
				NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		/// <summary>
		/// Indicates if events should be raised asynchronously.
		/// </summary>
		public virtual bool NotifyUsingAsync
		{
			get;
			protected set;
		}

		/// <summary>
		/// SynchronizationContext used to raise events.
		/// 
		/// If this is null, events will be raised on the thread that modified the collection.
		/// </summary>
		public virtual SynchronizationContext Context
		{
			get;
			protected set;
		}

		/// <summary>
		/// Object used to lock the List for use in a single thread.
		/// </summary>
		protected virtual object ListLockObject
		{
			get;
		} = new object();

		/// <summary>
		/// List containing all observed elements.
		/// </summary>
		protected virtual IList<T> ObservedElements
		{
			get;
			set;
		} = new List<T>();

		/// <summary>
		/// Object used to lock the ObservedElements for use in a single thread.
		/// </summary>
		protected virtual object ObservedElementsLockObject
		{
			get;
		} = new object();

		#endregion

		#region Methods
		/// <summary>
		/// Add a property changed subscription to the specified element.
		/// </summary>
		/// <param name="element">Element to observe.</param>
		protected virtual void ObserveElement(T element)
		{
			lock(ObservedElementsLockObject)
			{
				INotifyPropertyChanged notifyPropertyChanged = element as INotifyPropertyChanged;
				if(notifyPropertyChanged != null)
					notifyPropertyChanged.PropertyChanged += Element_PropertyChanged;

				ObservedElements.Add(element);
			}
		}

		/// <summary>
		/// Remove a property changed subscription from the specified element.
		/// </summary>
		/// <param name="element">Element to stop observing.</param>
		protected virtual void StopObservingElement(T element)
		{
			lock(ObservedElementsLockObject)
			{
				INotifyPropertyChanged notifyPropertyChanged = element as INotifyPropertyChanged;
				if(notifyPropertyChanged != null)
					notifyPropertyChanged.PropertyChanged -= Element_PropertyChanged;

				ObservedElements.Remove(element);
			}
		}

		/// <summary>
		/// Calls the notifyDelegate using the method specified in the constructor of this class.
		/// </summary>
		/// <param name="notifyDelegate">
		/// Delegate to invoke. The delegate must raise the event.
		/// </param>
		/// <remarks>
		/// This method is only intended to be called by one of the other Notify methods.
		/// </remarks>
		protected virtual void Notify(Action notifyDelegate)
		{
			if(notifyDelegate == null)
				return;

			// If events must be raised using the synchronization context.
			if(Context != null)
			{
				if(NotifyUsingAsync)
					Context.Post((unused)=>notifyDelegate(), null);
				else
				{
					// If we are called on the same synchronization context as events must be raised on,
					// there is no need to use the Context to send the event.
					if(SynchronizationContext.Current == Context)
						notifyDelegate();
					else
						Context.Send((unused)=>notifyDelegate(), null);
				}
			}
			else
			{
				notifyDelegate();
			}
		}
		#endregion

		#region Event handlers

		/// <summary>
		/// Calls the notification event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">Information about the event.</param>
		protected virtual void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(e.PropertyName);
		}

		/// <summary>
		/// Calls the notification event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">Information about the event.</param>
		protected virtual void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				{
					foreach(T newItem in e.NewItems)
					{
						// Start listening to the element's properties changing 
						ObserveElement(newItem);
					}
				}
				break;

				case NotifyCollectionChangedAction.Remove:
				{
					foreach(T oldItem in e.OldItems)
					{
						// Stop listening to the element's properties changing.
						// (Regardless of whether it is included in the filtered list).
						StopObservingElement(oldItem);
					}
				}
				break;

				case NotifyCollectionChangedAction.Replace:
				{
					foreach(T oldItem in e.OldItems)
					{
						// Stop listening to the element's properties changing.
						// (Regardless of whether it is included in the filtered list).
						StopObservingElement(oldItem);
					}

					foreach(T newItem in e.NewItems)
					{
						// Start listening to the element's properties changing.
						// (Regardless of whether to it is included in the filtered list).
						ObserveElement(newItem);
					}
				}
				break;

				case NotifyCollectionChangedAction.Move:
				{
				}
				break;

				case NotifyCollectionChangedAction.Reset:
				{
					lock(ObservedElementsLockObject)
					{
						foreach(T oldItem in ObservedElements)
						{
							// Stop listening to the element's properties changing.
							INotifyPropertyChanged notifyPropertyChanged = oldItem as INotifyPropertyChanged;
							if(notifyPropertyChanged != null)
								notifyPropertyChanged.PropertyChanged -= Element_PropertyChanged;
						}

						// Clear the list of observed elements.
						ObservedElements.Clear();
					}
				}
				break;
			}

			// Notify subscribers that the collection has changed.
			NotifyCollectionChanged(e);
		}

		/// <summary>
		/// Evaluates the changed element, determining if the elements should be included in the filtered list.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A PropertyChangedEventArgs that contains the event data.</param>
		protected virtual void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// Notify subscribers that a property value has changed on an element in the collection.
			NotifyCollectionPropertyChanged(sender, e.PropertyName);
		}
		#endregion

		#region Fields
		/// <summary>
		/// Backing field for the Source property.
		/// </summary>
		private ICollection<T> m_source;
		#endregion
	}
}
