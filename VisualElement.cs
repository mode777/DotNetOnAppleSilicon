using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
//using Microsoft.Maui.Controls.Shapes;
using Geometry = Microsoft.Maui.Controls.Shapes.Geometry;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement : NavigableElement, IAnimatable, IVisualElementController, IResourcesProvider, IStyleElement, IFlowDirectionController, IPropertyPropagationController, IVisualController
	{
		public new static readonly BindableProperty NavigationProperty = NavigableElement.NavigationProperty;

		public new static readonly BindableProperty StyleProperty = NavigableElement.StyleProperty;

		public static readonly BindableProperty InputTransparentProperty = BindableProperty.Create("InputTransparent", typeof(bool), typeof(VisualElement), default(bool));

		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create("IsEnabled", typeof(bool),
			typeof(VisualElement), true, propertyChanged: OnIsEnabledPropertyChanged);

		static readonly BindablePropertyKey XPropertyKey = BindableProperty.CreateReadOnly("X", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty XProperty = XPropertyKey.BindableProperty;

		static readonly BindablePropertyKey YPropertyKey = BindableProperty.CreateReadOnly("Y", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty YProperty = YPropertyKey.BindableProperty;

		public static readonly BindableProperty AnchorXProperty = BindableProperty.Create("AnchorX", typeof(double), typeof(VisualElement), .5d);

		public static readonly BindableProperty AnchorYProperty = BindableProperty.Create("AnchorY", typeof(double), typeof(VisualElement), .5d);

		public static readonly BindableProperty TranslationXProperty = BindableProperty.Create("TranslationX", typeof(double), typeof(VisualElement), 0d);

		public static readonly BindableProperty TranslationYProperty = BindableProperty.Create("TranslationY", typeof(double), typeof(VisualElement), 0d);

		static readonly BindablePropertyKey WidthPropertyKey = BindableProperty.CreateReadOnly("Width", typeof(double), typeof(VisualElement), -1d,
			coerceValue: (bindable, value) => double.IsNaN((double)value) ? 0d : value);

		public static readonly BindableProperty WidthProperty = WidthPropertyKey.BindableProperty;

		static readonly BindablePropertyKey HeightPropertyKey = BindableProperty.CreateReadOnly("Height", typeof(double), typeof(VisualElement), -1d,
			coerceValue: (bindable, value) => double.IsNaN((double)value) ? 0d : value);

		public static readonly BindableProperty HeightProperty = HeightPropertyKey.BindableProperty;

		public static readonly BindableProperty RotationProperty = BindableProperty.Create("Rotation", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty RotationXProperty = BindableProperty.Create("RotationX", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty RotationYProperty = BindableProperty.Create("RotationY", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty ScaleProperty = BindableProperty.Create(nameof(Scale), typeof(double), typeof(VisualElement), 1d);

		public static readonly BindableProperty ScaleXProperty = BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(VisualElement), 1d);

		public static readonly BindableProperty ScaleYProperty = BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(VisualElement), 1d);

		internal static readonly BindableProperty TransformProperty = BindableProperty.Create("Transform", typeof(string), typeof(VisualElement), null, propertyChanged: OnTransformChanged);

		public static readonly BindableProperty ClipProperty = BindableProperty.Create(nameof(Clip), typeof(Geometry), typeof(VisualElement), null,
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				if (oldvalue != null)
					(bindable as VisualElement)?.StopNotifyingClipChanges();
			},
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (newvalue != null)
					(bindable as VisualElement)?.NotifyClipChanges();
			});

		void NotifyClipChanges()
		{
			if (Clip != null)
			{
				Clip.PropertyChanged += OnClipChanged;

				if (Clip is GeometryGroup geometryGroup)
					geometryGroup.InvalidateGeometryRequested += InvalidateGeometryRequested;
			}
		}

		void StopNotifyingClipChanges()
		{
			if (Clip != null)
			{
				Clip.PropertyChanged -= OnClipChanged;

				if (Clip is GeometryGroup geometryGroup)
					geometryGroup.InvalidateGeometryRequested -= InvalidateGeometryRequested;
			}
		}

		void OnClipChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Clip));
		}

		void InvalidateGeometryRequested(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(Clip));
		}

		public static readonly BindableProperty VisualProperty =
			BindableProperty.Create(nameof(Visual), typeof(IVisual), typeof(VisualElement), Maui.Controls.VisualMarker.MatchParent,
									validateValue: (b, v) => v != null, propertyChanged: OnVisualChanged);

		static IVisual _defaultVisual = Microsoft.Maui.Controls.VisualMarker.Default;
		IVisual _effectiveVisual = _defaultVisual;

		[System.ComponentModel.TypeConverter(typeof(VisualTypeConverter))]
		public IVisual Visual
		{
			get { return (IVisual)GetValue(VisualProperty); }
			set { SetValue(VisualProperty, value); }
		}

		internal static void SetDefaultVisual(IVisual visual) => _defaultVisual = visual;

		IVisual IVisualController.EffectiveVisual
		{
			get { return _effectiveVisual; }
			set
			{
				if (value == _effectiveVisual)
					return;

				_effectiveVisual = value;
				OnPropertyChanged(VisualProperty.PropertyName);
			}
		}

		static void OnTransformChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if ((string)newValue == "none")
			{
				bindable.ClearValue(TranslationXProperty);
				bindable.ClearValue(TranslationYProperty);
				bindable.ClearValue(RotationProperty);
				bindable.ClearValue(RotationXProperty);
				bindable.ClearValue(RotationYProperty);
				bindable.ClearValue(ScaleProperty);
				bindable.ClearValue(ScaleXProperty);
				bindable.ClearValue(ScaleYProperty);
				return;
			}
			var transforms = ((string)newValue).Split(' ');
			foreach (var transform in transforms)
			{
				if (string.IsNullOrEmpty(transform) || transform.IndexOf('(') < 0 || transform.IndexOf(')') < 0)
					throw new FormatException("Format for transform is 'none | transform(value) [transform(value) ]*'");
				var transformName = transform.Substring(0, transform.IndexOf('('));
				var value = transform.Substring(transform.IndexOf('(') + 1, transform.IndexOf(')') - transform.IndexOf('(') - 1);
				double translationX, translationY, scaleX, scaleY, rotateX, rotateY, rotate;
				if (transformName.StartsWith("translateX", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out translationX))
					bindable.SetValue(TranslationXProperty, translationX);
				else if (transformName.StartsWith("translateY", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out translationY))
					bindable.SetValue(TranslationYProperty, translationY);
				else if (transformName.StartsWith("translate", StringComparison.OrdinalIgnoreCase))
				{
					var translate = value.Split(',');
					if (double.TryParse(translate[0], out translationX) && double.TryParse(translate[1], out translationY))
					{
						bindable.SetValue(TranslationXProperty, translationX);
						bindable.SetValue(TranslationYProperty, translationY);
					}
				}
				else if (transformName.StartsWith("scaleX", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out scaleX))
					bindable.SetValue(ScaleXProperty, scaleX);
				else if (transformName.StartsWith("scaleY", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out scaleY))
					bindable.SetValue(ScaleYProperty, scaleY);
				else if (transformName.StartsWith("scale", StringComparison.OrdinalIgnoreCase))
				{
					var scale = value.Split(',');
					if (double.TryParse(scale[0], out scaleX) && double.TryParse(scale[1], out scaleY))
					{
						bindable.SetValue(ScaleXProperty, scaleX);
						bindable.SetValue(ScaleYProperty, scaleY);
					}
				}
				else if (transformName.StartsWith("rotateX", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out rotateX))
					bindable.SetValue(RotationXProperty, rotateX);
				else if (transformName.StartsWith("rotateY", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out rotateY))
					bindable.SetValue(RotationYProperty, rotateY);
				else if (transformName.StartsWith("rotate", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out rotate))
					bindable.SetValue(RotationProperty, rotate);
				else
					throw new FormatException("Invalid transform name");
			}
		}

		internal static readonly BindableProperty TransformOriginProperty =
			BindableProperty.Create("TransformOrigin", typeof(Point), typeof(VisualElement), new Point(.5d, .5d),
									propertyChanged: (b, o, n) => { (((VisualElement)b).AnchorX, ((VisualElement)b).AnchorY) = (Point)n; });

		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create("IsVisible", typeof(bool), typeof(VisualElement), true,
			propertyChanged: (bindable, oldvalue, newvalue) => ((VisualElement)bindable).OnIsVisibleChanged((bool)oldvalue, (bool)newvalue));

		public static readonly BindableProperty OpacityProperty = BindableProperty.Create("Opacity", typeof(double), typeof(VisualElement), 1d, coerceValue: (bindable, value) => ((double)value).Clamp(0, 1));

		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(VisualElement), null);

		public static readonly BindableProperty BackgroundProperty = BindableProperty.Create(nameof(Background), typeof(Brush), typeof(VisualElement), Brush.Default,
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				if (oldvalue != null)
					(bindable as VisualElement)?.StopNotifyingBackgroundChanges();
			},
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (newvalue != null)
					(bindable as VisualElement)?.NotifyBackgroundChanges();
			});

		void NotifyBackgroundChanges()
		{
			if (Background != null)
			{
				Background.Parent = this;
				Background.PropertyChanged += OnBackgroundChanged;

				if (Background is GradientBrush gradientBrush)
					gradientBrush.InvalidateGradientBrushRequested += InvalidateGradientBrushRequested;
			}
		}

		void StopNotifyingBackgroundChanges()
		{
			if (Background != null)
			{
				Background.Parent = null;
				Background.PropertyChanged -= OnBackgroundChanged;

				if (Background is GradientBrush gradientBrush)
					gradientBrush.InvalidateGradientBrushRequested -= InvalidateGradientBrushRequested;
			}
		}

		void OnBackgroundChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Background));
		}

		void InvalidateGradientBrushRequested(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(Background));
		}

		internal static readonly BindablePropertyKey BehaviorsPropertyKey = BindableProperty.CreateReadOnly("Behaviors", typeof(IList<Behavior>), typeof(VisualElement), default(IList<Behavior>),
			defaultValueCreator: bindable =>
			{
				var collection = new AttachedCollection<Behavior>();
				collection.AttachTo(bindable);
				return collection;
			});

		public static readonly BindableProperty BehaviorsProperty = BehaviorsPropertyKey.BindableProperty;

		internal static readonly BindablePropertyKey TriggersPropertyKey = BindableProperty.CreateReadOnly("Triggers", typeof(IList<TriggerBase>), typeof(VisualElement), default(IList<TriggerBase>),
			defaultValueCreator: bindable =>
			{
				var collection = new AttachedCollection<TriggerBase>();
				collection.AttachTo(bindable);
				return collection;
			});

		public static readonly BindableProperty TriggersProperty = TriggersPropertyKey.BindableProperty;


		public static readonly BindableProperty WidthRequestProperty = BindableProperty.Create(nameof(WidthRequest), typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty HeightRequestProperty = BindableProperty.Create(nameof(HeightRequest), typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty MinimumWidthRequestProperty = BindableProperty.Create(nameof(MinimumWidthRequest), typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty MinimumHeightRequestProperty = BindableProperty.Create(nameof(MinimumHeightRequest), typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty MaximumWidthRequestProperty = BindableProperty.Create(nameof(MaximumWidthRequest), typeof(double), typeof(VisualElement), double.PositiveInfinity, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty MaximumHeightRequestProperty = BindableProperty.Create(nameof(MaximumHeightRequest), typeof(double), typeof(VisualElement), double.PositiveInfinity, propertyChanged: OnRequestChanged);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindablePropertyKey IsFocusedPropertyKey = BindableProperty.CreateReadOnly("IsFocused",
			typeof(bool), typeof(VisualElement), default(bool), propertyChanged: OnIsFocusedPropertyChanged);

		public static readonly BindableProperty IsFocusedProperty = IsFocusedPropertyKey.BindableProperty;

		public static readonly BindableProperty FlowDirectionProperty = BindableProperty.Create(nameof(FlowDirection), typeof(FlowDirection), typeof(VisualElement), FlowDirection.MatchParent, propertyChanging: FlowDirectionChanging, propertyChanged: FlowDirectionChanged);

		IFlowDirectionController FlowController => this;

		[System.ComponentModel.TypeConverter(typeof(FlowDirectionConverter))]
		public FlowDirection FlowDirection
		{
			get { return (FlowDirection)GetValue(FlowDirectionProperty); }
			set { SetValue(FlowDirectionProperty, value); }
		}

		EffectiveFlowDirection _effectiveFlowDirection = default(EffectiveFlowDirection);
		EffectiveFlowDirection IFlowDirectionController.EffectiveFlowDirection
		{
			get => _effectiveFlowDirection;
			set => SetEffectiveFlowDirection(value, true);
		}

		void SetEffectiveFlowDirection(EffectiveFlowDirection value, bool fireFlowDirectionPropertyChanged)
		{
			if (value == _effectiveFlowDirection)
				return;

			_effectiveFlowDirection = value;
			InvalidateMeasureInternal(InvalidationTrigger.Undefined);

			if (fireFlowDirectionPropertyChanged)
				OnPropertyChanged(FlowDirectionProperty.PropertyName);

		}

		EffectiveFlowDirection IVisualElementController.EffectiveFlowDirection => FlowController.EffectiveFlowDirection;

		readonly Dictionary<Size, SizeRequest> _measureCache = new Dictionary<Size, SizeRequest>();



		int _batched;
		LayoutConstraint _computedConstraint;

		bool _isInNativeLayout;

		bool _isNativeStateConsistent = true;

		bool _isPlatformEnabled;

		double _mockHeight = -1;

		double _mockWidth = -1;

		double _mockX = -1;

		double _mockY = -1;

		LayoutConstraint _selfConstraint;

		protected internal VisualElement()
		{
		}

		public double AnchorX
		{
			get { return (double)GetValue(AnchorXProperty); }
			set { SetValue(AnchorXProperty, value); }
		}

		public double AnchorY
		{
			get { return (double)GetValue(AnchorYProperty); }
			set { SetValue(AnchorYProperty, value); }
		}

		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		[System.ComponentModel.TypeConverter(typeof(BrushTypeConverter))]
		public Brush Background
		{
			get { return (Brush)GetValue(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}

		public IList<Behavior> Behaviors
		{
			get { return (IList<Behavior>)GetValue(BehaviorsProperty); }
		}

		public Rectangle Bounds
		{
			get { return new Rectangle(X, Y, Width, Height); }
			private set
			{
				if (value.X == X && value.Y == Y && value.Height == Height && value.Width == Width)
					return;
				BatchBegin();
				X = value.X;
				Y = value.Y;
				SetSize(value.Width, value.Height);
				BatchCommit();
			}
		}

		public double Height
		{
			get { return _mockHeight == -1 ? (double)GetValue(HeightProperty) : _mockHeight; }
			private set { SetValue(HeightPropertyKey, value); }
		}

		public double HeightRequest
		{
			get { return (double)GetValue(HeightRequestProperty); }
			set { SetValue(HeightRequestProperty, value); }
		}

		public bool InputTransparent
		{
			get { return (bool)GetValue(InputTransparentProperty); }
			set { SetValue(InputTransparentProperty, value); }
		}

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public bool IsFocused => (bool)GetValue(IsFocusedProperty);

		[System.ComponentModel.TypeConverter(typeof(VisibilityConverter))]
		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		public double MinimumHeightRequest
		{
			get { return (double)GetValue(MinimumHeightRequestProperty); }
			set { SetValue(MinimumHeightRequestProperty, value); }
		}

		public double MinimumWidthRequest
		{
			get { return (double)GetValue(MinimumWidthRequestProperty); }
			set { SetValue(MinimumWidthRequestProperty, value); }
		}

		public double MaximumHeightRequest
		{
			get { return (double)GetValue(MaximumHeightRequestProperty); }
			set { SetValue(MaximumHeightRequestProperty, value); }
		}

		public double MaximumWidthRequest
		{
			get { return (double)GetValue(MaximumWidthRequestProperty); }
			set { SetValue(MaximumWidthRequestProperty, value); }
		}

		public double Opacity
		{
			get { return (double)GetValue(OpacityProperty); }
			set { SetValue(OpacityProperty, value); }
		}

		public double Rotation
		{
			get { return (double)GetValue(RotationProperty); }
			set { SetValue(RotationProperty, value); }
		}

		public double RotationX
		{
			get { return (double)GetValue(RotationXProperty); }
			set { SetValue(RotationXProperty, value); }
		}

		public double RotationY
		{
			get { return (double)GetValue(RotationYProperty); }
			set { SetValue(RotationYProperty, value); }
		}

		public double Scale
		{
			get => (double)GetValue(ScaleProperty);
			set => SetValue(ScaleProperty, value);
		}

		public double ScaleX
		{
			get => (double)GetValue(ScaleXProperty);
			set => SetValue(ScaleXProperty, value);
		}

		public double ScaleY
		{
			get => (double)GetValue(ScaleYProperty);
			set => SetValue(ScaleYProperty, value);
		}

		public double TranslationX
		{
			get { return (double)GetValue(TranslationXProperty); }
			set { SetValue(TranslationXProperty, value); }
		}

		public double TranslationY
		{
			get { return (double)GetValue(TranslationYProperty); }
			set { SetValue(TranslationYProperty, value); }
		}

		public IList<TriggerBase> Triggers => (IList<TriggerBase>)GetValue(TriggersProperty);

		public double Width
		{
			get { return _mockWidth == -1 ? (double)GetValue(WidthProperty) : _mockWidth; }
			private set { SetValue(WidthPropertyKey, value); }
		}

		public double WidthRequest
		{
			get { return (double)GetValue(WidthRequestProperty); }
			set { SetValue(WidthRequestProperty, value); }
		}

		public double X
		{
			get { return _mockX == -1 ? (double)GetValue(XProperty) : _mockX; }
			private set { SetValue(XPropertyKey, value); }
		}

		public double Y
		{
			get { return _mockY == -1 ? (double)GetValue(YProperty) : _mockY; }
			private set { SetValue(YPropertyKey, value); }
		}

		[System.ComponentModel.TypeConverter(typeof(PathGeometryConverter))]
		public Geometry Clip
		{
			get { return (Geometry)GetValue(ClipProperty); }
			set { SetValue(ClipProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool Batched => _batched > 0;

		internal LayoutConstraint ComputedConstraint
		{
			get { return _computedConstraint; }
			set
			{
				if (_computedConstraint == value)
					return;

				LayoutConstraint oldConstraint = Constraint;
				_computedConstraint = value;
				LayoutConstraint newConstraint = Constraint;
				if (oldConstraint != newConstraint)
					OnConstraintChanged(oldConstraint, newConstraint);
			}
		}

		internal LayoutConstraint Constraint => ComputedConstraint | SelfConstraint;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool DisableLayout { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsInNativeLayout
		{
			get
			{
				if (_isInNativeLayout)
					return true;

				Element parent = RealParent;
				if (parent != null)
				{
					var visualElement = parent as VisualElement;
					if (visualElement != null && visualElement.IsInNativeLayout)
						return true;
				}

				return false;
			}
			set { _isInNativeLayout = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsNativeStateConsistent
		{
			get { return _isNativeStateConsistent; }
			set
			{
				if (_isNativeStateConsistent == value)
					return;
				_isNativeStateConsistent = value;
				if (value && IsPlatformEnabled)
					InvalidateMeasureInternal(InvalidationTrigger.RendererReady);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal event EventHandler PlatformEnabledChanged;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsPlatformEnabled
		{
			get { return _isPlatformEnabled; }
			set
			{
				if (value == _isPlatformEnabled)
					return;

				_isPlatformEnabled = value;
				if (value && IsNativeStateConsistent)
					InvalidateMeasureInternal(InvalidationTrigger.RendererReady);

				InvalidateStateTriggers(IsPlatformEnabled);

				OnIsPlatformEnabledChanged();
				PlatformEnabledChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		internal LayoutConstraint SelfConstraint
		{
			get { return _selfConstraint; }
			set
			{
				if (_selfConstraint == value)
					return;

				LayoutConstraint oldConstraint = Constraint;
				_selfConstraint = value;
				LayoutConstraint newConstraint = Constraint;
				if (oldConstraint != newConstraint)
				{
					OnConstraintChanged(oldConstraint, newConstraint);
				}
			}
		}

		public void BatchBegin() => _batched++;

		public void BatchCommit()
		{
			_batched = Math.Max(0, _batched - 1);
			if (!Batched)
			{
				BatchCommitted?.Invoke(this, new EventArg<VisualElement>(this));
				Device.Invalidate(this);
			}
		}

		ResourceDictionary _resources;
		bool IResourcesProvider.IsResourcesCreated => _resources != null;

		public ResourceDictionary Resources
		{
			get
			{
				if (_resources != null)
					return _resources;
				_resources = new ResourceDictionary();
				((IResourceDictionary)_resources).ValuesChanged += OnResourcesChanged;
				return _resources;
			}
			set
			{
				if (_resources == value)
					return;
				OnPropertyChanging();
				if (_resources != null)
					((IResourceDictionary)_resources).ValuesChanged -= OnResourcesChanged;
				_resources = value;
				OnResourcesChanged(value);
				if (_resources != null)
					((IResourceDictionary)_resources).ValuesChanged += OnResourcesChanged;
				OnPropertyChanged();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void NativeSizeChanged() => InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		public event EventHandler ChildrenReordered;

		public bool Focus()
		{
			if (IsFocused)
				return true;

			if (FocusChangeRequested == null)
				return false;

			var arg = new FocusRequestArgs { Focus = true };
			FocusChangeRequested(this, arg);
			return arg.Result;
		}

		public event EventHandler<FocusEventArgs> Focused;

		SizeRequest GetSizeRequest(double widthConstraint, double heightConstraint)
		{
			var constraintSize = new Size(widthConstraint, heightConstraint);
			if (_measureCache.TryGetValue(constraintSize, out SizeRequest cachedResult))
				return cachedResult;

			double widthRequest = WidthRequest;
			double heightRequest = HeightRequest;
			if (widthRequest >= 0)
				widthConstraint = Math.Min(widthConstraint, widthRequest);
			if (heightRequest >= 0)
				heightConstraint = Math.Min(heightConstraint, heightRequest);

			SizeRequest result = OnMeasure(widthConstraint, heightConstraint);
			bool hasMinimum = result.Minimum != result.Request;
			Size request = result.Request;
			Size minimum = result.Minimum;

			if (heightRequest != -1)
			{
				request.Height = heightRequest;
				if (!hasMinimum)
					minimum.Height = heightRequest;
			}

			if (widthRequest != -1)
			{
				request.Width = widthRequest;
				if (!hasMinimum)
					minimum.Width = widthRequest;
			}

			double minimumHeightRequest = MinimumHeightRequest;
			double minimumWidthRequest = MinimumWidthRequest;

			if (minimumHeightRequest != -1)
				minimum.Height = minimumHeightRequest;
			if (minimumWidthRequest != -1)
				minimum.Width = minimumWidthRequest;

			minimum.Height = Math.Min(request.Height, minimum.Height);
			minimum.Width = Math.Min(request.Width, minimum.Width);

			var r = new SizeRequest(request, minimum);

			if (r.Request.Width > 0 && r.Request.Height > 0)
				_measureCache[constraintSize] = r;

			return r;
		}

		public virtual SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
		{
			bool includeMargins = (flags & MeasureFlags.IncludeMargins) != 0;
			Thickness margin = default(Thickness);
			if (includeMargins)
			{
				if (this is View view)
					margin = view.Margin;
				widthConstraint = Math.Max(0, widthConstraint - margin.HorizontalThickness);
				heightConstraint = Math.Max(0, heightConstraint - margin.VerticalThickness);
			}

			SizeRequest result = GetSizeRequest(widthConstraint, heightConstraint);

			if (includeMargins && !margin.IsEmpty)
			{
				result.Minimum = new Size(result.Minimum.Width + margin.HorizontalThickness, result.Minimum.Height + margin.VerticalThickness);
				result.Request = new Size(result.Request.Width + margin.HorizontalThickness, result.Request.Height + margin.VerticalThickness);
			}

			DesiredSize = result.Request;
			return result;
		}

		public event EventHandler MeasureInvalidated;

		public event EventHandler SizeChanged;

		public void Unfocus()
		{
			if (!IsFocused)
				return;

			FocusChangeRequested?.Invoke(this, new FocusRequestArgs());
		}

		public event EventHandler<FocusEventArgs> Unfocused;

		protected virtual void InvalidateMeasure() => InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		protected override void OnBindingContextChanged()
		{
			PropagateBindingContextToStateTriggers();

			PropagateBindingContextToShadow();

			base.OnBindingContextChanged();
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			var view = child as View;
			if (view != null)
				ComputeConstraintForView(view);
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);
			if (child is View view)
				view.ComputedConstraint = LayoutConstraint.None;
		}

		protected void OnChildrenReordered()
			=> ChildrenReordered?.Invoke(this, EventArgs.Empty);

		protected virtual SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{

			if (!IsPlatformEnabled)
				return new SizeRequest(new Size(-1, -1));

			return Device.PlatformServices.GetNativeSize(this, widthConstraint, heightConstraint);
		}

		protected virtual void OnSizeAllocated(double width, double height)
		{
		}

		protected void SizeAllocated(double width, double height) => OnSizeAllocated(width, height);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<EventArg<VisualElement>> BatchCommitted;

		internal void ComputeConstrainsForChildren()
		{
			for (var i = 0; i < LogicalChildrenInternal.Count; i++)
			{
				if (LogicalChildrenInternal[i] is View child)
					ComputeConstraintForView(child);
			}
		}

		internal virtual void ComputeConstraintForView(View view) => view.ComputedConstraint = LayoutConstraint.None;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<FocusRequestArgs> FocusChangeRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void InvalidateMeasureNonVirtual(InvalidationTrigger trigger)
		{
			InvalidateMeasureInternal(trigger);
		}

		internal virtual void InvalidateMeasureInternal(InvalidationTrigger trigger)
		{
			_measureCache.Clear();
			(this as IView)?.InvalidateMeasure();
			MeasureInvalidated?.Invoke(this, new InvalidationEventArgs(trigger));
		}

		void IVisualElementController.InvalidateMeasure(InvalidationTrigger trigger) => InvalidateMeasureInternal(trigger);

		internal void InvalidateStateTriggers(bool attach)
		{
			if (!this.HasVisualStateGroups())
				return;

			var groups = (IList<VisualStateGroup>)GetValue(VisualStateManager.VisualStateGroupsProperty);

			if (groups.Count == 0)
				return;

			foreach (var group in groups)
				foreach (var state in group.States)
					foreach (var stateTrigger in state.StateTriggers)
					{
						if (attach)
							stateTrigger.SendAttached();
						else
							stateTrigger.SendDetached();
					}
		}

		internal void MockBounds(Rectangle bounds)
		{
#if NETSTANDARD2_0 || NET6_0
			(_mockX, _mockY, _mockWidth, _mockHeight) = bounds;
#else
			_mockX = bounds.X;
			_mockY = bounds.Y;
			_mockWidth = bounds.Width;
			_mockHeight = bounds.Height;
#endif
		}

		internal virtual void OnConstraintChanged(LayoutConstraint oldConstraint, LayoutConstraint newConstraint) => ComputeConstrainsForChildren();

		internal virtual void OnIsPlatformEnabledChanged()
		{
		}

		internal virtual void OnIsVisibleChanged(bool oldValue, bool newValue)
		{
			if (this is IView fe)
			{
				fe.Handler?.UpdateValue(nameof(IView.Visibility));
			}

			InvalidateMeasureInternal(InvalidationTrigger.Undefined);
		}

		internal override void OnParentResourcesChanged(IEnumerable<KeyValuePair<string, object>> values)
		{
			if (values == null)
				return;

			if (!((IResourcesProvider)this).IsResourcesCreated || Resources.Count == 0)
			{
				base.OnParentResourcesChanged(values);
				return;
			}

			var innerKeys = new HashSet<string>();
			var changedResources = new List<KeyValuePair<string, object>>();
			foreach (KeyValuePair<string, object> c in Resources)
				innerKeys.Add(c.Key);
			foreach (KeyValuePair<string, object> value in values)
			{
				if (innerKeys.Add(value.Key))
					changedResources.Add(value);
				else if (value.Key.StartsWith(Style.StyleClassPrefix, StringComparison.Ordinal))
				{
					var mergedClassStyles = new List<Style>(Resources[value.Key] as List<Style>);
					mergedClassStyles.AddRange(value.Value as List<Style>);
					changedResources.Add(new KeyValuePair<string, object>(value.Key, mergedClassStyles));
				}
			}
			if (changedResources.Count != 0)
				OnResourcesChanged(changedResources);
		}

		internal void UnmockBounds() => _mockX = _mockY = _mockWidth = _mockHeight = -1;

		void PropagateBindingContextToStateTriggers()
		{
			var groups = (IList<VisualStateGroup>)GetValue(VisualStateManager.VisualStateGroupsProperty);

			if (groups.Count == 0)
				return;

			foreach (var group in groups)
				foreach (var state in group.States)
					foreach (var stateTrigger in state.StateTriggers)
						SetInheritedBindingContext(stateTrigger, BindingContext);
		}

		void OnFocused() => Focused?.Invoke(this, new FocusEventArgs(this, true));

		internal void ChangeVisualStateInternal() => ChangeVisualState();

		protected internal virtual void ChangeVisualState()
		{
			if (!IsEnabled)
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Disabled);
			else if (IsFocused)
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Focused);
			else
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
		}

		static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = bindable as IVisualController;
			var newVisual = (IVisual)newValue;

			if (newVisual.IsMatchParent())
				self.EffectiveVisual = Microsoft.Maui.Controls.VisualMarker.Default;
			else
				self.EffectiveVisual = (IVisual)newValue;

			(self as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.VisualProperty.PropertyName);
		}

		static void FlowDirectionChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var self = bindable as IFlowDirectionController;

			if (self.EffectiveFlowDirection.IsExplicit() && oldValue == newValue)
				return;

			var newFlowDirection = ((FlowDirection)newValue).ToEffectiveFlowDirection(isExplicit: true);

			if (self is VisualElement ve)
				ve.SetEffectiveFlowDirection(newFlowDirection, false);
			else
				self.EffectiveFlowDirection = newFlowDirection;
		}

		static void FlowDirectionChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.FlowDirectionProperty.PropertyName);
		}


		static void OnIsEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var element = (VisualElement)bindable;

			if (element == null)
				return;

			element.ChangeVisualState();
		}

		static void OnIsFocusedPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var element = (VisualElement)bindable;

			if (element == null)
			{
				return;
			}

			var isFocused = (bool)newvalue;
			if (isFocused)
			{
				element.OnFocused();
			}
			else
			{
				element.OnUnfocus();
			}

			element.ChangeVisualState();
		}

		static void OnRequestChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var constraint = LayoutConstraint.None;
			var element = (VisualElement)bindable;
			if (element.WidthRequest >= 0 && element.MinimumWidthRequest >= 0)
			{
				constraint |= LayoutConstraint.HorizontallyFixed;
			}
			if (element.HeightRequest >= 0 && element.MinimumHeightRequest >= 0)
			{
				constraint |= LayoutConstraint.VerticallyFixed;
			}

			element.SelfConstraint = constraint;

			if (element is IView fe)
			{
				fe.Handler?.UpdateValue(nameof(IView.Width));
				fe.Handler?.UpdateValue(nameof(IView.Height));
				fe.Handler?.UpdateValue(nameof(IView.MinimumHeight));
				fe.Handler?.UpdateValue(nameof(IView.MinimumWidth));
				fe.Handler?.UpdateValue(nameof(IView.MaximumHeight));
				fe.Handler?.UpdateValue(nameof(IView.MaximumWidth));
			}

			((VisualElement)bindable).InvalidateMeasureInternal(InvalidationTrigger.SizeRequestChanged);
		}

		void OnUnfocus() => Unfocused?.Invoke(this, new FocusEventArgs(this, false));

		bool IFlowDirectionController.ApplyEffectiveFlowDirectionToChildContainer => true;

		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, ((IElementController)this).LogicalChildren);
		}

		void SetSize(double width, double height)
		{
			if (Width == width && Height == height)
				return;

			Width = width;
			Height = height;

			SizeAllocated(width, height);
			SizeChanged?.Invoke(this, EventArgs.Empty);
		}

		public class FocusRequestArgs : EventArgs
		{
			public bool Focus { get; set; }

			public bool Result { get; set; }
		}

		public class VisibilityConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
				=> sourceType == typeof(string);

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
				=> true;

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				var strValue = value?.ToString()?.Trim();

				if (!string.IsNullOrEmpty(strValue))
				{
					if (strValue.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
						return true;
					if (strValue.Equals("visible", StringComparison.OrdinalIgnoreCase))
						return true;
					if (strValue.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
						return false;
					if (strValue.Equals("hidden", StringComparison.OrdinalIgnoreCase))
						return false;
					if (strValue.Equals("collapse", StringComparison.OrdinalIgnoreCase))
						return false;
				}
				throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}.", strValue, typeof(bool)));
			}

			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (value is not bool visibility)
					throw new NotSupportedException();
				return visibility.ToString();
			}
		}

        		Semantics _semantics;

		public Rectangle Frame
		{
			get => Bounds;
			set
			{
				X = value.X;
				Y = value.Y;
				Width = value.Width;
				Height = value.Height;
			}
		}

		new public IViewHandler Handler
		{
			get => base.Handler as IViewHandler;
			set => base.Handler = value;
		}

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			IsPlatformEnabled = Handler != null;
		}

		Paint IView.Background
		{
			get
			{
				if (!Brush.IsNullOrEmpty(Background))
					return Background;
				if (BackgroundColor.IsNotDefault())
					return new SolidColorBrush(BackgroundColor);
				return null;
			}
		}

		IShape IView.Clip => Clip;

		IShadow IView.Shadow => Shadow;

		public static readonly BindableProperty ShadowProperty =
 			BindableProperty.Create(nameof(Shadow), typeof(Shadow), typeof(VisualElement), defaultValue: null,
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue != null)
						(bindable as VisualElement)?.StopNotifyingShadowChanges();
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue != null)
						(bindable as VisualElement)?.NotifyShadowChanges();
				});

		public Shadow Shadow
		{
			get { return (Shadow)GetValue(ShadowProperty); }
			set { SetValue(ShadowProperty, value); }
		}

		public Size DesiredSize { get; protected set; }

		public void Arrange(Rectangle bounds)
		{
			Layout(bounds);
		}

		Size IView.Arrange(Rectangle bounds)
		{
			return ArrangeOverride(bounds);
		}

		// ArrangeOverride provides a way to allow subclasses (e.g., ScrollView) to override Arrange even though
		// the interface has to be explicitly implemented to avoid conflict with the old Arrange method
		protected virtual Size ArrangeOverride(Rectangle bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.NativeArrange(Frame);
			return Frame.Size;
		}

		public void Layout(Rectangle bounds)
		{
			Bounds = bounds;
		}

		void IView.InvalidateMeasure()
		{
			InvalidateMeasureOverride();
		}

		// InvalidateMeasureOverride provides a way to allow subclasses (e.g., Layout) to override InvalidateMeasure even though
		// the interface has to be explicitly implemented to avoid conflict with the VisualElement.InvalidateMeasure method
		protected virtual void InvalidateMeasureOverride() => Handler?.Invoke(nameof(IView.InvalidateMeasure));

		void IView.InvalidateArrange()
		{
		}

		Size IView.Measure(double widthConstraint, double heightConstraint)
		{
			return MeasureOverride(widthConstraint, heightConstraint);
		}

		// MeasureOverride provides a way to allow subclasses (e.g., Layout) to override Measure even though
		// the interface has to be explicitly implemented to avoid conflict with the old Measure method
		protected virtual Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			DesiredSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);
			return DesiredSize;
		}

		Maui.FlowDirection IView.FlowDirection
			=> ((IFlowDirectionController)this).EffectiveFlowDirection.ToFlowDirection();

		Primitives.LayoutAlignment IView.HorizontalLayoutAlignment => default;
		Primitives.LayoutAlignment IView.VerticalLayoutAlignment => default;

		Visibility IView.Visibility => IsVisible.ToVisibility();

		Semantics IView.Semantics
		{
			get => _semantics;
		}

		// We don't want to initialize Semantics until someone explicitly 
		// wants to modify some aspect of the semantics class
		internal Semantics SetupSemantics() =>
			_semantics ??= new Semantics();

		static void ValidatePositive(double value, string name)
		{
			if (value < 0)
			{
				throw new InvalidOperationException($"{name} cannot be less than zero.");
			}
		}

		double IView.Width
		{
			get
			{
				if (!IsSet(WidthRequestProperty))
				{
					return Primitives.Dimension.Unset;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = WidthRequest;
				ValidatePositive(value, nameof(IView.Width));
				return value;
			}
		}

		double IView.Height
		{
			get
			{
				if (!IsSet(HeightRequestProperty))
				{
					return Primitives.Dimension.Unset;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = HeightRequest;
				ValidatePositive(value, nameof(IView.Height));
				return value;
			}
		}

		double IView.MinimumWidth
		{
			get
			{
				if (!IsSet(MinimumWidthRequestProperty))
				{
					return Primitives.Dimension.Minimum;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = MinimumWidthRequest;
				ValidatePositive(value, nameof(IView.MinimumWidth));
				return value;
			}
		}

		double IView.MinimumHeight
		{
			get
			{
				if (!IsSet(MinimumHeightRequestProperty))
				{
					return Primitives.Dimension.Minimum;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = MinimumHeightRequest;
				ValidatePositive(value, nameof(IView.MinimumHeight));
				return value;
			}
		}

		double IView.MaximumWidth
		{
			get
			{
				// Access once up front to avoid multiple GetValue calls
				var value = MaximumWidthRequest;
				ValidatePositive(value, nameof(IView.MaximumWidth));
				return value;
			}
		}

		double IView.MaximumHeight
		{
			get
			{
				// Access once up front to avoid multiple GetValue calls
				var value = MaximumHeightRequest;
				ValidatePositive(value, nameof(IView.MaximumHeight));
				return value;
			}
		}

		Thickness IView.Margin => Thickness.Zero;

		void NotifyShadowChanges()
		{
			if (Shadow != null)
			{
				Shadow.Parent = this;
				Shadow.PropertyChanged += OnShadowChanged;
			}
		}

		void StopNotifyingShadowChanges()
		{
			if (Shadow != null)
			{
				Shadow.Parent = null;
				Shadow.PropertyChanged -= OnShadowChanged;

			}
		}

		void OnShadowChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Shadow));
		}

		void PropagateBindingContextToShadow()
		{
			if (Shadow != null)
				SetInheritedBindingContext(Shadow, BindingContext);
		}
	}
}
