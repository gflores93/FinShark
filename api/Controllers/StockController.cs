using api.Dtos.Stock;
using api.Interfaces;
using api.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockRepository _stockRepo;
        public StockController(IStockRepository stockRepo)
        {
            _stockRepo = stockRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() {
            var stocks = await _stockRepo.GetAllAsync();
            var stockDto = stocks.Select(s => s.ToStockDto()); // Select method is the equivalent of a map in js
            return Ok(stockDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id) {
            var stock = await _stockRepo.GetByIdAsync(id);
            if (stock == null) {
                return NotFound();
            }
            else {
                return Ok(stock.ToStockDto());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStockRequestDto stockDto) {
            var stockModel = stockDto.ToStockFromCreateDto();
            await _stockRepo.CreateAsync(stockModel);
            // stockModel gets the new Id assigned when SaveChanges is executed
            // there is no need to store the updated stockModel as it an object (reference type)
            // CreatedAtAction use arg1 and arg2 for the URI (response.header.location) and arg3 as the return obj
            return CreatedAtAction(nameof(GetById), new {id = stockModel.Id}, stockModel.ToStockDto());
        }

        [HttpPut]
        [Route ("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockRequestDto updateDto) {
            var stockModel = await _stockRepo.UpdateAsync(id, updateDto);
            if(stockModel == null) {
                return NotFound();
            }
       
            return Ok(stockModel.ToStockDto());
        }

        [HttpDelete]
        [Route ("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id) {
            var stockModel = await _stockRepo.DeleteAsync(id);
            if (stockModel == null) {
                return NotFound();
            }
 
            return NoContent();
        }
    }
}