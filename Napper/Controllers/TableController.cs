using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

using Napper.Service;

using System.Text.Json;

namespace Napper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController(NapperService NapperService) : ControllerBase
    {
        private readonly NapperService _napperService = NapperService;

        // GET: api/<TableController>
        [HttpGet]
        public IActionResult Get()
        {
            var dataList = _napperService.TableAllQuery();

            return (dataList == null) ? NotFound(_napperService.Message) : Ok(dataList);

        }

        // GET api/<TableController>/Users
        [HttpGet("{tablename}")]
        public IActionResult Get(string tablename)
        {
            string queryString = Uri.UnescapeDataString(Request.QueryString.Value ?? "");
            if (!QueryFomatter.TryUrlQueryStringToDictionary(queryString, out var parameters))
            {
                return BadRequest("QueryFomatter Error.");
            }

            var dataList = _napperService.SelectQuery(tablename, parameters);

            return (dataList == null) ? NotFound(_napperService.Message) : Ok(dataList);
        }

        // POST api/<TableController>/Users
        [HttpPost("{tablename}")]
        public IActionResult Post(string tablename, [FromBody] JsonElement element)
        {
            if (!QueryFomatter.TryJsonElementArrayToDictinary(element, out var parameters))
            {
                return BadRequest("QueryFomatter Error.");
            }

            if (parameters == null)
            {
                return BadRequest("parameters Error.");
            }

            return _napperService.InsertQuery(tablename, parameters.First()) ?  Ok() : NotFound(_napperService.Message);
        }

        // PUT api/<TableController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
            throw new NotSupportedException();
        }

        // DELETE api/<TableController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            throw new NotSupportedException();
        }
    }
}
