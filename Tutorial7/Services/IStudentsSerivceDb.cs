using apbd_tut7.DTO.Requests;
using apbd_tut7.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tutorial7.Models;

namespace Tutorial7.Services
{
    public interface IStudentsSerivceDb
    {
   
        //------------------------------------------5th assignment-------------------------------//
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest req);

        PromoteStudentResponse PromoteStudent(PromoteStudentRequest req);
        //------------------------------------------6th assignment-------------------------------//
        bool idExists(string id);

        void logIntoFile(string data);
    }
}
