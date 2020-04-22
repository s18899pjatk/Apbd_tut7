using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tutorial7.DTO;
using Tutorial7.Models;
using Tutorial7.Services;

namespace Tutorial7.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private string _connString = "Data Source=db-mssql;Initial Catalog=s18899;Integrated Security=True;MultipleActiveResultSets=True;";
        private readonly IConfiguration _configuration;

        public StudentsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            var students = new List<Student>();
            using var sqlConnection = new SqlConnection(_connString);
            using var command = new SqlCommand();
            command.Connection = sqlConnection;
            command.CommandText = "select s.IndexNumber, s.FirstName, s.LastName, s.BirthDate, st.Name as Studies, e.Semester " +
                "from Student s " +
                "join Enrollment e on e.IdEnrollment = s.IdEnrollment " +
                "join Studies st on st.IdStudy = e.IdStudy; ";
            sqlConnection.Open();
            SqlDataReader response = command.ExecuteReader();
            while (response.Read())
            {
                var st = new Student
                {
                    IndexNumber = response["IndexNumber"].ToString(),
                    FirstName = response["FirstName"].ToString(),
                    LastName = response["LastName"].ToString(),
                    Studies = response["Studies"].ToString(),
                    BirthDate = DateTime.Parse(response["BirthDate"].ToString()),
                    Semester = int.Parse(response["Semester"].ToString())

                };

                students.Add(st);
            }

            return Ok(students);
        }

        [HttpGet("entries/{id}")]
        public IActionResult GetSemester(string id)
        {
            using var sqlConnection = new SqlConnection(_connString);
            using var command = new SqlCommand();
            command.Connection = sqlConnection;
            command.CommandText = "select e.Semester " +
                "from Student s " +
                "join Enrollment e on e.IdEnrollment = s.IdEnrollment " +
                "where IndexNumber like @index;";
            SqlParameter par = new SqlParameter();
            par.ParameterName = "index";
            par.Value = id;
            command.Parameters.Add(par);
            sqlConnection.Open();
            SqlDataReader response = command.ExecuteReader();
            var entriesList = new List<string>();
            while (response.Read())
                entriesList.Add(response["Semester"].ToString());

            if (entriesList.Count > 0)
            {
                return Ok(entriesList);
            }
            else
            {
                return BadRequest(null);
            }
        }

        /*------------------------------------------------------------------ASSIGNMENT 7-------------------------------------------------------------------*/
        [Authorize]
        [HttpPost("login")]
        public IActionResult Login(LoginRequestDto requestDto)
        {

            // check credentials in db
            using (var sqlConnection = new SqlConnection(_connString))
            using (var command = new SqlCommand())
            {
                sqlConnection.Open();
                command.Connection = sqlConnection;
                command.CommandText = "SELECT IndexNumber, Password, pSalt FROM Student WHERE IndexNumber = @index";
                command.Parameters.AddWithValue("index", requestDto.Login);
                command.Parameters.AddWithValue("password", requestDto.Password);
                var dr = command.ExecuteReader();
                if (!dr.Read())
                {
                    return StatusCode(401, "Login is not correct");
                }
                var indexNum = dr["IndexNumber"].ToString();
                var lastName = dr["LastName"].ToString();
                var password = dr["Password"].ToString();
                var salt = dr["pSalt"].ToString();
                dr.Close();

                bool verify = PasswordHashing.Validate(requestDto.Password, salt, password);
                if (!verify)
                {
                    return StatusCode(401, "Password is not correct");
                }

                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, indexNum),
                new Claim(ClaimTypes.Name, lastName),
                new Claim(ClaimTypes.Role,  "Employee"),
                new Claim(ClaimTypes.Role,  "Student"),
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken
                (
                    issuer: "Artem",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                );

                var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                var refreshToken = Guid.NewGuid();
                command.CommandText = "UPDATE student SET RefreshToken = @rToken WHERE IndexNumber = @id";
                command.Parameters.AddWithValue("rToken", refreshToken);
                command.Parameters.AddWithValue("id", indexNum);
                var q = command.ExecuteNonQuery();

                return Ok(new
                {
                    accessToken,
                    refreshToken
                });
            }
        }

        [Authorize(Roles = "Employee")]
        [HttpPost("refresh-token/{rToken}")]
        public IActionResult RefreshToken(string rToken)
        {

            using (var sqlConnection = new SqlConnection(_connString))
            using (var command = new SqlCommand())
            {
                sqlConnection.Open();
                command.Connection = sqlConnection;
                command.CommandText = "SELECT IndexNumber,LastName, RefreshToken FROM Student WHERE RefreshToken like @reftoken";
                command.Parameters.AddWithValue("reftoken", rToken);
                var dr = command.ExecuteReader();
                if (!dr.Read())
                {
                    return StatusCode(401, "Such token does not exists in a database");
                }
                var indexNum = dr["IndexNumber"].ToString();
                var lastName = dr["LastName"].ToString();
                dr.Close();

                var claims = new[]
                 {
                    new Claim(ClaimTypes.NameIdentifier, indexNum),
                    new Claim(ClaimTypes.Name, lastName),
                    new Claim(ClaimTypes.Role,  "Employee"),
                    new Claim(ClaimTypes.Role,  "Student"),
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken
                (
                    issuer: "Artem",
                    audience: "Students",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(10),
                    signingCredentials: creds
                );

                var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
                var refreshToken = Guid.NewGuid();
                command.CommandText = "UPDATE student SET RefreshToken = @rToken WHERE IndexNumber = @id";
                command.Parameters.AddWithValue("rToken", refreshToken);
                command.Parameters.AddWithValue("id", indexNum);
                var q = command.ExecuteNonQuery();

                return Ok(new
                {
                    accessToken,
                    refreshToken
                });
            }
        }
    }
}
