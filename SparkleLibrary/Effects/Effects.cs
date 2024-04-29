/*
 * Effects - A Factory that creates commonly useful effects
 * 
 * Author: Phillip Piper
 * Date: 18/01/2010 5:29 PM
 *
 * Change log:
 * 2010-01-18   JPP  - Initial version
 *
 * To do:
 *
 * Copyright (C) 2010 Phillip Piper
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
using System.Drawing;

namespace BrightIdeasSoftware
{
	/// <summary>Factory methods that create commonly useful effects</summary>
	public static class Effects
	{
		public static MoveEffect Move(Int32 x, Int32 y)
			=> new MoveEffect(new FixedPointLocator(new Point(x, y)));

		public static MoveEffect Move(Point from, Point to)
			=> new MoveEffect(new FixedPointLocator(from), new FixedPointLocator(to));

		public static MoveEffect Move(Corner toCorner)
			=> new MoveEffect(Locators.SpriteAligned(toCorner));

		public static MoveEffect Move(Corner toCorner, Point toOffset)
			=> new MoveEffect(Locators.SpriteAligned(toCorner, toOffset));

		public static MoveEffect Move(Corner fromCorner, Corner toCorner)
			=> new MoveEffect(
				Locators.SpriteAligned(fromCorner),
				Locators.SpriteAligned(toCorner));

		public static MoveEffect Move(Corner fromCorner, Corner toCorner, Point toOffset)
			=> new MoveEffect(
				Locators.SpriteAligned(fromCorner),
				Locators.SpriteAligned(toCorner, toOffset));

		public static MoveEffect Move(Corner fromCorner, Point fromOffset, Corner toCorner, Point toOffset)
			=> new MoveEffect(
				Locators.SpriteAligned(fromCorner, fromOffset),
				Locators.SpriteAligned(toCorner, toOffset));

		/// <summary>
		/// Create a Mover that will move a sprite so that the middle of the sprite moves from the given
		/// proportional location of the animation bounds to the other given proportional location.
		/// </summary>
		public static MoveEffect Move(Single fromProportionX, Single fromProportionY, Single toProportionX, Single toProportionY)
			=> Effects.Move(Corner.MiddleCenter, fromProportionX, fromProportionY, toProportionX, toProportionY);

		/// <summary>
		/// Create a Mover that will move a sprite so that the given corner moves from the given
		/// proportional location of the animation bounds to the other given proportional location.
		/// </summary>
		public static MoveEffect Move(Corner spriteCorner, Single fromProportionX, Single fromProportionY, Single toProportionX, Single toProportionY)
			=> new MoveEffect(
				Locators.SpriteAligned(spriteCorner, fromProportionX, fromProportionY),
				Locators.SpriteAligned(spriteCorner, toProportionX, toProportionY));

		public static GotoEffect Goto(Int32 x, Int32 y)
			=> new GotoEffect(Locators.At(x, y));

		public static GotoEffect Goto(Corner toCorner)
			=> new GotoEffect(Locators.SpriteAligned(toCorner));

		public static GotoEffect Goto(Corner toCorner, Point toOffset)
			=> new GotoEffect(Locators.SpriteAligned(toCorner, toOffset));

		/// <summary>Creates an animation that keeps the given corner in the centre of the animation</summary>
		/// <param name="cornerToCenter"></param>
		/// <returns></returns>
		public static MoveEffect Centre(Corner cornerToCenter)
			=> new GotoEffect(Locators.SpriteAligned(Corner.MiddleCenter, cornerToCenter));

		public static RotationEffect Rotate(Single from, Single to)
			=> new RotationEffect(from, to);

		public static FadeEffect Fade(Single from, Single to)
			=> new FadeEffect(from, to);

		public static ScaleEffect Scale(Single from, Single to)
			=> new ScaleEffect(from, to);

		public static BoundsEffect Bounds(IRectangleLocator to)
			=> new BoundsEffect(to);

		public static BoundsEffect Bounds(IRectangleLocator from, IRectangleLocator to)
			=> new BoundsEffect(from, to);

		public static RectangleWalkEffect Walk(IRectangleLocator rl)
			=> new RectangleWalkEffect(rl);

		public static RectangleWalkEffect Walk(IRectangleLocator rl, WalkDirection direction)
			=> new RectangleWalkEffect(rl, null, direction);

		public static RectangleWalkEffect Walk(IRectangleLocator rl, Corner start, WalkDirection direction)
			=> new RectangleWalkEffect(rl, null, direction, new PointOnRectangleLocator(rl, start));

		public static TickerBoardEffect TickerBoard(String endString)
			=> new TickerBoardEffect(endString);

		public static Repeater Repeat(Int32 repetitions, IEffect effect)
			=> new Repeater(repetitions, effect);

		public static BlinkEffect OneBlink(Int32 fadeIn, Int32 visible, Int32 fadeOut, Int32 invisible)
			=> new BlinkEffect(fadeIn, visible, fadeOut, invisible);

		public static Repeater Blink(Int32 repetitions, Int32 fading, Int32 visible)
			=> Effects.Repeat(repetitions, Effects.OneBlink(fading, visible, fading, 0));

		public static Repeater Blink(Int32 repetitions)
			=> Effects.Repeat(repetitions, Effects.OneBlink(2, 4, 2, 1));
	}
}