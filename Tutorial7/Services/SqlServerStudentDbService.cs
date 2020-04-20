
using apbd_tut7.DTO.Requests;
using apbd_tut7.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Tutorial7.Models;

namespace Tutorial7.Services
{
    public class SqlServerStudentDbService : IStudentsSerivceDb
    {
        private string _connString = "Data Source=db-mssql;Initial Catalog=s18899;Integrated Security=True;MultipleActiveResultSets=True;";
        private int _countSt;

        /*------------------------------------------------------------------ASSIGNMENT 5-------------------------------------------------------------------*/
        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {
            EnrollStudentResponse response = new EnrollStudentResponse();

            using (SqlConnection con = new SqlConnection(_connString))
            {
                con.Open();
                var tran = con.BeginTransaction();
                int idStudy;
                int lastIdEnrollment;
                int semester;
                try
                {
                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.Transaction = tran;
                        com.CommandText = "SELECT IdEnrollment FROM Enrollment WHERE IdEnrollment = (SELECT MAX(e1.IdEnrollment) From Enrollment e1); "; // looking for the last Id 
                        var dr = com.ExecuteReader();
                        if (!dr.Read())
                        {
                            lastIdEnrollment = 1;
                        }
                        lastIdEnrollment = (int)dr["IdEnrollment"];
                        dr.Close();
                    }

                    using (SqlCommand com = new SqlCommand())
                    {
                        com.Connection = con;
                        com.Transaction = tran;
                        //checking whether we have such studies or no
                        com.CommandText = "SELECT IdStudy FROM Studies WHERE Name=@Name";
                        com.Parameters.AddWithValue("Name", request.Studies);
                        var dr = com.ExecuteReader();
                        if (!dr.Read())
                        {
                            //return BadRequest("Studies does not exists");
                            return null;
                        }
                        idStudy = (int)dr["idStudy"];
                        dr.Close();

                    };
                    using (SqlCommand com1 = new SqlCommand())
                    using (SqlCommand com2 = new SqlCommand())
                    {
                        com1.Connection = con;
                        com1.Transaction = tran;
                        com2.Connection = con;
                        com2.Transaction = tran;

                        //cheking whether we have the records with semester 1
                        com1.CommandText = "SELECT Semester FROM Enrollment where Semester=1 AND IdStudy = @idStudy";
                        com1.Parameters.AddWithValue("idStudy", idStudy);
                        var dr1 = com1.ExecuteReader();
                        // if no inserting new one
                        if (!dr1.Read())
                        {
                            semester = 1;
                            DateTime now = DateTime.Now;
                            com2.CommandText = "INSERT INTO Enrollment(IdEnrollment,Semester,IdStudy,StartDate) values (@IdEnrollment, @Semester, @IdStudy, @StartDate)";
                            com2.Parameters.AddWithValue("IdEnrollment", ++lastIdEnrollment);
                            com2.Parameters.AddWithValue("Semester", semester);
                            com2.Parameters.AddWithValue("idStudy", idStudy);
                            com2.Parameters.AddWithValue("StartDate", now.Date);
                            com2.ExecuteNonQuery();
                        }
                        else semester = (int)dr1["Semester"];
                        dr1.Close();
                    };
                    using (SqlCommand com1 = new SqlCommand())
                    using (SqlCommand com2 = new SqlCommand())
                    {
                        com1.Connection = con;
                        com1.Transaction = tran;
                        com2.Connection = con;
                        com2.Transaction = tran;

                        //checking whether we already have student with the same stud number
                        com1.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber LIKE @idStudent";
                        com1.Parameters.AddWithValue("idStudent", request.IndexNumber);
                        var dr2 = com1.ExecuteReader();
                        if (dr2.Read())
                        {
                            return null;
                            // return BadRequest("Student with such ID has already exists");
                        }
                        //Insert into student
                        com2.CommandText = "INSERT INTO Student(IndexNumber,FirstName, LastName, BirthDate, IdEnrollment) Values (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
                        com2.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                        com2.Parameters.AddWithValue("FirstName", request.FirstName);
                        com2.Parameters.AddWithValue("LastName", request.LastName);
                        com2.Parameters.AddWithValue("BirthDate", request.BirthDate);
                        com2.Parameters.AddWithValue("IdEnrollment", lastIdEnrollment);
                        com2.ExecuteNonQuery();
                        dr2.Close();
                    };
                    response.LastName = request.LastName;
                    response.Semester = semester;
                    tran.Commit();
                }
                catch (SqlException)
                {
                    tran.Rollback();
                }
            }
            return response;
        }




        public PromoteStudentResponse PromoteStudent(PromoteStudentRequest req)
        {
            PromoteStudentResponse response = new PromoteStudentResponse();
            using (SqlConnection con = new SqlConnection(_connString))
            {
                con.Open();
                using (SqlCommand com = new SqlCommand("PromoteStudents", con))
                {
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.Add(new SqlParameter("@StudyName", req.Studies));
                    com.Parameters.Add(new SqlParameter("@SEMESTER", req.Semester));
                    com.ExecuteNonQuery();
                }

                response.Studies = req.Studies;
                response.Semester = req.Semester + 1;

            }
            return response;
        }

        /*------------------------------------------------------------------ASSIGNMENT 6-------------------------------------------------------------------*/

        public bool idExists(string id)
        {

            using (SqlConnection con = new SqlConnection(_connString))
            using (SqlCommand com = new SqlCommand())
            {
                con.Open();
                com.Connection = con;
                com.CommandText = "SELECT COUNT(1) countSt FROM Student WHERE IndexNumber = @index";
                com.Parameters.AddWithValue("index", id);
                var dr = com.ExecuteReader();
                if (dr.Read())
                {
                    _countSt = int.Parse(dr["countSt"].ToString());
                }

                if (_countSt > 0)
                {
                    return true;
                }
                else return false;
            }
        }

        public void logIntoFile(string data)
        {
            var sw = new StreamWriter(@"requestsLog.txt");
            sw.WriteLine(data);
            sw.Close();
        }

    }
}
