﻿using ApiTest.Data;
using ApiTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly LinkGenerator linkGenerator;

        public CampsController(ICampRepository repository, LinkGenerator linkGenerator)
        {
            _repository = repository;
            this.linkGenerator = linkGenerator;
        }
        [HttpGet]
        public async Task<ActionResult<List<CampModel>>> Get(bool includeTalks = false)
        {
            try
            {
                List<CampModel> campList = new List<CampModel>();

                var tempResult = await _repository.GetAllCampsAsync(includeTalks);
                var result = MapHelper.MapCampModels(tempResult);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            
        }
        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get(string moniker)
        {
            try
            {
                var tempResult = await _repository.GetCampAsync(moniker);

                if (tempResult == null)
                {
                    return NotFound();
                }

                var camp = MapHelper.MapCampModel(tempResult);
                return camp;
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
        [HttpGet("search")]
        public async Task<ActionResult<List<CampModel>>> SearchByDate(DateTime theDate, bool includeTalks = false)
        {
            try
            {
                List<CampModel> campList = new List<CampModel>();

                var tempResult = await _repository.GetAllCampsByEventDate(theDate, false);
                var result = MapHelper.MapCampModels(tempResult);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
        }
        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var existing = await _repository.GetCampAsync(model.Moniker);
                if (existing != null)
                {
                    return BadRequest("Moniker in Use");
                }

                var location = linkGenerator.GetPathByAction("Get",
                  "Camps",
                  new { moniker = model.Moniker });

                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                var result = MapHelper.MapCampModelBack(model);
                _repository.Add(result);
                if (await _repository.SaveChangesAsync())
                {
                    return Created("$/api/camps/{camp.Moniker}", MapHelper.CreatedReturnMap(result));
                }
            }
            catch (Exception e)
            {

                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database failure");
            }
            return BadRequest();
        }
    }
}
