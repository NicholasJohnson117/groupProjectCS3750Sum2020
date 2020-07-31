﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using Microsoft.AspNet.SignalR;
using MackTechGroupProject.Models;
using Microsoft.AspNet.Identity.Owin;
using System.Data.Entity;

namespace MackTechGroupProject
{
    public class NotificationComponents
    {
        //here we will add a function for register notification (will add sql dependency)
        public Boolean UpdatedAssignmentGradeNotification(DateTime currentTime)
        {
            Boolean result = false;
            string conStr = ConfigurationManager.ConnectionStrings["TitanDbConnection"].ConnectionString;
            //string sqlCommand = @"SELECT [Grade] from [dbo].[SubmissionGrades] where [GradeAddedOn] > @GradeAddedOn";
            //string sqlCommand = @"SELECT AssignmentTitle, Points, Grade from [dbo].[SubmissionGrades] sg INNER JOIN [dbo].[Assignments] a
            //                        ON sg.Assignment_AssignmentId = a.AssignmentId
            //                        where sg.GradeAddedOn > @GradeAddedOn";
            string sqlCommand = @"SELECT Id from [dbo].[SubmissionGrades]
                                    where GradeAddedOn > @GradeAddedOn";
            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand(sqlCommand, con);
                cmd.Parameters.AddWithValue("@GradeAddedOn", currentTime);
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                cmd.Notification = null;
                SqlDependency sqlDep = new SqlDependency(cmd);
                if (sqlDep.HasChanges)
                {
                    result = true;
                }
                sqlDep.OnChange += new OnChangeEventHandler(sqlDep1_OnChange);
                //sqlDep.OnChange += sqlDep_OnChange;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    //nothing need to add here
                }



            }

            return result;
        }

        public void AddedAssignmentNotification(DateTime currentTime)
        {
            string conStr = ConfigurationManager.ConnectionStrings["TitanDbConnection"].ConnectionString;
            //string sqlCommand = @"SELECT [Grade] from [dbo].[SubmissionGrades] where [GradeAddedOn] > @GradeAddedOn";
            string sqlCommand = @"SELECT AssignmentTitle, Points, DueDate 
                                    from [dbo].[Assignments]
                                    where AssignmentAddedOn > @AssignmentAddedOn";
            using (SqlConnection con = new SqlConnection(conStr))
            {
                SqlCommand cmd = new SqlCommand(sqlCommand, con);
                cmd.Parameters.AddWithValue("@AssignmentAddedOn", currentTime);
                if (con.State != System.Data.ConnectionState.Open)
                {
                    con.Open();
                }
                cmd.Notification = null;
                SqlDependency sqlDep = new SqlDependency(cmd);
                sqlDep.OnChange += new OnChangeEventHandler(sqlDep2_OnChange);
                //sqlDep.OnChange += sqlDep_OnChange;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    //nothing need to add here
                }

            }
        }

        void sqlDep1_OnChange(Object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep1_OnChange;

                //from here we will send message to client
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                notificationHub.Clients.All.Notify("added");

                //re-register notifcation
                UpdatedAssignmentGradeNotification(DateTime.Now);

            }
        }

        void sqlDep2_OnChange(Object sender, SqlNotificationEventArgs e)
        {
            if (e.Type == SqlNotificationType.Change)
            {
                SqlDependency sqlDep = sender as SqlDependency;
                sqlDep.OnChange -= sqlDep2_OnChange;

                //from here we will send message to client
                var notificationHub = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
                notificationHub.Clients.All.Notify("added");

                //re-register notifcation
                AddedAssignmentNotification(DateTime.Now);

            }
        }

        public List<StudentNotificationViewModel> GetNotificationData(DateTime afterDate, string userId)
        {

            using (ApplicationDbContext context = new ApplicationDbContext())
            {

                List<StudentNotificationViewModel> result = new List<StudentNotificationViewModel>();

                //get newly graded submissions per user
                var updatedSubmissions = context.SubmissionGrades.Where(x => x.GradeAddedOn > afterDate).Include(x => x.Assignment).Include(x => x.User).OrderByDescending(x => x.GradeAddedOn).ToList();
                var updatedSubmissionsForUser = updatedSubmissions.Where(x => x.User.Id == userId).ToList();
                var updatedSubmissionsForUserWithGrade = updatedSubmissionsForUser.Where(x => x.Grade != null).ToList();

                //get newly added assignements per user 
                var currentEnrollmentsWithAssignments = context.Enrollments.Where(x => x.User.Id == userId).Include(x => x.User).Include(x => x.Course).Include("Course.Assignments").ToList();
                var allAssignments = currentEnrollmentsWithAssignments.Select(x => x.Course).SelectMany(y => y.Assignments).ToList();
                var recentlyAddedAssignments = allAssignments.Where(x => x.AssignmentAddedOn > afterDate).ToList();

                //add newly graded submissions to the list
                foreach (SubmissionGrades sg in updatedSubmissionsForUserWithGrade)
                {

                    StudentNotificationViewModel newGradedNotification = new StudentNotificationViewModel()
                    {
                        AssignmentTitle = sg.Assignment.AssignmentTitle,
                        Grade = (double)sg.Grade,
                        Points = sg.Assignment.Points,
                        DueDate = null,
                        Department = sg.Assignment.Course.Department,
                        CourseNumber = sg.Assignment.Course.CourseNumber
                    };
                    result.Add(newGradedNotification);

                }

                //add newly added assignments to the same list
                foreach (Assignment a in recentlyAddedAssignments)
                {
                    StudentNotificationViewModel newAddedAssignment = new StudentNotificationViewModel()
                    {
                        AssignmentTitle = a.AssignmentTitle,
                        Grade = 0,
                        Points = a.Points,
                        DueDate = a.DueDate,
                        Department = a.Course.Department,
                        CourseNumber = a.Course.CourseNumber
                    };
                    result.Add(newAddedAssignment);

                }

                //return the list
                return result;
            }

        }

    }
}