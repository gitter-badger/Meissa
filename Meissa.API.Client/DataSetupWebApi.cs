﻿// <copyright file="DataSetupWebApi.cs" company="Automate The Planet Ltd.">
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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Meissa.Core.Contracts;
using Newtonsoft.Json;

namespace Meissa.API.Client
{
    public class RestClientRepository<TEntityDto> : IServiceClient<TEntityDto>
        where TEntityDto : class
    {
        protected const string AppJson = "application/json";

        protected RestClientRepository(string ip, int port, string controllerPartUrl)
        {
            BaseUrl = $"http://{ip}:{port}/";
            ControllerUrlPart = controllerPartUrl;
        }

        public string BaseUrl { get; set; }

        public string ControllerUrlPart { get; set; }

        public string ControllerUrl => $"api/{ControllerUrlPart}";

        public async Task<TEntityDto> CreateAsync(TEntityDto entityToBeCreated)
        {
            if (HttpClientService.Client.BaseAddress == null)
            {
                HttpClientService.Client.BaseAddress = new Uri(BaseUrl);
            }

            var entity = default(TEntityDto);
            string jsonToBeCreated = JsonConvert.SerializeObject(entityToBeCreated);
            var httpContent = new StringContent(jsonToBeCreated, Encoding.UTF8, AppJson);
            var response = await HttpClientService.Client.SendAsyncWithRetry(() => new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{BaseUrl}{ControllerUrl}"),
                Content = httpContent,
            },
            5,
            2000);
            entity = await DeserializeResponse<TEntityDto>(response);

            return entity;
        }

        public async Task UpdateAsync<TSearchCriteria>(TSearchCriteria id, TEntityDto entityToBeUpdated)
        {
            if (HttpClientService.Client.BaseAddress == null)
            {
                HttpClientService.Client.BaseAddress = new Uri(BaseUrl);
            }

            string jsonToBeUpdated = JsonConvert.SerializeObject(new KeyValuePair<TSearchCriteria, TEntityDto>(id, entityToBeUpdated));
            var httpContent = new StringContent(jsonToBeUpdated, Encoding.UTF8, AppJson);
            var response = await HttpClientService.Client.SendAsyncWithRetry(() => new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"{BaseUrl}{ControllerUrl}"),
                Content = httpContent,
            },
            5,
            2000);
        }

        public async Task DeleteAsync<TSearchCriteria>(TSearchCriteria id)
        {
            if (HttpClientService.Client.BaseAddress == null)
            {
                HttpClientService.Client.BaseAddress = new Uri(BaseUrl);
            }

            string jsonToBeCreated = JsonConvert.SerializeObject(id);
            var httpContent = new StringContent(jsonToBeCreated, Encoding.UTF8, AppJson);
            var response = await HttpClientService.Client.SendAsyncWithRetry(() => new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{BaseUrl}{ControllerUrl}"),
                Content = httpContent,
            },
            5,
            2000);
        }

        public async Task<TEntityDto> GetAsync<TSearchCriteria>(TSearchCriteria searchCriteria)
        {
            if (HttpClientService.Client.BaseAddress == null)
            {
                HttpClientService.Client.BaseAddress = new Uri(BaseUrl);
            }

            var entity = default(TEntityDto);
            string jsonToBeCreated = JsonConvert.SerializeObject(searchCriteria);
            var httpContent = new StringContent(jsonToBeCreated, Encoding.UTF8, AppJson);

            var response = await HttpClientService.Client.SendAsyncWithRetry(() => new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{BaseUrl}{ControllerUrl}/id"),
                Content = httpContent,
            },
            5,
            2000);
            entity = await DeserializeResponse<TEntityDto>(response);

            return entity;
        }

        public async Task<IQueryable<TEntityDto>> GetAllAsync()
        {
            if (HttpClientService.Client.BaseAddress == null)
            {
                HttpClientService.Client.BaseAddress = new Uri(BaseUrl);
            }

            var entitiesList = default(List<TEntityDto>);

            var response = await HttpClientService.Client.SendAsyncWithRetry(() => new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{BaseUrl}{ControllerUrl}"),
            },
            5,
            2000);
            entitiesList = await DeserializeResponse<List<TEntityDto>>(response);

            if (entitiesList == null)
            {
                entitiesList = new List<TEntityDto>();
            }

            return entitiesList.AsQueryable();
        }

        protected async Task<TEntity> DeserializeResponse<TEntity>(HttpResponseMessage response)
            where TEntity : class
        {
            TEntity result = default(TEntity);
            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<TEntity>(jsonString);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                result = null;
            }

            return result;
        }
    }
}
