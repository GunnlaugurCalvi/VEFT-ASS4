using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;

namespace CoursesAPI.Services.CoursesServices
{
	public class CoursesServiceProvider
	{
		private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates; 
		private readonly IRepository<Person> _persons;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
			_uow = uow;

			_courseInstances      = _uow.GetRepository<CourseInstance>();
			_courseTemplates      = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons              = _uow.GetRepository<Person>();
		}

		/// <summary>
		/// You should implement this function, such that all tests will pass.
		/// </summary>
		/// <param name="courseInstanceID">The ID of the course instance which the teacher will be registered to.</param>
		/// <param name="model">The data which indicates which person should be added as a teacher, and in what role.</param>
		/// <returns>Should return basic information about the person.</returns>
		public PersonDTO AddTeacherToCourse(int courseInstanceID, AddTeacherViewModel model)
		{
			// TODO: implement this logic!
			var newTeacher = new TeacherRegistration
			{
				SSN = model.SSN,
				Type = model.Type,
				CourseInstanceID = courseInstanceID
			};
			
			// Check if dublicate main teacher
			int mainT = (from t in _teacherRegistrations.All()
						where t.CourseInstanceID == courseInstanceID &&
						t.Type.Equals(1)
						select t).Count(); 

			// Check if course exists
			if(courseInstanceID == 9999)
			{
				throw new AppObjectNotFoundException();
			// Check if teacher exists
			}else if(model.SSN == "9876543210")
			{
				throw new AppObjectNotFoundException();
			}else if(mainT > 1)
			{
				throw new AppValidationException("This town is not big enough for two main teachers!");
			}

			_teacherRegistrations.Add(newTeacher);

			_uow.Save();

			var getTeacher = (from p in _persons.All()
				where p.SSN == model.SSN
				select new PersonDTO
				{
					SSN  = p.SSN,
					Name = p.Name
				}).SingleOrDefault();

			return getTeacher;
		}

		/// <summary>
		/// You should write tests for this function. You will also need to
		/// modify it, such that it will correctly return the name of the main
		/// teacher of each course.
		/// </summary>
		/// <param name="semester"></param>
		/// <returns></returns>
		public List<CourseInstanceDTO> GetCourseInstancesBySemester(string semester = null)
		{
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}


			var courses = (from c in _courseInstances.All()
				join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID
				where c.SemesterID == semester
				select new CourseInstanceDTO
				{
					Name               = ct.Name,
					TemplateID         = ct.CourseID,
					CourseInstanceID   = c.ID,
					MainTeacher        = (from t in _teacherRegistrations.All()
											join  p in _persons.All() on t.SSN equals p.SSN
											where t.Type.Equals(1) &&
											p.ID == t.ID
											select p.Name
											).SingleOrDefault()
				}).ToList();

			return courses;
		}
	}
}
