/*
 * Cluster - Implements a simple cluster
 *
 * Author: Phillip Piper
 * Date: 3-March-2011 10:53 pm
 *
 * Change log:
 * 2011-03-03  JPP  - First version
 * 
 * Copyright (C) 2011-2014 Phillip Piper
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
using System.Text;

namespace BrightIdeasSoftware
{

	/// <summary>Concrete implementation of the ICluster interface.</summary>
	public class Cluster : ICluster
	{

		#region Life and death

		/// <summary>Create a cluster</summary>
		/// <param name="key">The key for the cluster</param>
		public Cluster(Object key)
		{
			this.Count = 1;
			this.ClusterKey = key;
		}

		#endregion

		#region Public overrides

		/// <summary>Return a String representation of this cluster</summary>
		/// <returns></returns>
		public override String ToString()
			=> this.DisplayLabel ?? "[empty]";

		#endregion

		#region Implementation of ICluster

		/// <summary>Gets or sets how many items belong to this cluster</summary>
		public Int32 Count { get; set; }

		/// <summary>Gets or sets the label that will be shown to the user to represent this cluster</summary>
		public String DisplayLabel { get; set; }

		/// <summary>Gets or sets the actual data Object that all members of this cluster have commonly returned.</summary>
		public Object ClusterKey { get; set; }

		#endregion

		#region Implementation of IComparable

		/// <summary>Return an indication of the ordering between this Object and the given one</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Int32 CompareTo(Object other)
		{
			if(other == null || other == DBNull.Value)
				return 1;

			if(!(other is ICluster otherCluster))
				return 1;

			if(this.ClusterKey is String keyAsString)
				return String.Compare(keyAsString, otherCluster.ClusterKey as String, StringComparison.CurrentCultureIgnoreCase);

			if(this.ClusterKey is IComparable keyAsComparable)
				return keyAsComparable.CompareTo(otherCluster.ClusterKey);

			return -1;
		}

		#endregion
	}
}