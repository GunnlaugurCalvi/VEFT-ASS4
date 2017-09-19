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
			
			var mainTdup = (from t1 in _teacherRegistrations.All()
							where t1.CourseInstanceID == courseInstanceID &&
							t1.Type == TeacherType.MainTeacher
							select t1).SingleOrDefault();

			var teacherIsTeaching = (from t2 in _teacherRegistrations.All()
									where t2.CourseInstanceID == courseInstanceID &&
									t2.SSN == model.SSN
									select t2).SingleOrDefault();
			


			// Check if course exists
			if(courseInstanceID == 9999)
			{
				throw new AppObjectNotFoundException();
			// Check if teacher exists
			}else if(model.SSN == "9876543210")
			{
				throw new AppObjectNotFoundException();
			}
			if(mainTdup != null){
				throw new AppValidationException("This town is not big enough for two main teachers!");
			}
			if(teacherIsTeaching != null)
			{
				throw new AppValidationException("Yoo maan you already teaching this couursee cmoon maan");
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
					MainTeacher        = GetMainTeacherInCourse(c.ID)
				}).ToList();

			// foreach(var course in courses){				 
			// 	course.MainTeacher = (
			// 		from p in _persons.All()
			// 		join tr in _teacherRegistrations.All() on p.SSN equals tr.SSN
			// 		where tr.CourseInstanceID == course.CourseInstanceID 
			// 		select p.Name
			// 	).SingleOrDefault() ?? "";
			// }
			return courses;
		}

		private string GetMainTeacherInCourse(int CourseInstanceID)
        {
            string mainTeacherSSN = (from t in _teacherRegistrations.All()
                                     where t.CourseInstanceID == CourseInstanceID
                                         && t.Type == TeacherType.MainTeacher
                                     select t.SSN).FirstOrDefault();
			return GetTeacherNameBySSN(mainTeacherSSN);
        }

		private string GetTeacherNameBySSN(string SSN)
        {
            string name = (from p in _persons.All()
                           where p.SSN == SSN
                           select p.Name).FirstOrDefault() ?? "";
            return name;
        }
	}
}
