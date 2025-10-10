/*
 * Filters - Filtering on ObjectListViews
 *
 * Author: Phillip Piper
 * Date: 03/03/2010 17:00 
 *
 * Change log:
 * 2011-03-01  JPP  Added CompositeAllFilter, CompositeAnyFilter and OneOfFilter
 * v2.4.1
 * 2010-06-23  JPP  Extended TextMatchFilter to handle regular expressions and String prefix matching.
 * v2.4
 * 2010-03-03  JPP  Initial version
 *
 * TO DO:
 *
 * Copyright (C) 2010-2014 Phillip Piper
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BrightIdeasSoftware
{
	/// <summary>Interface for model-by-model filtering</summary>
	public interface IModelFilter
	{
		/// <summary>Should the given model be included when this filter is installed</summary>
		/// <param name="modelObject">The model Object to consider</param>
		/// <returns>Returns true if the model will be included by the filter</returns>
		Boolean Filter(Object modelObject);
	}

	/// <summary>Interface for whole list filtering</summary>
	public interface IListFilter
	{
		/// <summary>Return a subset of the given list of model objects as the new contents of the ObjectListView</summary>
		/// <param name="modelObjects">The collection of model objects that the list will possibly display</param>
		/// <returns>The filtered collection that holds the model objects that will be displayed.</returns>
		IEnumerable Filter(IEnumerable modelObjects);
	}

	/// <summary>Base class for model-by-model filters</summary>
	public class AbstractModelFilter : IModelFilter
	{
		/// <summary>Should the given model be included when this filter is installed</summary>
		/// <param name="modelObject">The model Object to consider</param>
		/// <returns>Returns true if the model will be included by the filter</returns>
		virtual public Boolean Filter(Object modelObject)
			=> true;
	}

	/// <summary>This filter calls a given Predicate to decide if a model Object should be included</summary>
	public class ModelFilter : IModelFilter
	{
		/// <summary>Create a filter based on the given predicate</summary>
		/// <param name="predicate">The function that will filter objects</param>
		public ModelFilter(Predicate<Object> predicate)
			=> this.Predicate = predicate;

		/// <summary>Gets or sets the predicate used to filter model objects.</summary>
		protected Predicate<Object> Predicate { get; set; }

		/// <summary>Returns true if the model object is accepted by the predicate.</summary>
		/// <param name="modelObject">The model object to consider.</param>
		/// <returns>True if the model should be included.</returns>
		virtual public Boolean Filter(Object modelObject)
			=> this.Predicate == null || this.Predicate.Invoke(modelObject);
	}

	/// <summary>A CompositeFilter joins several other filters together.</summary>
	/// <remarks>If there are no filters, all model objects are included</remarks>
	public abstract class CompositeFilter : IModelFilter
	{

		/// <summary>Creates an empty CompositeFilter.</summary>
		protected CompositeFilter()
		{
		}

		/// <summary>Creates a composite filter from the given list of filters.</summary>
		/// <param name="filters">A list of filters</param>
		protected CompositeFilter(IEnumerable<IModelFilter> filters)
		{
			foreach(IModelFilter filter in filters)
				if(filter != null)
					this.Filters.Add(filter);
		}

		/// <summary>Gets or sets the filters used by this composite filter.</summary>
		public IList<IModelFilter> Filters { get; set; } = new List<IModelFilter>();

		/// <summary>Gets the collection of text filters within this composite.</summary>
		public IEnumerable<TextMatchFilter> TextFilters
		{
			get
			{
				foreach(IModelFilter filter in this.Filters)
					if(filter is TextMatchFilter textFilter)
						yield return textFilter;
			}
		}

		/// <summary>Returns true if the model object is accepted by the filter.</summary>
		/// <param name="modelObject">The model object to consider.</param>
		/// <returns>True if the Object is included by the filter</returns>
		virtual public Boolean Filter(Object modelObject)
		{
			if(this.Filters == null || this.Filters.Count == 0)
				return true;

			return this.FilterObject(modelObject);
		}

		/// <summary>Decide if the given model should be included by the filter.</summary>
		/// <remarks>Filters is guaranteed to be non-empty when this method is called</remarks>
		/// <param name="modelObject">The model Object under consideration</param>
		/// <returns>True if the Object is included by the filter</returns>
		abstract public Boolean FilterObject(Object modelObject);
	}

	/// <summary>
	/// A CompositeAllFilter joins several other filters together.
	/// A model Object must satisfy all filters to be included.
	/// If there are no filters, all model objects are included
	/// </summary>
	public class CompositeAllFilter : CompositeFilter
	{

		/// <summary>Creates a CompositeAllFilter from the given list of filters.</summary>
		/// <param name="filters">The list of filters to use.</param>
		public CompositeAllFilter(List<IModelFilter> filters)
			: base(filters)
		{
		}

		/// <summary>Decide whether or not the given model should be included by the filter</summary>
		/// <remarks>Filters is guaranteed to be non-empty when this method is called</remarks>
		/// <param name="modelObject">The model Object under consideration</param>
		/// <returns>True if the Object is included by the filter</returns>
		override public Boolean FilterObject(Object modelObject)
		{
			foreach(IModelFilter filter in this.Filters)
				if(!filter.Filter(modelObject))
					return false;

			return true;
		}
	}

	/// <summary>
	/// A CompositeAnyFilter joins several other filters together.
	/// A model Object must only satisfy one of the filters to be included.
	/// If there are no filters, all model objects are included
	/// </summary>
	public class CompositeAnyFilter : CompositeFilter
	{

		/// <summary>Creates a CompositeAnyFilter from the given list of filters.</summary>
		/// <param name="filters">The list of filters to use.</param>
		public CompositeAnyFilter(List<IModelFilter> filters)
			: base(filters)
		{
		}

		/// <summary>Decide whether or not the given model should be included by the filter</summary>
		/// <remarks>Filters is guaranteed to be non-empty when this method is called</remarks>
		/// <param name="modelObject">The model Object under consideration</param>
		/// <returns>True if the Object is included by the filter</returns>
		override public Boolean FilterObject(Object modelObject)
			=> this.Filters.Any(f => f.Filter(modelObject));
	}

	/// <summary>
	/// Instances of this class extract a value from the model object and compare that value to a list of fixed values.
	/// The model object is included if the extracted value is in the list.
	/// </summary>
	/// <remarks>If there is no delegate installed or there are no values to match, no model objects will be matched</remarks>
	public class OneOfFilter : IModelFilter
	{

		/// <summary>Creates a filter that will use the given delegate to extract values.</summary>
		/// <param name="valueGetter">The delegate to extract values.</param>
		public OneOfFilter(AspectGetterDelegate valueGetter) :
			this(valueGetter, new ArrayList())
		{
		}

		/// <summary>Creates a filter that will extract values using the given delegate and compare them to the given values.</summary>
		/// <param name="valueGetter">The delegate to extract values.</param>
		/// <param name="possibleValues">The list of values to match against.</param>
		public OneOfFilter(AspectGetterDelegate valueGetter, ICollection possibleValues)
		{
			this.ValueGetter = valueGetter;
			this.PossibleValues = new ArrayList(possibleValues);
		}

		/// <summary>Gets or sets the delegate that will extract values from model objects.</summary>
		virtual public AspectGetterDelegate ValueGetter { get; set; }

		/// <summary>Gets or sets the list of values to be matched.</summary>
		virtual public IList PossibleValues { get; set; }

		/// <summary>Returns true if the model object is accepted by the filter.</summary>
		/// <param name="modelObject">The model object to consider.</param>
		/// <returns>True if the object should be included.</returns>
		public virtual Boolean Filter(Object modelObject)
		{
			if(this.ValueGetter == null || this.PossibleValues == null || this.PossibleValues.Count == 0)
				return false;

			Object result = this.ValueGetter(modelObject);
			IEnumerable enumerable = result as IEnumerable;
			if(result is String || enumerable == null)
				return this.DoesValueMatch(result);

			foreach(Object x in enumerable)
			{
				if(this.DoesValueMatch(x))
					return true;
			}
			return false;
		}

		/// <summary>Returns true if the given value is present in the PossibleValues collection.</summary>
		/// <param name="result">The value to check.</param>
		/// <returns>True if the value is in the collection.</returns>
		protected virtual Boolean DoesValueMatch(Object result)
			=> this.PossibleValues.Contains(result);
	}

	/// <summary>
	/// Instances of this class match a property of a model objects against a list of bit flags.
	/// The property should be an xor-ed collection of bits flags.
	/// </summary>
	/// <remarks>Both the property compared and the list of possible values must be convertible to ulongs.</remarks>
	public class FlagBitSetFilter : OneOfFilter
	{

		/// <summary>Creates a FlagBitSetFilter.</summary>
		/// <param name="valueGetter">The delegate to extract values.</param>
		/// <param name="possibleValues">The flag values to match against.</param>
		public FlagBitSetFilter(AspectGetterDelegate valueGetter, ICollection possibleValues)
			: base(valueGetter, possibleValues)
			=> this.ConvertPossibleValues();

		/// <summary>
		/// Gets or sets the collection of values that will be matched.
		/// These must be ulongs (or convertible to ulongs).
		/// </summary>
		public override IList PossibleValues
		{
			get => base.PossibleValues;
			set
			{
				base.PossibleValues = value;
				this.ConvertPossibleValues();
			}
		}

		private void ConvertPossibleValues()
		{
			this._possibleValuesAsUlongs = new List<UInt64>();
			foreach(Object x in this.PossibleValues)
				this._possibleValuesAsUlongs.Add(Convert.ToUInt64(x));
		}

		/// <summary>Decides if the given property is a match for the values in the PossibleValues collection</summary>
		/// <param name="result">The value to check.</param>
		/// <returns>True if the value matches.</returns>
		protected override Boolean DoesValueMatch(Object result)
		{
			try
			{
				UInt64 value = Convert.ToUInt64(result);
				foreach(UInt64 flag in this._possibleValuesAsUlongs)
				{
					if((value & flag) == flag)
						return true;
				}
				return false;
			} catch(InvalidCastException)
			{
				return false;
			} catch(FormatException)
			{
				return false;
			}
		}

		private List<UInt64> _possibleValuesAsUlongs = new List<UInt64>();
	}

	/// <summary>Base class for whole list filters</summary>
	public class AbstractListFilter : IListFilter
	{
		/// <summary>Return a subset of the given list of model objects as the new contents of the ObjectListView</summary>
		/// <param name="modelObjects">The collection of model objects that the list will possibly display</param>
		/// <returns>The filtered collection that holds the model objects that will be displayed.</returns>
		virtual public IEnumerable Filter(IEnumerable modelObjects)
			=> modelObjects;
	}

	/// <summary>Instance of this class implement delegate based whole list filtering</summary>
	public class ListFilter : AbstractListFilter
	{
		/// <summary>Defines a delegate that filters a collection of model objects.</summary>
		/// <param name="rowObjects">The collection to be filtered.</param>
		/// <returns>An IEnumerable of the filtered objects.</returns>
		public delegate IEnumerable ListFilterDelegate(IEnumerable rowObjects);

		/// <summary>Creates a ListFilter using the given delegate.</summary>
		/// <param name="function">The delegate to use for filtering.</param>
		public ListFilter(ListFilterDelegate function)
			=> this.Function = function;

		/// <summary>Gets or sets the delegate that will filter the list.</summary>
		public ListFilterDelegate Function { get; set; }

		/// <summary>Filters the given collection of model objects using the installed delegate.</summary>
		/// <param name="modelObjects">The collection to be filtered.</param>
		/// <returns>The filtered collection.</returns>
		public override IEnumerable Filter(IEnumerable modelObjects)
		{
			if(this.Function == null)
				return modelObjects;

			return this.Function(modelObjects);
		}
	}

	/// <summary>Filter the list so only the last N entries are displayed.</summary>
	public class TailFilter : AbstractListFilter
	{
		/// <summary>Creates a no-op tail filter.</summary>
		public TailFilter()
		{
		}

		/// <summary>Creates a filter that includes only the last N model objects.</summary>
		/// <param name="numberOfObjects">The number of objects to include.</param>
		public TailFilter(Int32 numberOfObjects)
			=> this.Count = numberOfObjects;

		/// <summary>Gets or sets the number of objects to be shown.</summary>
		public Int32 Count { get; set; }

		/// <summary>Filters the given collection to return only the last N objects.</summary>
		/// <param name="modelObjects">The collection to be filtered.</param>
		/// <returns>A collection of the last N objects.</returns>
		public override IEnumerable Filter(IEnumerable modelObjects)
		{
			if(this.Count <= 0)
				return modelObjects;

			ArrayList list = ObjectListView.EnumerableToArray(modelObjects, false);

			if(this.Count > list.Count)
				return list;

			Object[] tail = new Object[this.Count];
			list.CopyTo(list.Count - this.Count, tail, 0, this.Count);
			return new ArrayList(tail);
		}
	}
}