using System;
using System.ComponentModel;
using System.Drawing;
using BrightIdeasSoftware;

namespace ObjectListViewDemo
{

	public enum MaritalStatus
	{
		Single,
		Married,
		Divorced,
		Partnered
	}

	public class Person : INotifyPropertyChanged
	{
		public Boolean IsActive = true;

		public Person(String name)
			=> this._name = name;

		public Person(String name, String occupation, Int32 culinaryRating, DateTime birthDate, Double hourlyRate, Boolean canTellJokes, String photo, String comments)
		{
			this._name = name;
			this.Occupation = occupation;
			this.CulinaryRating = culinaryRating;
			this.BirthDate = birthDate;
			this._hourlyRate = hourlyRate;
			this.CanTellJokes = canTellJokes;
			this.Comments = comments;
			this.Photo = photo;
		}

		public Person(Person other)
		{
			this._name = other.Name;
			this.Occupation = other.Occupation;
			this.CulinaryRating = other.CulinaryRating;
			this.BirthDate = other.BirthDate;
			this._hourlyRate = other.GetRate();
			this.CanTellJokes = other.CanTellJokes;
			this.Photo = other.Photo;
			this.Comments = other.Comments;
			this.MaritalStatus = other.MaritalStatus;
		}

		[OLVIgnore]
		public Image ImageAspect { get; } = Resource.folder16;

		[OLVIgnore]
		public String ImageName { get; } = "user";

		// Allows tests for properties.
		[OLVColumn(ImageAspectName = "ImageName")]
		public String Name
		{
			get => this._name;
			set
			{
				if(this._name == value) return;
				this._name = value;
				this.OnPropertyChanged(nameof(this.Name));
			}
		}
		private String _name;

		[OLVColumn(ImageAspectName = "ImageName")]
		public String Occupation
		{
			get => this._occupation;
			set
			{
				if(this._occupation == value) return;
				this._occupation = value;
				this.OnPropertyChanged(nameof(this.Occupation));
			}
		}
		private String _occupation;

		public Int32 CulinaryRating { get; set; }

		public DateTime BirthDate { get; set; }

		public Int32 YearOfBirth
		{
			get => this.BirthDate.Year;
			set => this.BirthDate = new DateTime(value, this.BirthDate.Month, this.BirthDate.Day);
		}

		// Allow tests for methods
		public Double GetRate()
			=> this._hourlyRate;
		public void SetRate(Double value)
			=> this._hourlyRate = value;

		private Double _hourlyRate;

		// Allows tests for fields.
		public String Photo;
		public String Comments;
		public Int32 serialNumber;
		public Boolean? CanTellJokes;

		// Allow tests for enums
		public MaritalStatus MaritalStatus = MaritalStatus.Single;

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String propertyName)
				=> this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		#endregion
	}
}