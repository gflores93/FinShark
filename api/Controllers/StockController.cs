using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Data;
using api.Dtos.Stock;
using api.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        public StockController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll() {
            var stocks = _context.Stocks.ToList()
            .Select(s => s.ToStockDto()); // Select method is the equivalent of a map in js
            return Ok(stocks);
        }

        [HttpGet("{id}")]
        public IActionResult GetById([FromRoute] int id) {
            var stock = _context.Stocks.Find(id);
            if (stock == null) {
                return NotFound();
            }
            else {
                return Ok(stock.ToStockDto());
            }
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateStockRequestDto stockDto) {
            var stockModel = stockDto.ToStockFromCreateDto();
            _context.Stocks.Add(stockModel);
            _context.SaveChanges();
            /*
            stockModel gets updated after the SQL insert, and as Id is defined as idenitity 
            after SaveChanges() stockModel gets an id value even though it was not assign by the ToStockFromCreateDto() method.

            CreatedAtAction method provides the URI of the method with the param passed in args1 and args2. written in response.header.location
            arg3 is the object response
            */
            return CreatedAtAction(nameof(GetById), new {id = stockModel.Id}, stockModel.ToStockDto());
        }
    }
}