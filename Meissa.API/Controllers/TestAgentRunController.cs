﻿// <copyright file="TestAgentRunController.cs" company="Automate The Planet Ltd.">
// Copyright 2018 Automate The Planet Ltd.
// Licensed under the Apache License, Version 2.0 (the "License");
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Anton Angelov</author>
// <site>https://automatetheplanet.com/</site>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Meissa.API.Models;
using Meissa.API.Services;
using Meissa.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Meissa.API.Controllers
{
    [Route("api/testagentruns")]
    public class TestAgentRunController : Controller
    {
        private readonly ILogger<TestAgentRunController> _logger;
        private readonly MeissaRepository _meissaRepository;

        public TestAgentRunController(ILogger<TestAgentRunController> logger, MeissaRepository repository)
        {
            _logger = logger;
            _meissaRepository = repository;
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetTestAgentRunAsync([FromBody] int id)
        {
            try
            {
                var testAgentRun = await _meissaRepository.GetByIdAsync<TestAgentRun>(id);
                if (testAgentRun == null)
                {
                    _logger.LogInformation($"TestAgentRun with id {id} wasn't found.");
                    return NotFound();
                }

                var testAgentRunDto = Mapper.Map<TestAgentRunDto>(testAgentRun);

                return Ok(testAgentRunDto);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting test run with id {id}.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTestAgentRunsAsync()
        {
            try
            {
                var logs = await _meissaRepository.GetAllQueryWithRefreshAsync<TestAgentRun>();
                var logDto = Mapper.Map<IEnumerable<TestAgentRunDto>>(logs);

                return Ok(logDto);
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Exception while getting logs.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestAgentRunAsync([FromBody] TestAgentRunDto logDto)
        {
            if (logDto == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var log = Mapper.Map<TestAgentRun>(logDto);

            var result = await _meissaRepository.InsertWithSaveAsync(log);

            var resultDto = Mapper.Map<TestAgentRunDto>(result);

            return Ok(resultDto);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTestAgentRunAsync([FromBody] KeyValuePair<int, TestAgentRunDto> updateObject)
        {
            if (updateObject.Value == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entityToBeUpdated = await _meissaRepository.GetByIdAsync<TestAgentRun>(updateObject.Key);
            if (entityToBeUpdated == null)
            {
                return NotFound();
            }

            ////if (entityToBeUpdated.Status.Equals(TestAgentRunStatus.Completed) && !entityToBeUpdated.Status.Equals(updateObject.Value.Status) && updateObject.Value.Status.Equals(TestAgentRunStatus.Aborted))
            ////{
            ////    return NoContent();
            ////}

            entityToBeUpdated = Mapper.Map(updateObject.Value, entityToBeUpdated);
            await _meissaRepository.UpdateWithSaveAsync(entityToBeUpdated);

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTestAgentRunAsync([FromBody] int id)
        {
            var entityToBeRemoved = await _meissaRepository.GetByIdAsync<TestAgentRun>(id);

            if (entityToBeRemoved == null)
            {
                return NotFound();
            }

            await _meissaRepository.DeleteWithSaveAsync(entityToBeRemoved);

            return NoContent();
        }
    }
}
