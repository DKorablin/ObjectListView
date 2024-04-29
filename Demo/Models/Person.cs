using System;
using System.ComponentModel;
using System.Drawing;
using BrightIdeasSoftware;
using ObjectListViewDemo.Properties;

namespace ObjectListViewDemo.Models
{
	public class Person : INotifyPropertyChanged
	{
		public Boolean IsActive = true;

		public Person(String name)
			=> this.name = name;

		public Person(String name, String occupation, Int32 culinaryRating, DateTime birthDate, Double hourlyRate, Boolean canTellJokes, String photo, String comments)
		{
			this.name = name;
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
			this.name = other.Name;
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
		public Image ImageAspect { get => Resource.folder16; }

		[OLVIgnore]
		public String ImageName { get => "user"; }

		// Allows tests for properties.
		[OLVColumn(ImageAspectName = "ImageName")]
		public String Name
		{
			get => this.name;
			set
			{
				if(this.name == value) return;
				this.name = value;
				this.OnPropertyChanged("Name");
			}
		}
		private String name;

		[OLVColumn(ImageAspectName = "ImageName")]
		public String Occupation
		{
			get => this.occupation;
			set
			{
				if(this.occupation == value) return;
				this.occupation = value;
				this.OnPropertyChanged("Occupation");
			}
		}
		private String occupation;

		public Int32 CulinaryRating { get; set; }

		public DateTime BirthDate { get; set; }

		public Int32 YearOfBirth
		{
			get => this.BirthDate.Year;
			set => this.BirthDate = new DateTime(value, this.BirthDate.Month, this.BirthDate.Day);
		}

		// Allow tests for methods
		public Double GetRate() => this._hourlyRate;
		private Double _hourlyRate;

		public void SetRate(Double value)
			=> this._hourlyRate = value;

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