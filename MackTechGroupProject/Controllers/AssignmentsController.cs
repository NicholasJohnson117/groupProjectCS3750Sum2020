﻿using MackTechGroupProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;


namespace MackTechGroupProject.Controllers
{
    public class AssignmentsController : BaseController
    {
        // GET: Assignments
        public ActionResult Assignment1()
        {
            return View();
        }
        public ActionResult Assignment2()
        {
            return View();
        }
        public ActionResult Assignment3()
        {
            return View();
        }
        public ActionResult Discussion()
        {
            return View();
        }

        public ActionResult SelectCourse()
        {
            String userId = User.Identity.GetUserId();

            var currentInstructorEnrollments = currentEnrollments.Select(c => c.Course).ToList();

            return View(currentInstructorEnrollments);
        }

        public ActionResult AddAssignment()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult AddAssignment(int id, Assignment model)
        {
            var selectedCourseId = id;

            var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var selectedCourse = context.Courses.Where(x => x.CourseId == selectedCourseId).FirstOrDefault();

            if (ModelState.IsValid)
            {
                var assignment = new Assignment
                {
                    AssignmentId = model.AssignmentId,
                    Course = selectedCourse,
                    Points = model.Points,
                    AssignmentTitle = model.AssignmentTitle,
                    AssignmentDescription = model.AssignmentDescription,
                    DueDate = model.DueDate,
                    SubmissionType = model.SubmissionType
                };

                context.Assignments.Add(assignment);
                context.SaveChanges();

                return RedirectToAction("Index", "Home");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult ViewAssignments(int id)
        {
            var selectedCourseId = id;
            var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var selectedCourse = context.Courses.Where(x => x.CourseId == selectedCourseId).Include(x => x.Assignments).FirstOrDefault();
            var currentAssignments = context.Assignments.Where(x => x.Course.CourseId == selectedCourseId).ToList();

            return View(currentAssignments);
        }



        public ActionResult ViewAllAssignments()
        {
            //var selectedCourseId = id;
            //var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            //var selectedCourse = context.Courses.Where(x => x.CourseId == selectedCourseId).Include(x => x.Assignments).FirstOrDefault();
            // var currentAssignments = context.Assignments.Where(x => x.Course.CourseId == selectedCourseId).ToList();

            var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
            var allEnrollments = context.Enrollments.Include(x => x.User).ToList();
            //var allCourses = context.Courses.Include(x => x.Assignments).Include(x => x.Enrollments).Include(x => x.Users).ToList();
            //var currentCourses = allCourses.Where(x => x.In)

            var currentAssignments = new List<Assignment>();



            /*
            foreach (Enrollment e in currentEnrollments)
            {
                currentCourses.Add(e.Course);
            }

            foreach (Course c in currentCourses)
            {

                Assignment a = context.Assignments.Where(x => x.Course.CourseId == c.CourseId).FirstOrDefault();
                if (a != null)
                {
                    currentAssignments.Add(a);
                }
            }
            */


            return View(currentAssignments);
        }

        public ActionResult AssignmentSubmission()
        {
            return View();
        }

        //private IEnumerable<SelectListItem> GetCourses()
        //{
        //    String userId = User.Identity.GetUserId();

        //    var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();

        //    var currentInstructorEnrollments = context.Enrollments.Include(x => x.User).Include(c => c.Course).Where(s => s.User.Id == userId).ToList();

        //    var instructorCourses = currentEnrollments.Select(x => x.Course).Select(x => new SelectListItem
        //                                                                                        {
        //                                                                                            Value = x.CourseId.ToString(),
        //                                                                                            Text = x.CourseName
        //                                                                                        });

        //    return new SelectList(instructorCourses, "Value", "Text");
        //}
    }
}