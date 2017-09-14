using System;
using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;
using CoursesAPI.Services.CoursesServices;
using CoursesAPI.Tests.MockObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoursesAPI.Tests.Services
{
	[TestClass]
    public class CourseServicesTests
	{
		private MockUnitOfWork<MockDataContext> _mockUnitOfWork;
		private CoursesServiceProvider _service;
		private List<TeacherRegistration> _teacherRegistrations;

		private const string SSN_DABS    = "1203735289";
		private const string SSN_GUNNA   = "1234567890";
		private const string SSN_ARNOR   = "1601872989";
		private const string SSN_GULLI   = "1707952889";

		private const string INVALID_SSN = "9876543210";

		private const string NAME_GUNNA  = "Guðrún Guðmundsdóttir";
		private const string NAME_ARNOR  = "Arnór Þrastarson";
		private const string NAME_GULLI  = "Gunnlaugur Kristinn Hreidarsson";

		private const int COURSEID_VEFT_20153 = 1337;
		private const int COURSEID_VEFT_20163 = 1338;
		private const int INVALID_COURSEID    = 9999;

		[TestInitialize]
        public void CourseServicesTestsSetup()
		{
			_mockUnitOfWork = new MockUnitOfWork<MockDataContext>();

			#region Persons
			var persons = new List<Person>
			{
				// Of course I'm the first person,
				// did you expect anything else?
				new Person
				{
					ID    = 1,
					Name  = "Daníel B. Sigurgeirsson",
					SSN   = SSN_DABS,
					Email = "dabs@ru.is"
				},
				new Person
				{
					ID    = 2,
					Name  = NAME_GUNNA,
					SSN   = SSN_GUNNA,
					Email = "gunna@ru.is"
				},
				new Person
				{
					ID    = 3,
					Name  = NAME_ARNOR,
					SSN   = SSN_ARNOR,
					Email = "arnor15@ru.is"
				},
				new Person
				{
					ID 		= 4,
					Name 	= NAME_GULLI,
					SSN		= SSN_GULLI,
					Email 	= "Gunnlaugur15@ru.is"
				}
			};
			#endregion

			#region Course templates

			var courseTemplates = new List<CourseTemplate>
			{
				new CourseTemplate
				{
					CourseID    = "T-514-VEFT",
					Description = "Í þessum áfanga verður fjallað um vefþj...",
					Name        = "Vefþjónustur"
				}
			};
			#endregion

			#region Courses
			var courses = new List<CourseInstance>
			{
				new CourseInstance
				{
					ID         = COURSEID_VEFT_20153,
					CourseID   = "T-514-VEFT",
					SemesterID = "20153"
				},
				new CourseInstance
				{
					ID         = COURSEID_VEFT_20163,
					CourseID   = "T-514-VEFT",
					SemesterID = "20163"
				}
			};
			#endregion

			#region Teacher registrations
			_teacherRegistrations = new List<TeacherRegistration>
			{
				new TeacherRegistration
				{
					ID               = 101,
					CourseInstanceID = COURSEID_VEFT_20153,
					SSN              = SSN_DABS,
					Type             = TeacherType.MainTeacher
				},
				new TeacherRegistration
				{
					ID               = 102,
					CourseInstanceID = COURSEID_VEFT_20153,
					SSN              = SSN_DABS,
					Type             = TeacherType.MainTeacher
				}
			};
			#endregion

			_mockUnitOfWork.SetRepositoryData(persons);
			_mockUnitOfWork.SetRepositoryData(courseTemplates);
			_mockUnitOfWork.SetRepositoryData(courses);
			_mockUnitOfWork.SetRepositoryData(_teacherRegistrations);

			// TODO: this would be the correct place to add 
			// more mock data to the mockUnitOfWork!

			_service = new CoursesServiceProvider(_mockUnitOfWork);
		}

		#region GetCoursesBySemester
		/// <summary>
		/// TODO: implement this test, and several others!
		/// </summary>
		[TestMethod]
		public void GetCoursesBySemester_ReturnsEmptyListWhenNoDataDefined()
		{
			// Arrange:

			// Act:

			// Assert:
			// Assert.True(true);
		}

		// TODO!!! you should write more unit tests here!

		#endregion

		#region AddTeacher

		/// <summary>
		/// Adds a main teacher to a course which doesn't have a
		/// main teacher defined already (see test data defined above).
		/// </summary>
		[TestMethod]
		public void AddTeacher_WithValidTeacherAndCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			var prevCount = _teacherRegistrations.Count;

			// Act:
			var dto = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model);

			// Assert:

			// Check that the dto object is correctly populated:
			Assert.AreEqual(SSN_GUNNA, dto.SSN);
			Assert.AreEqual(NAME_GUNNA, dto.Name);

			// Ensure that a new entity object has been created:
			var currentCount = _teacherRegistrations.Count;
			Assert.AreEqual(prevCount + 1, currentCount);

			// Get access to the entity object and assert that
			// the properties have been set:
			var newEntity = _teacherRegistrations.Last();
			Assert.AreEqual(COURSEID_VEFT_20163, newEntity.CourseInstanceID);
			Assert.AreEqual(SSN_GUNNA, newEntity.SSN);
			Assert.AreEqual(TeacherType.MainTeacher, newEntity.Type);
			
			// Ensure that the Unit Of Work object has been instructed
			// to save the new entity object:
			Assert.IsTrue(_mockUnitOfWork.GetSaveCallCount() > 0);
		}

        [TestMethod]
		[ExpectedException(typeof(AppObjectNotFoundException))]
		public void AddTeacher_InvalidCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.AssistantTeacher
			};
			var prevCount = _teacherRegistrations.Count;

			// Act:
				var dto = _service.AddTeacherToCourse(INVALID_COURSEID, model);
				
				// Assert:

				// Check that the dto object is correctly populated:
				Assert.AreEqual(SSN_GUNNA, dto.SSN);
				Assert.AreEqual(NAME_GUNNA, dto.Name);

				// Get access to the entity object and assert that
				// the properties have been set:
				var newEntity = _teacherRegistrations.Last();
				Assert.AreEqual(INVALID_COURSEID, newEntity.CourseInstanceID);

				// Ensure that the Unit Of Work object has been instructed
				// to save the new entity object:
				Assert.IsTrue(_mockUnitOfWork.GetSaveCallCount() > 0);
		}

		/// <summary>
		/// Ensure it is not possible to add a person as a teacher
		/// when that person is not registered in the system.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof (AppObjectNotFoundException))]
		public void AddTeacher_InvalidTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = INVALID_SSN,
				Type = TeacherType.MainTeacher
			};

			// Act: 
			var dto = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model);
				
			// Assert:

			// Check that the dto object is correctly populated:
			Assert.AreEqual(SSN_GUNNA, dto.SSN);
			Assert.AreEqual(NAME_GUNNA, dto.Name);

			// Get access to the entity object and assert that
			// the properties have been set:
			var newEntity = _teacherRegistrations.Last();
			Assert.AreEqual(COURSEID_VEFT_20163, newEntity.CourseInstanceID);

			// Ensure that the Unit Of Work object has been instructed
			// to save the new entity object:
			Assert.IsTrue(_mockUnitOfWork.GetSaveCallCount() > 0);
		}

		/// <summary>
		/// In this test, we test that it is not possible to
		/// add another main teacher to a course, if one is already
		/// defined.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof (AppValidationException))]
		public void AddTeacher_AlreadyWithMainTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_ARNOR,
				Type = TeacherType.MainTeacher
			};

			var model2 = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			

			// Act:

			var dto = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model);
			var dto2 = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model2);

				
			// Assert:

			// Check that the dto object is correctly populated:
			Assert.AreEqual(SSN_GUNNA, dto2.SSN);
			Assert.AreEqual(NAME_GUNNA, dto2.Name);
			Assert.AreEqual(SSN_ARNOR, dto.SSN);
			Assert.AreEqual(NAME_ARNOR, dto.Name);

			// Get access to the entity object and assert that
			// the properties have been set:
			var newEntity = _teacherRegistrations.First();

			Assert.IsNull(newEntity.Type);
			// Ensure that the Unit Of Work object has been instructed
			// to save the new entity object:
			Assert.IsTrue(_mockUnitOfWork.GetSaveCallCount() > 0);


		}

		/// <summary>
		/// In this test, we ensure that a person cannot be added as a
		/// teacher in a course, if that person is already registered
		/// as a teacher in the given course.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof(AppValidationException))]
		public void AddTeacher_PersonAlreadyRegisteredAsTeacherInCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_DABS,
				Type = TeacherType.AssistantTeacher
			};

			// Act:
			var dto = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model);

			// Assert:

			Assert.AreEqual(SSN_DABS, dto.SSN);
			Assert.AreEqual("Daníel B. Sigurgeirsson", dto.Name);

			var newEntity = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model);


			Assert.IsNull(newEntity);
			Assert.IsTrue(_mockUnitOfWork.GetSaveCallCount() > 0);

		}

		#endregion
	}
}
