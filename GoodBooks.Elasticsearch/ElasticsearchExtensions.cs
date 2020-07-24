using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nest;
using System;
using GoodBooks.Common.Exceptions;
using GoodBooks.Data.Model.Models;

namespace GoodBooks.Elasticsearch
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var elasticsearchOptions = configuration
                .GetSection(ElasticsearchOptions.Section)?
                .Get<ElasticsearchOptions>();

            if (elasticsearchOptions == null)
                throw new MissingConfigurationException(
                    $"Configuration section named {ElasticsearchOptions.Section} was not found.");

            var settings = new ConnectionSettings(new Uri(elasticsearchOptions.Url)).DefaultIndex(elasticsearchOptions.Index)
                .DefaultMappingFor<Book>(m => m
                    .IdProperty(p => p.BookId)
                    .PropertyName(p => p.Title, "title")
                    .PropertyName(p => p.Authors, "authors")
                    .Ignore(p => p.Reviews)
                );

            var client = new ElasticClient(settings);

            services.AddSingleton<IElasticClient>(client);
        }
    }
}