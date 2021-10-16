using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class View : VisualElement, IViewController, IGestureController, IGestureRecognizers
	{
		protected internal IGestureController GestureController => this;

		public static readonly BindableProperty VerticalOptionsProperty =
			BindableProperty.Create(nameof(VerticalOptions), typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
									propertyChanged: (bindable, oldvalue, newvalue) =>
									((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.VerticalOptionsChanged));

		public static readonly BindableProperty HorizontalOptionsProperty =
			BindableProperty.Create(nameof(HorizontalOptions), typeof(LayoutOptions), typeof(View), LayoutOptions.Fill,
									propertyChanged: (bindable, oldvalue, newvalue) =>
									((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.HorizontalOptionsChanged));

		public static readonly BindableProperty MarginProperty =
			BindableProperty.Create(nameof(Margin), typeof(Thickness), typeof(View), default(Thickness),
									propertyChanged: MarginPropertyChanged);

		internal static readonly BindableProperty MarginLeftProperty =
			BindableProperty.Create("MarginLeft", typeof(double), typeof(View), default(double),
									propertyChanged: OnMarginLeftPropertyChanged);

		static void OnMarginLeftPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var margin = (Thickness)bindable.GetValue(MarginProperty);
			margin.Left = (double)newValue;
			bindable.SetValue(MarginProperty, margin);
		}

		internal static readonly BindableProperty MarginTopProperty =
			BindableProperty.Create("MarginTop", typeof(double), typeof(View), default(double),
									propertyChanged: OnMarginTopPropertyChanged);

		static void OnMarginTopPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var margin = (Thickness)bindable.GetValue(MarginProperty);
			margin.Top = (double)newValue;
			bindable.SetValue(MarginProperty, margin);
		}

		internal static readonly BindableProperty MarginRightProperty =
			BindableProperty.Create("MarginRight", typeof(double), typeof(View), default(double),
									propertyChanged: OnMarginRightPropertyChanged);

		static void OnMarginRightPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var margin = (Thickness)bindable.GetValue(MarginProperty);
			margin.Right = (double)newValue;
			bindable.SetValue(MarginProperty, margin);
		}

		internal static readonly BindableProperty MarginBottomProperty =
			BindableProperty.Create("MarginBottom", typeof(double), typeof(View), default(double),
									propertyChanged: OnMarginBottomPropertyChanged);


		static void OnMarginBottomPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var margin = (Thickness)bindable.GetValue(MarginProperty);
			margin.Bottom = (double)newValue;
			bindable.SetValue(MarginProperty, margin);
		}

		readonly ObservableCollection<IGestureRecognizer> _gestureRecognizers = new ObservableCollection<IGestureRecognizer>();

		protected internal View()
		{
			_gestureRecognizers.CollectionChanged += (sender, args) =>
			{
				void AddItems(IEnumerable<IElement> elements)
				{
					foreach (IElement item in elements)
					{
						ValidateGesture(item as IGestureRecognizer);
						item.Parent = this;
						GestureController.CompositeGestureRecognizers.Add(item as IGestureRecognizer);
					}
				}

				void RemoveItems(IEnumerable<IElement> elements)
				{
					foreach (IElement item in elements)
					{
						item.Parent = null;
						GestureController.CompositeGestureRecognizers.Remove(item as IGestureRecognizer);
					}
				}

				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
						AddItems(args.NewItems.OfType<IElement>());
						break;
					case NotifyCollectionChangedAction.Remove:
						RemoveItems(args.OldItems.OfType<IElement>());
						break;
					case NotifyCollectionChangedAction.Replace:
						AddItems(args.NewItems.OfType<IElement>());
						RemoveItems(args.OldItems.OfType<IElement>());
						break;
					case NotifyCollectionChangedAction.Reset:

						List<IElement> remove = new List<IElement>();
						List<IElement> add = new List<IElement>();

						foreach (IElement item in _gestureRecognizers.OfType<IElement>())
						{
							if (!_gestureRecognizers.Contains((IGestureRecognizer)item))
								add.Add(item);
							item.Parent = this;
						}

						foreach (IElement item in GestureController.CompositeGestureRecognizers.OfType<IElement>())
						{
							if (_gestureRecognizers.Contains((IGestureRecognizer)item))
								item.Parent = this;
							else
								remove.Add(item);
						}

						AddItems(add);
						RemoveItems(remove);

						break;
				}
			};
		}

		public IList<IGestureRecognizer> GestureRecognizers
		{
			get { return _gestureRecognizers; }
		}

		ObservableCollection<IGestureRecognizer> _compositeGestureRecognizers;

		IList<IGestureRecognizer> IGestureController.CompositeGestureRecognizers
		{
			get { return _compositeGestureRecognizers ?? (_compositeGestureRecognizers = new ObservableCollection<IGestureRecognizer>()); }
		}

		public virtual IList<GestureElement> GetChildElements(Point point)
		{
			return null;
		}

		public LayoutOptions HorizontalOptions
		{
			get { return (LayoutOptions)GetValue(HorizontalOptionsProperty); }
			set { SetValue(HorizontalOptionsProperty, value); }
		}

		public Thickness Margin
		{
			get { return (Thickness)GetValue(MarginProperty); }
			set { SetValue(MarginProperty, value); }
		}

		public LayoutOptions VerticalOptions
		{
			get { return (LayoutOptions)GetValue(VerticalOptionsProperty); }
			set { SetValue(VerticalOptionsProperty, value); }
		}

		protected override void OnBindingContextChanged()
		{
			this.PropagateBindingContext(GestureRecognizers);
			base.OnBindingContextChanged();
		}

		static void MarginPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((View)bindable).InvalidateMeasureInternal(InvalidationTrigger.MarginChanged);
		}

		void ValidateGesture(IGestureRecognizer gesture)
		{
			if (gesture == null)
				return;
			if (gesture is PinchGestureRecognizer && _gestureRecognizers.GetGesturesFor<PinchGestureRecognizer>().Count() > 1)
				throw new InvalidOperationException($"Only one {nameof(PinchGestureRecognizer)} per view is allowed");
		}

        Thickness IView.Margin => Margin;

		partial void HandlerChangedPartial();
		GestureManager? _gestureManager;
		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();
			_gestureManager?.Dispose();

			if (Handler != null)
				_gestureManager = new GestureManager(Handler);

			HandlerChangedPartial();
		}

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			_gestureManager?.Dispose();
			_gestureManager = null;

			base.OnHandlerChangingCore(args);
		}

		protected PropertyMapper propertyMapper;

		protected PropertyMapper<T> GetRendererOverrides<T>() where T : IView =>
			(PropertyMapper<T>)(propertyMapper as PropertyMapper<T> ?? (propertyMapper = new PropertyMapper<T>()));

		PropertyMapper IPropertyMapperView.GetPropertyMapperOverrides() => propertyMapper;

		Primitives.LayoutAlignment IView.HorizontalLayoutAlignment => HorizontalOptions.ToCore();
		Primitives.LayoutAlignment IView.VerticalLayoutAlignment => VerticalOptions.ToCore();

		#region HotReload

		IView IReplaceableView.ReplacedView =>
			MauiHotReloadHelper.GetReplacedView(this) ?? this;

		IReloadHandler IHotReloadableView.ReloadHandler { get; set; }

		void IHotReloadableView.TransferState(IView newView)
		{
			//TODO: LEt you hot reload the the ViewModel
			if (newView is View v)
				v.BindingContext = BindingContext;
		}

		void IHotReloadableView.Reload()
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				this.CheckHandlers();
				//Handler = null;
				var reloadHandler = ((IHotReloadableView)this).ReloadHandler;
				reloadHandler?.Reload();
				//TODO: if reload handler is null, Do a manual reload?
			});
		}

		#endregion
	}
}