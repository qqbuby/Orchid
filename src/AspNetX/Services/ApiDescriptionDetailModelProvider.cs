﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AspNetX.Abstractions;
using AspNetX.Models;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AspNetX.Services
{
    /// <inheritdoc />
    public class ApiDescriptionDetailModelProvider : IApiDescriptionDetailModelProvider
    {
        private readonly IReadOnlyDictionary<string, ApiDescriptionDetailModel> _apiDescriptionDetailModelCache;

        /// <summary>
        /// Creates a new instance of <see cref="ApiDescriptionDetailModelProvider"/>.
        /// </summary>
        /// <param name="apiDescriptionGroupModelCollectionProvider">
        /// The <see cref="IApiDescriptionGroupModelCollectionProvider"/>.
        /// </param>
        public ApiDescriptionDetailModelProvider(IApiDescriptionGroupModelCollectionProvider apiDescriptionGroupModelCollectionProvider)
        {
            var dictionary = apiDescriptionGroupModelCollectionProvider
                .ApiDescriptionGroups
                .Items
                .SelectMany(g => g.Items)
                .ToDictionary(o => o.Id, o => CreateApiDescriptionDetailModel(o));   //TODO Maybe exits dumpliate ids.
            _apiDescriptionDetailModelCache = new ReadOnlyDictionary<string, ApiDescriptionDetailModel>(dictionary);
        }

        public ApiDescriptionDetailModel this[string key] => _apiDescriptionDetailModelCache[key];

        public int Count => _apiDescriptionDetailModelCache.Count;

        public IEnumerable<string> Keys => _apiDescriptionDetailModelCache.Keys;

        public IEnumerable<ApiDescriptionDetailModel> Values => _apiDescriptionDetailModelCache.Values;

        public bool ContainsKey(string key) => _apiDescriptionDetailModelCache.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, ApiDescriptionDetailModel>> GetEnumerator() => _apiDescriptionDetailModelCache.GetEnumerator();

        public bool TryGetValue(string key, out ApiDescriptionDetailModel value) => _apiDescriptionDetailModelCache.TryGetValue(key, out value);

        private ApiDescriptionDetailModel CreateApiDescriptionDetailModel(ApiDescriptionModel apiDescriptionModel)
        {
            var apiDescriptionDetailModel = new ApiDescriptionDetailModel
            {
                Id = apiDescriptionModel.Id,
                Description = apiDescriptionModel.Description,
                GroupName = apiDescriptionModel.GroupName,
                HttpMethod = apiDescriptionModel.HttpMethod,
                RelativePath = apiDescriptionModel.RelativePath
            };
            foreach (var parameter in apiDescriptionModel.ApiDescription.ParameterDescriptions)
            {
                if (parameter.Source == BindingSource.Body)
                {
                    apiDescriptionDetailModel.RequestInformation.BodyParameterDescriptions.Add(CreateApiParameterDescriptionModel(parameter));
                }
                else if (parameter.Source == BindingSource.Query || parameter.Source == BindingSource.Path)
                {
                    apiDescriptionDetailModel.RequestInformation.UriParameterDescriptions.Add(CreateApiParameterDescriptionModel(parameter));
                }
            }
            return apiDescriptionDetailModel;
        }

        private static ApiParameterDescriptionModel CreateApiParameterDescriptionModel(ApiParameterDescription parameter)
        {
            return new ApiParameterDescriptionModel { ApiParameterDescription = parameter };
        }

        IEnumerator IEnumerable.GetEnumerator() => _apiDescriptionDetailModelCache.GetEnumerator();
    }
}
