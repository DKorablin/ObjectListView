﻿/*
 * Animation - An entire sequence of sprites, sounds and effects which are 
 * united to produce a "movie clip".
 * 
 * Author: Phillip Piper
 * Date: 19/10/2009 1:01 AM
 *
 * Change log:
 * 2010-02-05   JPP  - Made animation system independent of any control.
 * 2009-10-19   JPP  - Initial version
 * 
 * To do:
 * - Animation wide effects?? Freeze. Fade.
 * - FramesPerSecond setting
 * 
 * Copyright (C) 20009-2014 Phillip Piper
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

namespace BrightIdeasSoftware
{
	/// <summary>This enum tells the animation how it should behave when it reaches the end</summary>
	public enum Repeat
	{
		None = 0,
		Loop,
		Bounce, // Not yet implemented JPP 2010-02-23
		Pause
	}

	/// <summary>An animation is the "canvas" upon which multiple sprites and sounds will be drawn.</summary>
	public class Animation
	{
		#region Life and death

		public Animation()
		{
			this._Timer = new System.Timers.Timer();
			this._Stopwatch = new System.Diagnostics.Stopwatch();
			this.Interval = 30;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the outer bounds of the animation.
		/// All locations will be calculated with reference to these bounds.
		/// </summary>
		public Rectangle Bounds { get; set; }

		/// <summary>Gets or sets the "tick" interval of the animation in milliseconds.</summary>
		public Int32 Interval
		{
			get { return (Int32)this._Timer.Interval; }
			set { this._Timer.Interval = value; }
		}

		/// <summary>Gets or sets whether the sprites should be paused</summary>
		/// <remarks>Sounds that are already in progress will not be paused.</remarks>
		public Boolean Paused
		{
			get => !this._Timer.Enabled;
			set
			{
				if(value)
					this.Pause();
				else
					this.Unpause();
			}
		}

		/// <summary>
		/// Gets or sets if the animation is running.
		/// A animation is running when it has been started and not yet stopped.
		/// A paused animation is still running.
		/// </summary>
		public Boolean Running { get; protected set; }

		/// <summary>Gets or sets how the animation will behave when it reaches theend of the animation.</summary>
		public Repeat Repeat { get; set; }

		#endregion

		#region Events

		public event EventHandler<StartAnimationEventArgs> Started;

		protected void OnStarted(StartAnimationEventArgs e)
			=> this.Started?.Invoke(this, e);

		public event EventHandler<TickEventArgs> Ticked;

		protected void OnTicked(TickEventArgs e)
			=> this.Ticked?.Invoke(this, e);

		public event EventHandler<StopAnimationEventArgs> Stopped;

		protected void OnStopped(StopAnimationEventArgs e)
			=> this.Stopped?.Invoke(this, e);

		public event EventHandler<RedrawEventArgs> Redraw;

		protected void OnRedraw(RedrawEventArgs e)
			=> this.Redraw?.Invoke(this, e);

		#endregion

		#region Commands

		/// <summary>Force the animation to be redrawn</summary>
		public void Invalidate()
			=> this.OnRedraw(new RedrawEventArgs());

		/// <summary>Force the animation to be redrawn</summary>
		public void Invalidate(Rectangle r)
			=> this.OnRedraw(new RedrawEventArgs(r));

		/// <summary>Start the story</summary>
		public void Start()
		{
			this.Running = true;
			this._Timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
			this._Stopwatch.Start();
			this._Timer.Start();
			this.OnStarted(new StartAnimationEventArgs());
		}

		/// <summary>Pause the story</summary>
		/// <remarks>Any sounds that are already playing will continue to play.</remarks>
		public void Pause()
		{
			this._Stopwatch.Stop();
			this._Timer.Stop();
		}

		/// <summary>Unpause the story</summary>
		public void Unpause()
		{
			this._Stopwatch.Start();
			this._Timer.Start();
		}

		/// <summary>Stop the story.</summary>
		public void Stop()
		{
			this.Running = false;
			this._Timer.Elapsed -= new System.Timers.ElapsedEventHandler(Timer_Elapsed);
			this._Timer.Stop();
			this._Stopwatch.Stop();
			foreach(AnimateableControlBlock cb in this._ControlBlocks)
			{
				if(cb.Started && !cb.Stopped)
				{
					cb.Component.Stop();
					cb.Stopped = true;
				}
			}

			// Tell the world we have stopped
			this.OnStopped(new StopAnimationEventArgs());
		}

		/// <summary>Advance the animation one tick and then redraw.</summary>
		public void Tick()
		{
			this.TickOnce();
			this.Invalidate();
		}

		#endregion

		#region Sprite manipulation

		/// <summary>Add the given sprite to the animation so that it appears the given number of ticks after the animation starts.</summary>
		/// <param name="startTick">How many milliseconds after the animation starts should the sprite appear?</param>
		/// <param name="sprite">The sprite that will appear</param>
		public void Add(Int64 startTick, ISprite sprite)
			=> this.AddControlBlock(new AnimateableControlBlock(startTick, sprite));

		/// <summary>Add the given sound to the animation so that it begins to play the given number of ticks after the animation starts.</summary>
		/// <param name="startTick">When should  the sound begin to play?</param>
		/// <param name="sprite">The sprite that will appear</param>
		public void Add(Int64 startTick, Audio sound)
		{
			this.AddControlBlock(new AnimateableControlBlock(startTick, sound));
		}

		#endregion

		#region Implementation

		/// <summary>Add the given control block to those used by the animation</summary>
		/// <param name="component"></param>
		/// <returns></returns>
		protected void AddControlBlock(AnimateableControlBlock cb)
		{
			cb.Component.Animation = this;
			this._ControlBlocks.Add(cb);
		}

		/// <summary>The timer has elapsed. Tickle the animation.</summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void Timer_Elapsed(Object sender, System.Timers.ElapsedEventArgs e)
		{
			TickEventArgs args = new TickEventArgs();
			this.OnTicked(args);
			if(!args.Handled)
				this.Tick();
		}

		/// <summary>Advance each sprite by one "tick"</summary>
		protected void TickOnce()
		{
			this._Timer.Enabled = false;
			Int64 ms = this._Stopwatch.ElapsedMilliseconds;
			foreach(AnimateableControlBlock cb in this._ControlBlocks)
			{

				if(!cb.Stopped && cb.ScheduledStartTick <= ms)
				{
					if(!cb.Started)
					{
						cb.StartTick = ms;
						cb.Component.Start();
					}
					if(!cb.Component.Tick(ms - cb.StartTick))
					{
						cb.Stopped = true;
						cb.Component.Stop();
					}
				}
			}

			// If any component is not stopped, restart the timer. Otherwise stop the animation.
			if(this._ControlBlocks.Exists(cb => !cb.Stopped))
				this._Timer.Enabled = true;
			else
				this.AnimationEnded();
		}

		protected void AnimationEnded()
		{
			switch(this.Repeat)
			{
			case Repeat.None:
				this.Stop();
				break;
			case Repeat.Loop:
				this.Restart();
				break;
			case Repeat.Bounce:
				break;
			case Repeat.Pause:
				this.Pause();
				break;
			}
		}

		/// <summary>The animation has stopped. Restart it from the beginning.</summary>
		protected void Restart()
		{
			this.Stop();
			this._Stopwatch.Reset();
			// Reset the components in reverse order so their effects are unwound
			for(Int32 i = this._ControlBlocks.Count - 1; i >= 0; i--)
			{
				AnimateableControlBlock cb = this._ControlBlocks[i];
				cb.Component.Reset();
				cb.StartTick = 0;
				cb.Stopped = false;
			}
			this.Start();
		}

		/// <summary>Draw the sprites for this animation onto the given context</summary>
		/// <param name="g">The graphic onto which the animation will draw itself</param>
		/// <remarks>It's normally a good idea for the given Graphics Object to be
		/// double buffered to cut down on flicker.</remarks>
		public void Draw(Graphics g)
		{
			// Draw each started sprite
			foreach(AnimateableControlBlock cb in this._ControlBlocks)
				if(cb.Sprite != null && cb.Started)
					cb.Sprite.Draw(g);
		}

		#endregion

		#region Implementation classes

		/// <summary>Instances of this class are used to control the animation of a component on a animation.</summary>
		protected class AnimateableControlBlock
		{
			public AnimateableControlBlock(Int64 scheduledStartTick, IAnimateable component)
			{
				this.ScheduledStartTick = scheduledStartTick;
				this.Component = component;
			}
			public AnimateableControlBlock(Int64 scheduledStartTick, ISprite sprite) :
				this(scheduledStartTick, (IAnimateable)sprite)
			{
				this.Sprite = sprite;
			}

			public IAnimateable Component;
			public Int64 ScheduledStartTick;
			public Int64 StartTick;
			public Boolean Stopped;

			public Boolean Started
			{
				get => this.StartTick != 0;
			}

			/// <summary>Gets or sets the sprite that is managed by this control block.</summary>
			/// <remarks>Almost all components are sprites so we keep this property to prevent the use of casts.</remarks>
			public ISprite Sprite;
		}

		#endregion

		#region Private variables

		private System.Timers.Timer _Timer;
		private System.Diagnostics.Stopwatch _Stopwatch;
		private List<AnimateableControlBlock> _ControlBlocks = new List<AnimateableControlBlock>();

		#endregion
	}
}