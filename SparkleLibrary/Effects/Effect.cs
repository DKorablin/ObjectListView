/*
 * Effect - An effect changes a property of a Sprite over time
 * 
 * Author: Phillip Piper
 * Date: 23/10/2009 10:29 PM
 *
 * Change log:
 * 2010-03-18   JPP  - Added GenericEffect
 * 2010-03-01   JPP  - Added Repeater effect
 * 2010-02-02   JPP  - Added RectangleWalkEffect
 * 2009-10-23   JPP  - Initial version
 *
 * To do:
 *
 * Copyright (C) 2009 Phillip Piper
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 * If you wish to use this code in a closed source application, please contact phillip.piper@gmail.com.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Reflection;

namespace BrightIdeasSoftware
{
	/// <summary>An IEffect describes anything that can made a change to a sprite over time.</summary>
	public interface IEffect
	{
		/// <summary>Gets or set the sprite to which the effect will be applied</summary>
		ISprite Sprite { get; set; }

		/// <summary>Signal that this effect is about to applied to its sprite for the first time</summary>
		void Start();

		/// <summary>Apply this effect to the underlying sprite</summary>
		/// <param name="fractionDone">How far through the total effect are we? This will always in the range 0.0 .. 1.0.</param>
		void Apply(Single fractionDone);

		/// <summary>The effect has completed</summary>
		void Stop();

		/// <summary>Reset the effect AND the sprite to its condition before the effect was applied.</summary>
		void Reset();
	}

	/// <summary>An Effect provides a do-nothing implementation of the IEffect interface, plus providing a number of utility methods.</summary>
	public class Effect : IEffect
	{
		#region IEffect interface

		public ISprite Sprite { get; set; }

		public virtual void Start() { }
		public virtual void Apply(Single fractionDone) { }
		public virtual void Stop() { }
		public virtual void Reset() { }

		#endregion

		#region Calculations

		protected Rectangle CalculateIntermediary(Rectangle from, Rectangle to, Single fractionDone)
		{
			if(fractionDone >= 1.0f)
				return to;

			if(fractionDone <= 0.0f)
				return from;

			return new Rectangle(
				this.CalculateIntermediary(from.X, to.X, fractionDone),
				this.CalculateIntermediary(from.Y, to.Y, fractionDone),
				this.CalculateIntermediary(from.Width, to.Width, fractionDone),
				this.CalculateIntermediary(from.Height, to.Height, fractionDone));
		}

		protected RectangleF CalculateIntermediary(RectangleF from, RectangleF to, Single fractionDone)
		{
			if(fractionDone >= 1.0f)
				return to;

			if(fractionDone <= 0.0f)
				return from;

			return new RectangleF(
				this.CalculateIntermediary(from.X, to.X, fractionDone),
				this.CalculateIntermediary(from.Y, to.Y, fractionDone),
				this.CalculateIntermediary(from.Width, to.Width, fractionDone),
				this.CalculateIntermediary(from.Height, to.Height, fractionDone));
		}

		protected Point CalculateIntermediary(Point from, Point to, Single fractionDone)
		{
			return new Point(
				this.CalculateIntermediary(from.X, to.X, fractionDone),
				this.CalculateIntermediary(from.Y, to.Y, fractionDone));
		}

		protected PointF CalculateIntermediary(PointF from, PointF to, Single fractionDone)
		{
			return new PointF(
				this.CalculateIntermediary(from.X, to.X, fractionDone),
				this.CalculateIntermediary(from.Y, to.Y, fractionDone));
		}

		protected Size CalculateIntermediary(Size from, Size to, Single fractionDone)
		{
			return new Size(
				this.CalculateIntermediary(from.Width, to.Width, fractionDone),
				this.CalculateIntermediary(from.Height, to.Height, fractionDone));
		}

		protected SizeF CalculateIntermediary(SizeF from, SizeF to, Single fractionDone)
		{
			return new SizeF(
				this.CalculateIntermediary(from.Width, to.Width, fractionDone),
				this.CalculateIntermediary(from.Height, to.Height, fractionDone));
		}

		protected Char CalculateIntermediary(Char from, Char to, Single fractionDone)
		{
			Int32 intermediary = this.CalculateIntermediary(Convert.ToInt32(from), Convert.ToInt32(to), fractionDone);
			return Convert.ToChar(intermediary);
		}

		protected Int32 CalculateIntermediary(Int32 from, Int32 to, Single fractionDone)
		{
			if(fractionDone >= 1.0f)
				return to;

			if(fractionDone <= 0.0f)
				return from;

			return from + (Int32)((to - from) * fractionDone);
		}

		protected Int64 CalculateIntermediary(Int64 from, Int64 to, Single fractionDone)
		{
			if(fractionDone >= 1.0f)
				return to;

			if(fractionDone <= 0.0f)
				return from;

			return from + (Int32)((to - from) * fractionDone);
		}

		protected Single CalculateIntermediary(Single from, Single to, Single fractionDone)
		{
			if(fractionDone >= 1.0f)
				return to;

			if(fractionDone <= 0.0f)
				return from;

			return from + ((to - from) * fractionDone);
		}

		protected Double CalculateIntermediary(Double from, Double to, Single fractionDone)
		{
			if(fractionDone >= 1.0f)
				return to;

			if(fractionDone <= 0.0f)
				return from;

			return from + ((to - from) * fractionDone);
		}

		protected Color CalculateIntermediary(Color from, Color to, Single fractionDone)
		{
			if(fractionDone >= 1.0f)
				return to;

			if(fractionDone <= 0.0f)
				return from;

			// There are a couple of different strategies we could use here:
			// - Calc intermediary individual RGB components - fastest
			// - Calc intermediary HSB components - nicest results, but slower

			//Color c = Color.FromArgb(
			//    this.CalculateIntermediary(from.R, to.R, fractionDone),
			//    this.CalculateIntermediary(from.G, to.G, fractionDone),
			//    this.CalculateIntermediary(from.B, to.B, fractionDone)
			//);
			Color c = FromHSB(
				this.CalculateIntermediary(from.GetHue(), to.GetHue(), fractionDone),
				this.CalculateIntermediary(from.GetSaturation(), to.GetSaturation(), fractionDone),
				this.CalculateIntermediary(from.GetBrightness(), to.GetBrightness(), fractionDone)
			);

			return Color.FromArgb(this.CalculateIntermediary(from.A, to.A, fractionDone), c);
		}

		/// <summary>Convert a HSB tuple into a RGB color</summary>
		/// <param name="hue"></param>
		/// <param name="saturation"></param>
		/// <param name="brightness"></param>
		/// <returns></returns>
		/// <remarks>
		/// Adapted from http://discuss.fogcreek.com/dotnetquestions/default.asp?cmd=show&ixPost=846
		/// </remarks>
		protected static Color FromHSB(Single hue, Single saturation, Single brightness)
		{

			if(hue < 0 || hue > 360)
				throw new ArgumentException("Value must be between 0 and 360", "hue");
			if(saturation < 0 || saturation > 100)
				throw new ArgumentException("Value must be between 0 and 100", "saturation");
			if(brightness < 0 || brightness > 100)
				throw new ArgumentException("Value must be between 0 and 100", "brightness");

			Single h = hue;
			Single s = saturation;
			Single v = brightness;

			Int32 i;
			Single f, p, q, t;
			Single r, g, b;

			if(saturation == 0)
			{
				// achromatic (grey)
				return Color.FromArgb((Int32)(v * 255), (Int32)(v * 255), (Int32)(v * 255));
			}
			h /= 60; // sector 0 to 5
			i = (Int32)Math.Floor(h);
			f = h - i;
			p = v * (1 - s);
			q = v * (1 - s * f);
			t = v * (1 - s * (1 - f));
			switch(i)
			{
			case 0:
				r = v;
				g = t;
				b = p;
				break;
			case 1:
				r = q;
				g = v;
				b = p;
				break;
			case 2:
				r = p;
				g = v;
				b = t;
				break;
			case 3:
				r = p;
				g = q;
				b = v;
				break;
			case 4:
				r = t;
				g = p;
				b = v;
				break;
			case 5:
			default:
				r = v;
				g = p;
				b = q;
				break;
			}
			return Color.FromArgb((Int32)(r * 255f), (Int32)(g * 255f), (Int32)(b * 255f));
		}

		protected String CalculateIntermediary(String from, String to, Single fractionDone)
		{
			Int32 length1 = from.Length;
			Int32 length2 = to.Length;
			Int32 length = Math.Max(length1, length2);
			Char[] result = new Char[length];

			Int32 complete = this.CalculateIntermediary(0, length2, fractionDone);

			for(Int32 i = 0; i < length; ++i)
			{
				if(i < complete)
					result[i] = i < length2 ? to[i] : ' ';
				else
				{
					Char fromChar = (i < length1) ? from[i] : 'a';
					Char toChar = (i < length2) ? to[i] : 'a';

					if(toChar == ' ')
						result[i] = ' ';
					else
					{
						Int32 mid = this.CalculateIntermediary(
							Convert.ToInt32(fromChar), Convert.ToInt32(toChar), fractionDone);
						// Unless we're finished, make sure that the same chars don't always map
						// to the same intermediate value.
						if(fractionDone < 1.0f)
							mid -= i;
						Char c = Convert.ToChar(mid);
						if(Char.IsUpper(toChar) && Char.IsLower(c))
							c = Char.ToUpper(c);
						else
							if(Char.IsLower(toChar) && Char.IsUpper(c))
							c = Char.ToLower(c);
						result[i] = c;
					}
				}
			}

			return new String(result);
		}

		//protected Object CalculateIntermediary(Object from, Object to, float fractionDone) {
		//    throw new NotImplementedException();
		//}

		#endregion

		#region Initializations

		protected void InitializeLocator(IPointLocator locator)
		{
			if(locator != null && locator.Sprite == null)
				locator.Sprite = this.Sprite;
		}

		protected void InitializeLocator(IRectangleLocator locator)
		{
			if(locator != null && locator.Sprite == null)
				locator.Sprite = this.Sprite;
		}

		protected void InitializeEffect(IEffect effect)
		{
			if(effect != null && effect.Sprite == null)
				effect.Sprite = this.Sprite;
		}

		#endregion
	}

	/// <summary>This abstract class save and restores the location of its target sprite</summary>
	public class LocationEffect : Effect
	{
		public override void Start()
			=> this._originalLocation = this.Sprite.Location;

		public override void Reset()
			=> this.Sprite.Location = this._originalLocation;

		private Point _originalLocation;
	}

	/// <summary>This animation moves from sprite from one calculated point to another</summary>
	public class MoveEffect : LocationEffect
	{
		#region Life and death

		public MoveEffect(IPointLocator to)
			=> this._To = to;

		public MoveEffect(IPointLocator from, IPointLocator to)
		{
			this._From = from;
			this._To = to;
		}

		#endregion

		#region Configuration properties

		protected IPointLocator _From;
		protected IPointLocator _To;

		#endregion

		#region Public methods

		public override void Start()
		{
			base.Start();

			if(this._From == null)
				this._From = new FixedPointLocator(this.Sprite.Location);

			this.InitializeLocator(this._From);
			this.InitializeLocator(this._To);
		}

		public override void Apply(Single fractionDone)
			=> this.Sprite.Location = this.CalculateIntermediary(this._From.GetPoint(), this._To.GetPoint(), fractionDone);

		#endregion
	}

	/// <summary>This animation moves a sprite to a given point then halts.</summary>
	public class GotoEffect : MoveEffect
	{
		public GotoEffect(IPointLocator to)
			: base(to)
		{
		}

		public override void Apply(Single fractionDone)
			=> this.Sprite.Location = this._To.GetPoint();
	}

	/// <summary>This animation spins a sprite in place</summary>
	public class RotationEffect : Effect
	{
		#region Life and death

		public RotationEffect(Single from, Single to)
		{
			this.From = from;
			this.To = to;
		}

		#endregion

		#region Configuration properties

		protected Single From;
		protected Single To;

		#endregion

		#region Public methods

		public override void Start()
			=> this._originalSpin = this.Sprite.Spin;

		public override void Apply(Single fractionDone)
			=> this.Sprite.Spin = this.CalculateIntermediary(this.From, this.To, fractionDone);

		public override void Reset()
			=> this.Sprite.Spin = this._originalSpin;

		#endregion

		private Single _originalSpin;
	}

	/// <summary>This animation fades/reveals a sprite by altering its opacity</summary>
	public class FadeEffect : Effect
	{
		#region Life and death

		public FadeEffect(Single from, Single to)
		{
			this.From = from;
			this.To = to;
		}

		#endregion

		#region Configuration properties

		protected Single From;
		protected Single To;

		#endregion

		#region Public methods

		public override void Start()
			=> this._originalOpacity = this.Sprite.Opacity;

		public override void Apply(Single fractionDone)
			=> this.Sprite.Opacity = this.CalculateIntermediary(this.From, this.To, fractionDone);

		public override void Reset()
			=> this.Sprite.Opacity = this._originalOpacity;

		#endregion

		private Single _originalOpacity;
	}

	/// <summary>This animation fades a sprite in and out.</summary>
	/// <remarks>
	/// <para>
	/// This effect is structured to have a fade in interval, followed by a full visible
	/// interval, followed by a fade out interval and an invisible interval. The fade in
	/// and fade out are linear transitions. Any interval can be skipped by passing zero
	/// for its period.
	/// </para>
	/// <para>The intervals are proportions, not absolutes. They are not the milliseconds
	/// that will be spent in each phase. They are the relative proportions that will be
	/// spent in each phase.</para>
	/// <para>To blink more than once, use a Repeater effect.</para>
	/// <example>sprite.Add(0, 1000, new Repeater(4, new BlinkEffect(1, 2, 1, 1)));</example>
	/// </remarks>
	public class BlinkEffect : Effect
	{
		#region Life and death

		public BlinkEffect(Int32 fadeIn, Int32 visible, Int32 fadeOut, Int32 invisible)
		{
			this.FadeIn = fadeIn;
			this.Visible = visible;
			this.FadeOut = fadeOut;
			this.Invisible = invisible;
		}

		#endregion

		#region Configuration properties

		protected Int32 FadeIn;
		protected Int32 FadeOut;
		protected Int32 Visible;
		protected Int32 Invisible;

		#endregion

		#region Public methods

		public override void Start()
			=> this._originalOpacity = this.Sprite.Opacity;

		public override void Apply(Single fractionDone)
		{
			Single total = this.FadeIn + this.Visible + this.FadeOut + this.Invisible;

			// Are we in the invisible phase?
			if(((this.FadeIn + this.Visible + this.FadeOut) / total) < fractionDone)
			{
				this.Sprite.Opacity = 0.0f;
				return;
			}

			// Are we in the fade out phase?
			Single fadeOutStart = (this.FadeIn + this.Visible) / total;
			if(fadeOutStart < fractionDone)
			{
				Single progressInPhase = fractionDone - fadeOutStart;
				this.Sprite.Opacity = 1.0f - (progressInPhase / (this.FadeOut / total));
				return;
			}

			// Are we in the visible phase?
			if(((this.FadeIn) / total) < fractionDone)
			{
				this.Sprite.Opacity = 1.0f;
				return;
			}

			// We must be in the FadeIn phase?
			if(this.FadeIn > 0)
				this.Sprite.Opacity = fractionDone / (this.FadeIn / total);
		}

		public override void Reset()
			=> this.Sprite.Opacity = this._originalOpacity;

		#endregion

		private Single _originalOpacity;
	}

	/// <summary>This animation enlarges or shrinks a sprite.</summary>
	public class ScaleEffect : Effect
	{
		#region Life and death

		public ScaleEffect(Single from, Single to)
		{
			this.From = from;
			this.To = to;
		}

		#endregion

		#region Configuration properties

		protected Single From;
		protected Single To;

		#endregion

		#region Public methods

		public override void Start()
			=> this._originalScale = this.Sprite.Scale;

		public override void Apply(Single fractionDone)
			=> this.Sprite.Scale = this.CalculateIntermediary(this.From, this.To, fractionDone);

		public override void Reset()
			=> this.Sprite.Scale = this._originalScale;

		#endregion

		private Single _originalScale;
	}

	/// <summary>This animation progressively changes the bounds of a sprite</summary>
	public class BoundsEffect : Effect
	{
		#region Life and death

		public BoundsEffect(IRectangleLocator to)
			=> this.To = to;

		public BoundsEffect(IRectangleLocator from, IRectangleLocator to)
		{
			this.From = from;
			this.To = to;
		}

		#endregion

		#region Configuration properties

		protected IRectangleLocator From;
		protected IRectangleLocator To;

		#endregion

		#region Public methods

		public override void Start()
		{
			this._originalBounds = this.Sprite.Bounds;

			if(this.From == null)
				this.From = new FixedRectangleLocator(this.Sprite.Bounds);
			if(this.To == null)
				this.To = new FixedRectangleLocator(this.Sprite.Bounds);

			this.InitializeLocator(this.From);
			this.InitializeLocator(this.To);
		}

		public override void Apply(Single fractionDone)
		{
			this.Sprite.Bounds = this.CalculateIntermediary(this.From.GetRectangle(),
				this.To.GetRectangle(), fractionDone);
		}

		public override void Reset()
			=> this.Sprite.Bounds = this._originalBounds;

		#endregion

		private Rectangle _originalBounds;
	}

	/// <summary>Move the sprite so that it "walks" around the given rectangle</summary>
	public class RectangleWalkEffect : LocationEffect
	{
		#region Life and death

		public RectangleWalkEffect()
		{

		}

		public RectangleWalkEffect(IRectangleLocator rectangleLocator)
			: this(rectangleLocator, null)
		{
		}

		public RectangleWalkEffect(IRectangleLocator rectangleLocator, IPointLocator alignmentPointLocator)
			: this(rectangleLocator, alignmentPointLocator, WalkDirection.Clockwise, null)
		{

		}

		public RectangleWalkEffect(IRectangleLocator rectangleLocator, IPointLocator alignmentPointLocator,
			WalkDirection direction)
			: this(rectangleLocator, alignmentPointLocator, direction, null)
		{

		}

		public RectangleWalkEffect(IRectangleLocator rectangleLocator, IPointLocator alignmentPointLocator,
			WalkDirection direction, IPointLocator startPoint)
		{
			this.RectangleLocator = rectangleLocator;
			this.AlignmentPointLocator = alignmentPointLocator;
			this.WalkDirection = direction;
			this.StartPointLocator = startPoint;
		}

		#endregion

		#region Configuration properties

		/// <summary>Gets or sets the rectangle around which the sprite will be walked</summary>
		protected IRectangleLocator RectangleLocator;

		/// <summary>Gets or sets the locator which will return the point on rectangle where the "walk" will begin.</summary>
		/// <remarks>If this is null, the walk will start in the top left.</remarks>
		protected IPointLocator StartPointLocator;

		/// <summary>Get or set the locator which will return the point of the sprite that will be walked around the rectangle</summary>
		protected IPointLocator AlignmentPointLocator;

		/// <summary>Gets or sets in what direction the walk will proceed</summary>
		protected WalkDirection WalkDirection;

		#endregion

		#region Public methods

		public override void Start()
		{
			base.Start();

			if(this.AlignmentPointLocator == null)
				this.AlignmentPointLocator = Locators.SpriteBoundsPoint(Corner.MiddleCenter);

			this.InitializeLocator(this.RectangleLocator);
			this.InitializeLocator(this.AlignmentPointLocator);
			this.InitializeLocator(this.StartPointLocator);

			IPointLocator startLocator = this.StartPointLocator ??
				Locators.At(this.RectangleLocator.GetRectangle().Location);

			this._walker = new RectangleWalker(this.RectangleLocator, this.WalkDirection, startLocator)
			{
				Sprite = this.Sprite
			};

			this._spriteLocator = new AlignedSpriteLocator(this._walker, this.AlignmentPointLocator)
			{
				Sprite = this.Sprite
			};
		}
		private RectangleWalker _walker;
		private AlignedSpriteLocator _spriteLocator;

		public override void Apply(Single fractionDone)
		{
			this._walker.WalkProgress = fractionDone;
			this.Sprite.Location = this._spriteLocator.GetPoint();
		}

		#endregion
	}

	/// <summary>Move the sprite so that it "walks" along a given series of points</summary>
	public class PointWalkEffect : LocationEffect
	{
		#region Life and death

		public PointWalkEffect()
		{
		}

		public PointWalkEffect(IEnumerable<Point> points)
			=> this.PointWalker = new PointWalker(points);

		public PointWalkEffect(IEnumerable<IPointLocator> locators)
			=> this.PointWalker = new PointWalker(locators);

		#endregion

		#region Configuration properties

		protected PointWalker PointWalker;

		/// <summary>
		/// Get or set the locator which will return the point of the sprite
		/// that will be walked around the rectangle
		/// </summary>
		protected IPointLocator AlignmentPointLocator;

		#endregion

		#region Public methods

		public override void Start()
		{
			base.Start();

			if(this.AlignmentPointLocator == null)
				this.AlignmentPointLocator = Locators.SpriteBoundsPoint(Corner.MiddleCenter);

			this.InitializeLocator(this.PointWalker);
			this.InitializeLocator(this.AlignmentPointLocator);

			this._spriteLocator = new AlignedSpriteLocator(this.PointWalker, this.AlignmentPointLocator)
			{
				Sprite = this.Sprite
			};
		}
		private AlignedSpriteLocator _spriteLocator;

		public override void Apply(Single fractionDone)
		{
			this.PointWalker.WalkProgress = fractionDone;
			this.Sprite.Location = this._spriteLocator.GetPoint();
		}

		#endregion
	}

	/// <summary>
	/// A TickerBoard clicks through from on text string to another string in a way
	/// vaguely similar to old style airport tickerboard displays. 
	/// </summary>
	/// <remarks>This is not a particularly useful class :) Someone might find it helpful.</remarks>
	public class TickerBoardEffect : Effect
	{
		public TickerBoardEffect()
		{
		}

		public TickerBoardEffect(String endString)
			=> this.EndString = endString;

		protected String StartString;
		public String EndString;

		protected TextSprite TextSprite
		{
			get => (TextSprite)this.Sprite;
		}

		public override void Start()
		{
			System.Diagnostics.Debug.Assert(this.Sprite is TextSprite);
			if(String.IsNullOrEmpty(this.StartString))
				this.StartString = this.TextSprite.Text;
		}

		public override void Apply(Single fractionDone)
			=> this.TextSprite.Text = this.CalculateIntermediary(this.StartString, this.EndString, fractionDone);

		public override void Reset()
			=> this.TextSprite.Text = this.StartString;
	}

	/// <summary>Repeaters operate on another Effect and cause it to repeat one or more times.</summary>
	/// <remarks>
	/// It does *not* change the duration of the effect.
	/// It divides the original effects duration into multiple parts.
	/// </remarks>
	/// <example>animation.Add(0, 1000, new Repeater(4, Effect.Move(Corner.TopLeft, Corner.BottomRight)));</example>
	public class Repeater : Effect
	{
		public Repeater()
		{
		}

		public Repeater(Int32 repetitions, IEffect effect)
		{
			this.Repetitions = repetitions;
			this.Effect = effect;
		}

		public Int32 Repetitions;
		public IEffect Effect;

		public override void Start()
			=> this.InitializeEffect(this.Effect);

		public override void Apply(Single fractionDone)
		{
			if(fractionDone >= 1.0f)
				this.Effect.Apply(1.0f);
			else
			{
				// Calculate how far we are through this repetition
				Double x = fractionDone * this.Repetitions;
				Double newFractionDone = x - Math.Truncate(x); // get just the fractional part
				this.Effect.Apply((Single)newFractionDone);
			}
		}
	}

	/// <summary>A GenericEffect allows any property to be set. The</summary>
	public class GenericEffect<T> : Effect
	{
		#region Life and death

		public GenericEffect()
			=> _munger = new SimpleMunger<T>(String.Empty);

		public GenericEffect(String propertyName)
			=> this.PropertyName = propertyName;

		public GenericEffect(String propertyName, T from, T to)
		{
			this.PropertyName = propertyName;
			this.From = from;
			this.To = to;
		}

		#endregion

		#region Public properties

		/// <summary>Get or set the property that will be changed by this Effect</summary>
		public String PropertyName
		{
			get => _propertyName;
			set
			{
				_propertyName = value;
				_munger = new SimpleMunger<T>(value);
			}
		}
		private String _propertyName;

		/// <summary>Gets or set the starting value for this effect</summary>
		public Object From;

		/// <summary>Gets or sets the ending value</summary>
		public Object To;

		#endregion

		#region IEffect interface

		public override void Start()
			=> this._originalValue = this.GetValue();

		public override void Apply(Single fractionDone)
		{
			Object intermediateValue = default(T);

			// Until we use 'dynamic' in C# 4.0, there is no way to force the compiler to
			// resolve which CalculateIntermediate we want based on T. So we have to put
			// this ugly switch statement here. (Yes, I know we could initialize a Dictionary,
			// keyed by Type and hold the function, but that's just more complication for no real payback).

			if(typeof(T) == typeof(Int32))
				intermediateValue = this.CalculateIntermediary((Int32)this.From, (Int32)this.To, fractionDone);
			else if(typeof(T) == typeof(Single))
				intermediateValue = this.CalculateIntermediary((Single)this.From, (Single)this.To, fractionDone);
			else if(typeof(T) == typeof(Int64))
				intermediateValue = this.CalculateIntermediary((Int64)this.From, (Int64)this.To, fractionDone);
			else if(typeof(T) == typeof(Point))
				intermediateValue = this.CalculateIntermediary((Point)this.From, (Point)this.To, fractionDone);
			else if(typeof(T) == typeof(Rectangle))
				intermediateValue = this.CalculateIntermediary((Rectangle)this.From, (Rectangle)this.To, fractionDone);
			else if(typeof(T) == typeof(Size))
				intermediateValue = this.CalculateIntermediary((Size)this.From, (Size)this.To, fractionDone);
			else if(typeof(T) == typeof(PointF))
				intermediateValue = this.CalculateIntermediary((PointF)this.From, (PointF)this.To, fractionDone);
			else if(typeof(T) == typeof(RectangleF))
				intermediateValue = this.CalculateIntermediary((RectangleF)this.From, (RectangleF)this.To, fractionDone);
			else if(typeof(T) == typeof(SizeF))
				intermediateValue = this.CalculateIntermediary((SizeF)this.From, (SizeF)this.To, fractionDone);
			else if(typeof(T) == typeof(Char))
				intermediateValue = this.CalculateIntermediary((Char)this.From, (Char)this.To, fractionDone);
			else if(typeof(T) == typeof(String))
				intermediateValue = this.CalculateIntermediary((String)this.From, (String)this.To, fractionDone);
			else if(typeof(T) == typeof(Color))
				intermediateValue = this.CalculateIntermediary((Color)this.From, (Color)this.To, fractionDone);
			else
				System.Diagnostics.Debug.Assert(false, "Unknown data type");

			this.SetValue(intermediateValue);
		}

		public override void Stop()
			=> this.SetValue(this._originalValue);

		#endregion

		#region Implementation

		/// <summary>Gets the value of our named property from our sprite</summary>
		/// <returns></returns>
		protected virtual T GetValue()
			=> this._munger.GetValue(this.Sprite);

		protected virtual void SetValue(Object value)
			=> this._munger.PutValue(this.Sprite, value);

		#endregion

		#region Implementation variable

		protected T _originalValue;
		protected SimpleMunger<T> _munger;

		#endregion

		/// <summary>Instances of this class know how to peek and poke values from an Object using reflection.</summary>
		/// <remarks>
		/// This is a simplified form of the Munger from the ObjectListView project
		/// (http://objectlistview.sourceforge.net).
		/// </remarks>
		protected class SimpleMunger<U>
		{
			public SimpleMunger()
			{
			}

			public SimpleMunger(String memberName)
				=> this.MemberName = memberName;

			/// <summary>The name of the aspect that is to be peeked or poked.</summary>
			/// <remarks>
			/// <para>This name can be a field, property or parameter-less method.</para>
			public String MemberName { get; set; }

			/// <summary>Extract the value indicated by our MemberName from the given target.</summary>
			/// <param name="target">The Object that will be peeked</param>
			/// <returns>The value read from the target</returns>
			public U GetValue(Object target)
			{
				if(String.IsNullOrEmpty(this.MemberName))
					return default;

				const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
					BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.GetField;

				// We specifically don't try to catch errors here. If this don't work, it's 
				// a programmer error that needs to be fixed.
				return (U)target.GetType().InvokeMember(this.MemberName, flags, null, target, null);
			}

			/// <summary>Poke the given value into the given target indicated by our MemberName.</summary>
			/// <param name="target">The Object that will be poked</param>
			/// <param name="value">The value that will be poked into the target</param>
			public void PutValue(Object target, Object value)
			{
				if(String.IsNullOrEmpty(this.MemberName))
					return;

				if(target == null)
					return;

				try
				{
					// Try to set a property or field first, since that's the most common case
					const BindingFlags flags3 = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
						BindingFlags.SetProperty | BindingFlags.SetField;
					target.GetType().InvokeMember(this.MemberName, flags3, null, target, new Object[] { value });
				} catch(System.MissingMethodException)
				{
					// If that failed, it could be method name that we are looking for.
					// We specifically don't catch errors here.
					const BindingFlags flags4 = BindingFlags.Public | BindingFlags.NonPublic |
						BindingFlags.Instance | BindingFlags.InvokeMethod;
					target.GetType().InvokeMember(this.MemberName, flags4, null, target, new Object[] { value });
				}
			}
		}
	}
}