/*
 * [File purpose]
 * Author: Phillip Piper
 * Date: 10/21/2008 11:09 PM
 * 
 * CHANGE LOG:
 * when who what
 * 10/21/2008 JPP  Initial Version
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace BrightIdeasSoftware.Tests
{
	/// <summary>Description of Person.</summary>

	public class Person : INotifyPropertyChanged, ITreeModel
	{
		public bool? IsActive = null;

		public Person(String name)
			=> this._name = name;

		public Person(String name, String occupation, int culinaryRating, DateTime birthDate, double hourlyRate, bool canTellJokes, String photo, String comments)
		{
			this._name = name;
			this.Occupation = occupation;
			this._culinaryRating = culinaryRating;
			this.birthDate = birthDate;
			this._hourlyRate = hourlyRate;
			this.CanTellJokes = canTellJokes;
			this.Comments = comments;
			this.Photo = photo;
		}

		public Person(Person other)
		{
			this._name = other.Name;
			this.Occupation = other.Occupation;
			this._culinaryRating = other.CulinaryRating;
			this.birthDate = other.BirthDate;
			this._hourlyRate = other.GetRate();
			this.CanTellJokes = other.CanTellJokes;
			this.Photo = other.Photo;
			this.Comments = other.Comments;
			this.Parent = other.Parent;
		}

		public Person Parent
		{
			get => _parent;
			set
			{
				if(_parent == value) return;
				_parent = value;
				this.OnPropertyChanged("Parent");
			}
		}
		private Person _parent;

		// Allows tests for properties.
		public String Name
		{
			get => _name;
			set
			{
				if(_name == value) return;
				_name = value;
				this.OnPropertyChanged("Name");
			}
		}
		private String _name;

		public String Occupation
		{
			get => _occupation;
			set
			{
				if(_occupation == value) return;
				_occupation = value;
				this.OnPropertyChanged("Occupation");
			}
		}
		private String _occupation;

		public Int32 CulinaryRating
		{
			get => _culinaryRating;
			set
			{
				if(_culinaryRating == value) return;
				_culinaryRating = value;
				this.OnPropertyChanged("CulinaryRating");
			}
		}
		private Int32 _culinaryRating;

		public DateTime BirthDate
		{
			get => birthDate;
			set
			{
				if(birthDate == value) return;
				birthDate = value;
				this.OnPropertyChanged("BirthDate");
			}
		}
		private DateTime birthDate;

		public int YearOfBirth
		{
			get => this.BirthDate.Year;
			set => this.BirthDate = new DateTime(value, birthDate.Month, birthDate.Day);
		}

		// Allows tests for methods
		virtual public double GetRate()
			=> _hourlyRate;

		private double _hourlyRate;

		public void SetRate(double value)
			=> _hourlyRate = value;

		// Allow tests on trees
		public IList<Person> Children
		{
			get => _children;
			set => _children = value;
		}
		private IList<Person> _children = new List<Person>();

		public void AddChild(Person child)
		{
			Children.Add(child);
			child.Parent = this;
		}

		// Allows tests for fields.
		public String Photo;
		public String Comments;
		public bool CanTellJokes;

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String propertyName)
			=> this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public int CountNotifyPropertyChangedSubscriptions
		{
			get => this.PropertyChanged == null ? 0 : this.PropertyChanged.GetInvocationList().Length;
		}

		#endregion

		#region Equality members

		protected bool Equals(Person other)
			=> String.Equals(_name, other._name);

		public override bool Equals(Object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			Person other = obj as Person;
			return other != null && Equals(other);
		}

		public override Int32 GetHashCode()
			=> (_name != null ? _name.GetHashCode() : 0);

		#endregion

		#region Implementation of ITreeModel

		// These are used when the TreeListView doesn't have getter delegates installed
		public bool TreeCanExpand
		{
			get => Children.Count > 0;
		}
		public IEnumerable TreeChildren
		{
			get => Children;
		}
		public Object TreeParent
		{
			get => Parent;
		}

		#endregion
	}

	// Model class for testing virtual and overridden methods

	public class Person2 : Person
	{
		public Person2(String name, String occupation, int culinaryRating, DateTime birthDate, double hourlyRate, bool canTellJokes, String photo, String comments)
			: base(name, occupation, culinaryRating, birthDate, hourlyRate, canTellJokes, photo, comments)
		{
		}

		public override double GetRate()
		{
			return base.GetRate() * 2;
		}

		new public int CulinaryRating
		{
			get => base.CulinaryRating * 2;
			set { base.CulinaryRating = value; }
		}
	}

	public static class PersonDb
	{
		static void InitializeAllPersons()
		{
			sAllPersons = new List<Person>(new Person[] {
				new Person("name", "occupation", 300, DateTime.Now.AddYears(1), 1.0, true, "  photo  ", "comments"),
				new Person2("name2", "occupation", 100, DateTime.Now, 1.0, true, "  photo  ", "comments"),
				new Person(PersonDb.FirstAlphabeticalName, "occupation3", 90, DateTime.Now, 3.0, true, "  photo3  ", "comments3"),
				new Person("name4", "occupation4", 80, DateTime.Now, 4.0, true, "  photo4  ", "comments4"),
				new Person2("name5", "occupation5", 70, DateTime.Now, 5.0, true, "  photo5  ", "comments5"),
				new Person("name6", "occupation6", 65, DateTime.Now, 6.0, true, "  photo6  ", "comments6"),
				new Person("name7", "occupation7", 62, DateTime.Now, 7.0, true, "  photo7  ", "comments7"),
				new Person(PersonDb.LastAlphabeticalName, "occupation6", 60, DateTime.Now.AddYears(-1), 6.0, true, "  photo6  ", LastComment),
			});
			sAllPersons[0].AddChild(sAllPersons[2]);
			sAllPersons[0].AddChild(sAllPersons[3]);
			sAllPersons[1].AddChild(sAllPersons[4]);
			sAllPersons[1].AddChild(sAllPersons[5]);
			sAllPersons[5].AddChild(sAllPersons[6]);
			sAllPersons[6].AddChild(sAllPersons[7]);
		}
		static private List<Person> sAllPersons;

		static public List<Person> All
		{
			get
			{
				if(sAllPersons == null)
					InitializeAllPersons();
				return sAllPersons;
			}
		}

		static public void Reset()
			=> sAllPersons = null;

		static public String FirstAlphabeticalName
		{
			get => "aaa First Alphabetical Name";
		}

		static public String LastAlphabeticalName
		{
			get => "zzz Last Alphabetical Name";
		}

		static public String LastComment
		{
			get => "zzz Last Comment";
		}
	}
}