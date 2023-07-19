using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class TestController : ControllerBase
    {
        // https://localhost:5001/test
        [HttpGet("test")]
        public ActionResult<string> Get()
        {
            return "Hello World";
        } 
    }
}