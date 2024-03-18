using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using VfxApi.Models;
using System.Text.Json;
using Newtonsoft.Json.Linq;


namespace VfxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExchangeRatesController : ControllerBase
    {
        private readonly VfxContext _context;
    

        

        public ExchangeRatesController(VfxContext context)
        {
            _context = context;
            
         
        }   


        /// <summary>
        /// Gets all the ExchangeRates in the database.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExchangeRate>>> GetExchangeRates()
        {
            return await _context.ExchangeRates.ToListAsync();
        }

        /// <summary>
        /// Gets a ExchangeRate with an Id 
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("ById/{id}")]
        public async Task<ActionResult<ExchangeRate>> GetExchangeRateById(int id)
        {
            var exchangeRate = await _context.ExchangeRates.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (exchangeRate == null)
            {
                return NotFound();
            }

            return exchangeRate;
        }

        /// <summary>
        /// Gets a ExchangeRate with the name of the Exchange Rate in the database, if it does not exists goes to an external API. 
        /// </summary>
        [HttpGet("ByName")]
        public async Task<ActionResult<ExchangeRate>> GetExchangeRateByName(string currencyPairfrom, string currencyPairTo)
        {
            var exchangeRate = await _context.ExchangeRates.Where(x=>x.CurrencyPairFrom.ToUpper() == currencyPairfrom.ToUpper() && x.CurrencyPairTo.ToUpper() == currencyPairTo.ToUpper()).FirstOrDefaultAsync(); //Checks if the Exchange pair exists in the database

            if (exchangeRate == null)
            {
                string QUERY_URL = "https://www.alphavantage.co/query?function=CURRENCY_EXCHANGE_RATE&from_currency="+ currencyPairfrom + "&to_currency="+ currencyPairTo + "&apikey=6CZ0007BTGAKMZZP"; //If it does not exists he uses this URL to request the information on the API
                Uri queryUri = new Uri(QUERY_URL);

                using (WebClient client = new WebClient())
                {                        
                    JObject jsonObj = JObject.Parse(client.DownloadString(queryUri)); //Gets the information as a JSON from the API 
                    if (jsonObj != null)
                    {

                        var realTimeCurrencyExchangeRate = (JObject)jsonObj["Realtime Currency Exchange Rate"]; //Contains the information about the desired Exchange Rate

                        if (realTimeCurrencyExchangeRate != null)
                        {
                            //Gets all the information about the from, to, bid and ask
                            var from = (string)realTimeCurrencyExchangeRate["1. From_Currency Code"];
                            var to = (string)realTimeCurrencyExchangeRate["3. To_Currency Code"];
                            var bid = (decimal)realTimeCurrencyExchangeRate["8. Bid Price"];
                            var ask = (decimal)realTimeCurrencyExchangeRate["9. Ask Price"];

                            if(!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to) && bid!=null && ask != null)
                            {
                                ExchangeRate newExchangeRate = new ExchangeRate() // If all the four variable above have a value it creates a new ExchangeRate
                                {
                                    CurrencyPairFrom = from,
                                    CurrencyPairTo = to,
                                    Bid = bid,
                                    Ask = ask
                                };

                                _context.ExchangeRates.Add(newExchangeRate); //Adds the new ExchangeRate in the database
                                await _context.SaveChangesAsync();

                                return newExchangeRate;
                            }
                        }
                        else if (jsonObj.First.ToString().Contains("Error")) //If the JsonObject that we receive from the external API contains an error , it returns with a message saying that the Exchange Rate does not Exist.
                        {
                            var statusCode = HttpStatusCode.NotFound;
                            var message = "The currency that you are trying to find does not exists.";
                            return StatusCode((int)statusCode, message);
                        }

                    }
                                      
                }        
            
            }

            return exchangeRate;
        }

        /// <summary>
        /// Edits an Exchange Rate.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutExchangeRate(int id, ExchangeRate exchangeRate)
        {
            if (id != exchangeRate.Id)
            {
                return BadRequest();
            }

            _context.Entry(exchangeRate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExchangeRateExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Adds an Exchange Rate.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost()]
        public async Task<IActionResult> PostExchangeRate(ExchangeRate exchangeRate)
        {
            var statusCode = HttpStatusCode.OK;
            var message = "";

            var rates = await _context.ExchangeRates.ToListAsync();
            foreach(var rate in rates)
            {
                if(rate.CurrencyPairTo.ToUpper() == exchangeRate.CurrencyPairTo.ToUpper() && (rate.CurrencyPairFrom.ToUpper() == exchangeRate.CurrencyPairFrom.ToUpper())) //checks if the Exchange Pair already exists
                {
                    statusCode = HttpStatusCode.UnprocessableContent;
                    message = "The currency that you are trying to add already exists.";
                    return StatusCode((int)statusCode, message);
                }
            }
            _context.ExchangeRates.Add(exchangeRate); //If it does not exists it adds on the database
            await _context.SaveChangesAsync();

            //return CreatedAtAction(nameof(GetExchangeRate), new { id = exchangeRate.Id }, exchangeRate);

            statusCode = HttpStatusCode.OK;
            message = "Currency added with success!";
            return StatusCode((int)statusCode, message);
        }

        /// <summary>
        /// Delete one Exchange Rate with the Id.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExchangeRate(int id)
        {
            var exchangeRate = await _context.ExchangeRates.FindAsync(id);
            if (exchangeRate == null)
            {
                return NotFound();
            }

            _context.ExchangeRates.Remove(exchangeRate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Deletes all the Exchange Rate.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete()]
        public async Task<IActionResult> DeleteAllExchangeRate()
        {
            var statusCode = HttpStatusCode.OK;
            var message = "";

            var exchangeRate = await _context.ExchangeRates.ToListAsync();
            if (exchangeRate.Count<1) //If the list has no values it does not delete.
            {
                statusCode = HttpStatusCode.Conflict;
                message = "There are no currencies to delete.";
                return StatusCode((int)statusCode, message);
            }
            foreach(var rate in exchangeRate)
            {
                _context.ExchangeRates.Remove(rate);
            }
            
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ExchangeRateExists(int id)
        {
            return _context.ExchangeRates.Any(e => e.Id == id);
        }
    }
}
