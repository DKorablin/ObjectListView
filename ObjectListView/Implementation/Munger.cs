/*
 * Munger - An Interface pattern on getting and setting values from Object through Reflection
 *
 * Author: Phillip Piper
 * Date: 28/11/2008 17:15 
 *
 * Change log:
 * v2.5.1
 * 2012-05-01  JPP  - Added IgnoreMissingAspects property
 * v2.5
 * 2011-05-20  JPP  - Accessing through an indexer when the target had both a integer and
 *                    a String indexer didn't work reliably.
 * v2.4.1
 * 2010-08-10  JPP  - Refactored into Munger/SimpleMunger. 3x faster!
 * v2.3
 * 2009-02-15  JPP  - Made Munger a public class
 * 2009-01-20  JPP  - Made the Munger capable of handling indexed access.
 *                    Incidentally, this removed the ugliness that the last change introduced.
 * 2009-01-18  JPP  - Handle target objects from a DataListView (normally DataRowViews)
 * v2.0
 * 2008-11-28  JPP  Initial version
 *
 * TO DO:
 *
 * Copyright (C) 2006-2014 Phillip Piper
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
using System.Reflection;

namespace BrightIdeasSoftware
{
	/// <summary>
	/// An instance of Munger gets a value from or puts a value into a target Object. The property
	/// to be peeked (or poked) is determined from a String. The peeking or poking is done using reflection.
	/// </summary>
	/// <remarks>
	/// Name of the aspect to be peeked can be a field, property or parameterless method.
	/// The name of an aspect to poke can be a field, writable property or single parameter method.
	/// <para>Aspect names can be dotted to chain a series of references. </para>
	/// <example>Order.Customer.HomeAddress.State</example>
	/// </remarks>
	public class Munger
	{
		#region Life and death

		/// <summary>Create a do nothing Munger</summary>
		public Munger()
		{
		}

		/// <summary>Create a Munger that works on the given aspect name</summary>
		/// <param name="aspectName">The name of the </param>
		public Munger(String aspectName)
			=> this.AspectName = aspectName;

		#endregion

		#region Static utility methods

		/// <summary>A helper method to put the given value into the given aspect of the given Object.</summary>
		/// <remarks>This method catches and silently ignores any errors that occur
		/// while modifying the target Object</remarks>
		/// <param name="target">The Object to be modified</param>
		/// <param name="propertyName">The name of the property/field to be modified</param>
		/// <param name="value">The value to be assigned</param>
		/// <returns>Did the modification work?</returns>
		public static Boolean PutProperty(Object target, String propertyName, Object value)
		{
			try
			{
				Munger munger = new Munger(propertyName);
				return munger.PutValue(target, value);
			} catch(MungerException)
			{
				// Not a lot we can do about this. Something went wrong in the bowels
				// of the property. Let's take the ostrich approach and just ignore it :-)

				// Normally, we would never just silently ignore an exception.
				// However, in this case, this is a utility method that explicitly 
				// contracts to catch and ignore errors. If this is not acceptable,
				// the programmer should not use this method.
			}

			return false;
		}

		/// <summary>Gets or sets whether Mungers will silently ignore missing aspect errors.</summary>
		/// <remarks>
		/// <para>
		/// By default, if a Munger is asked to fetch a field/property/method
		/// that does not exist from a model, it returns an error message, since that 
		/// condition is normally a programming error. There are some use cases where
		/// this is not an error, and the munger should simply keep quiet.
		/// </para>
		/// <para>By default this is true during release builds.</para>
		/// </remarks>
		public static Boolean IgnoreMissingAspects { get; set; }
#if DEBUG
			= false;
#else
			= true;
#endif

		#endregion

		#region Public properties

		/// <summary>The name of the aspect that is to be peeked or poked.</summary>
		/// <remarks>
		/// <para>This name can be a field, property or parameter-less method.</para>
		/// <para>
		/// The name can be dotted, which chains references.
		/// If any link in the chain returns null, the entire chain is considered to return null.
		/// </para>
		/// </remarks>
		/// <example>"DateOfBirth"</example>
		/// <example>"Owner.HomeAddress.Postcode"</example>
		public String AspectName
		{
			get => this._aspectName;
			set
			{
				this._aspectName = value;

				// Clear any cache
				this._aspectParts = null;
			}
		}
		private String _aspectName;

		#endregion

		#region Public interface

		/// <summary>Extract the value indicated by our AspectName from the given target.</summary>
		/// <remarks>If the aspect name is null or empty, this will return null.</remarks>
		/// <param name="target">The Object that will be peeked</param>
		/// <returns>The value read from the target</returns>
		public Object GetValue(Object target)
		{
			if(this.Parts.Count == 0)
				return null;

			try
			{
				return this.EvaluateParts(target, this.Parts);
			} catch(MungerException ex)
			{
				return Munger.IgnoreMissingAspects
					? null
					: String.Format("'{0}' is not a parameter-less method, property or field of type '{1}'",
						ex.Munger.AspectName, ex.Target.GetType());
			}
		}

		/// <summary>Extract the value indicated by our AspectName from the given target, raising exceptions if the munger fails.</summary>
		/// <remarks>If the aspect name is null or empty, this will return null.</remarks>
		/// <param name="target">The Object that will be peeked</param>
		/// <returns>The value read from the target</returns>
		public Object GetValueEx(Object target)
			=> this.Parts.Count == 0
				? null
				: this.EvaluateParts(target, this.Parts);

		/// <summary>Poke the given value into the given target indicated by our AspectName.</summary>
		/// <remarks>
		/// <para>
		/// If the AspectName is a dotted path, all the selectors bar the last
		/// are used to find the Object that should be updated, and the last
		/// selector is used as the property to update on that Object.
		/// </para>
		/// <para>
		/// So, if 'target' is a Person and the AspectName is "HomeAddress.Postcode",
		/// this method will first fetch "HomeAddress" property, and then try to set the
		/// "Postcode" property on the home address Object.
		/// </para>
		/// </remarks>
		/// <param name="target">The Object that will be poked</param>
		/// <param name="value">The value that will be poked into the target</param>
		/// <returns>bool indicating whether the put worked</returns>
		public Boolean PutValue(Object target, Object value)
		{
			if(this.Parts.Count == 0)
				return false;

			SimpleMunger lastPart = this.Parts[this.Parts.Count - 1];

			if(this.Parts.Count > 1)
			{
				List<SimpleMunger> parts = new List<SimpleMunger>(this.Parts);
				parts.RemoveAt(parts.Count - 1);
				try
				{
					target = this.EvaluateParts(target, parts);
				} catch(MungerException ex)
				{
					this.ReportPutValueException(ex);
					return false;
				}
			}

			if(target != null)
			{
				try
				{
					return lastPart.PutValue(target, value);
				} catch(MungerException ex)
				{
					this.ReportPutValueException(ex);
				}
			}

			return false;
		}

		#endregion

		#region Implementation

		/// <summary>Gets the list of SimpleMungers that match our AspectName</summary>
		private IList<SimpleMunger> Parts => this._aspectParts ?? (this._aspectParts = this.BuildParts(this.AspectName));
		private IList<SimpleMunger> _aspectParts;

		/// <summary>Convert a possibly dotted AspectName into a list of SimpleMungers</summary>
		/// <param name="aspect"></param>
		/// <returns></returns>
		private IList<SimpleMunger> BuildParts(String aspect)
		{
			List<SimpleMunger> parts = new List<SimpleMunger>();
			if(!String.IsNullOrEmpty(aspect))
				foreach(String part in aspect.Split('.'))
					parts.Add(new SimpleMunger(part.Trim()));
			return parts;
		}

		/// <summary>Evaluate the given chain of SimpleMungers against an initial target.</summary>
		/// <param name="target"></param>
		/// <param name="parts"></param>
		/// <returns></returns>
		private Object EvaluateParts(Object target, IList<SimpleMunger> parts)
		{
			foreach(SimpleMunger part in parts)
			{
				if(target == null)
					break;
				target = part.GetValue(target);
			}
			return target;
		}

		private void ReportPutValueException(MungerException ex)
		{
			//TODO: How should we report this error?
			System.Diagnostics.Debug.WriteLine("PutValue failed");
			System.Diagnostics.Debug.WriteLine(String.Format("- Culprit aspect: {0}", ex.Munger.AspectName));
			System.Diagnostics.Debug.WriteLine(String.Format("- Target: {0} of type {1}", ex.Target, ex.Target.GetType()));
			System.Diagnostics.Debug.WriteLine(String.Format("- Inner exception: {0}", ex.InnerException));
		}

		#endregion
	}

	/// <summary>A SimpleMunger deals with a single property/field/method on its target.</summary>
	/// <remarks>Munger uses a chain of these resolve a dotted aspect name.</remarks>
	public class SimpleMunger
	{
		#region Life and death

		/// <summary>Create a SimpleMunger</summary>
		/// <param name="aspectName"></param>
		public SimpleMunger(String aspectName)
			=> this.AspectName = aspectName;

		#endregion

		#region Public properties

		/// <summary>The name of the aspect that is to be peeked or poked.</summary>
		/// <remarks>
		/// <para>
		/// This name can be a field, property or method. 
		/// When using a method to get a value, the method must be parameter-less.
		/// When using a method to set a value, the method must accept 1 parameter.
		/// </para>
		/// <para>It cannot be a dotted name.</para>
		/// </remarks>
		public String AspectName { get; }

		#endregion

		#region Public interface

		/// <summary>Get a value from the given target</summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public Object GetValue(Object target)
		{
			if(target == null)
				return null;

			this.ResolveName(target, this.AspectName, 0);

			try
			{
				if(this._resolvedPropertyInfo != null)
					return this._resolvedPropertyInfo.GetValue(target, null);

				if(this._resolvedMethodInfo != null)
					return this._resolvedMethodInfo.Invoke(target, null);

				if(this._resolvedFieldInfo != null)
					return this._resolvedFieldInfo.GetValue(target);

				// If that didn't work, try to use the indexer property. 
				// This covers things like dictionaries and DataRows.
				if(this._indexerPropertyInfo != null)
					return this._indexerPropertyInfo.GetValue(target, new Object[] { this.AspectName });
			} catch(Exception ex)
			{
				// Lots of things can do wrong in these invocations
				throw new MungerException(this, target, ex);
			}

			// If we get to here, we couldn't find a match for the aspect
			throw new MungerException(this, target, new MissingMethodException());
		}

		/// <summary>Poke the given value into the given target indicated by our AspectName.</summary>
		/// <param name="target">The Object that will be poked</param>
		/// <param name="value">The value that will be poked into the target</param>
		/// <returns>bool indicating if the put worked</returns>
		public Boolean PutValue(Object target, Object value)
		{
			if(target == null)
				return false;

			this.ResolveName(target, this.AspectName, 1);

			try
			{
				if(this._resolvedPropertyInfo != null)
				{
					this._resolvedPropertyInfo.SetValue(target, value, null);
					return true;
				}

				if(this._resolvedMethodInfo != null)
				{
					this._resolvedMethodInfo.Invoke(target, new Object[] { value });
					return true;
				}

				if(this._resolvedFieldInfo != null)
				{
					this._resolvedFieldInfo.SetValue(target, value);
					return true;
				}

				// If that didn't work, try to use the indexer property. 
				// This covers things like dictionaries and DataRows.
				if(this._indexerPropertyInfo != null)
				{
					this._indexerPropertyInfo.SetValue(target, value, new Object[] { this.AspectName });
					return true;
				}
			} catch(Exception ex)
			{
				// Lots of things can do wrong in these invocations
				throw new MungerException(this, target, ex);
			}

			return false;
		}

		#endregion

		#region Implementation

		private void ResolveName(Object target, String name, Int32 numberMethodParameters)
		{
			if(this._cachedTargetType == target.GetType() && this._cachedName == name && this._cachedNumberParameters == numberMethodParameters)
				return;

			this._cachedTargetType = target.GetType();
			this._cachedName = name;
			this._cachedNumberParameters = numberMethodParameters;

			this._resolvedFieldInfo = null;
			this._resolvedPropertyInfo = null;
			this._resolvedMethodInfo = null;
			this._indexerPropertyInfo = null;

			const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance /*| BindingFlags.NonPublic*/;

			foreach(PropertyInfo pinfo in target.GetType().GetProperties(flags))
			{
				if(pinfo.Name == name)
				{
					this._resolvedPropertyInfo = pinfo;
					return;
				}

				// See if we can find an String indexer property while we are here.
				// We also need to allow for old style <Object> keyed collections.
				if(_indexerPropertyInfo == null && pinfo.Name == "Item")
				{
					ParameterInfo[] par = pinfo.GetGetMethod().GetParameters();
					if(par.Length > 0)
					{
						Type parameterType = par[0].ParameterType;
						if(parameterType == typeof(String) || parameterType == typeof(Object))
							this._indexerPropertyInfo = pinfo;
					}
				}
			}

			foreach(FieldInfo info in target.GetType().GetFields(flags))
				if(info.Name == name)
				{
					this._resolvedFieldInfo = info;
					return;
				}

			foreach(MethodInfo info in target.GetType().GetMethods(flags))
				if(info.Name == name && info.GetParameters().Length == numberMethodParameters)
				{
					this._resolvedMethodInfo = info;
					return;
				}
		}

		private Type _cachedTargetType;
		private String _cachedName;
		private Int32 _cachedNumberParameters;

		private FieldInfo _resolvedFieldInfo;
		private PropertyInfo _resolvedPropertyInfo;
		private MethodInfo _resolvedMethodInfo;
		private PropertyInfo _indexerPropertyInfo;

		#endregion
	}

	/// <summary>These exceptions are raised when a munger finds something it cannot process</summary>
	public class MungerException : ApplicationException
	{
		/// <summary>Create a MungerException</summary>
		/// <param name="munger"></param>
		/// <param name="target"></param>
		/// <param name="ex"></param>
		public MungerException(SimpleMunger munger, Object target, Exception ex)
			: base("Munger failed", ex)
		{
			this.Munger = munger;
			this.Target = target;
		}

		/// <summary>Get the munger that raised the exception</summary>
		public SimpleMunger Munger { get; }

		/// <summary>Gets the target that threw the exception</summary>
		public Object Target { get; }
	}

	/*
	 * We don't currently need this
	 * 2010-08-06
	 * 
	 
	internal class SimpleBinder : Binder
	{
		public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, Object value, System.Globalization.CultureInfo culture) {
			//return Type.DefaultBinder.BindToField(
			throw new NotImplementedException();
		}

		public override Object ChangeType(Object value, Type type, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}

		public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref Object[] args, ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, String[] names, out Object state) {
			throw new NotImplementedException();
		}

		public override void ReorderArgumentArray(ref Object[] args, Object state) {
			throw new NotImplementedException();
		}

		public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers) {
			throw new NotImplementedException();
		}

		public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers) {
			if (match == null)
				throw new ArgumentNullException("match");

			if (match.Length == 0)
				return null;

			return match[0];
		}
	}
	 */
}