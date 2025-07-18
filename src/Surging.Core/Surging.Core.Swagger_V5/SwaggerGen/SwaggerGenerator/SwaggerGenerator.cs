﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Runtime.Server.Implementation.ServiceDiscovery.Attributes;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.Swagger_V5;
using Surging.Core.Swagger_V5.Swagger; 

namespace Surging.Core.Swagger_V5.SwaggerGen
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly IApiDescriptionGroupCollectionProvider _apiDescriptionsProvider;
        private readonly ISchemaGenerator _schemaGenerator;
        private readonly SwaggerGeneratorOptions _options;
        private readonly IServiceEntryProvider _serviceEntryProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SwaggerGenerator(
            SwaggerGeneratorOptions options,
            IApiDescriptionGroupCollectionProvider apiDescriptionsProvider,
            IHttpContextAccessor httpContextAccessor,
            ISchemaGenerator schemaGenerator)
        {
            _httpContextAccessor = httpContextAccessor;
            _options = options ?? new SwaggerGeneratorOptions();
            _apiDescriptionsProvider = apiDescriptionsProvider;
            _schemaGenerator = schemaGenerator;
            _serviceEntryProvider = ServiceLocator.Current.Resolve<IServiceEntryProvider>();
        }

        public OpenApiDocument GetSwagger(string documentName, string host = null, string basePath = null)
        {
            if (!_options.SwaggerDocs.TryGetValue(documentName, out OpenApiInfo info))
                throw new UnknownSwaggerDocument(documentName, _options.SwaggerDocs.Select(d => d.Key));

            var applicableApiDescriptions = _apiDescriptionsProvider.ApiDescriptionGroups.Items
                .SelectMany(group => group.Items)
                .Where(apiDesc => !(_options.IgnoreObsoleteActions && apiDesc.CustomAttributes().OfType<ObsoleteAttribute>().Any()))
                .Where(apiDesc => _options.DocInclusionPredicate(documentName, apiDesc));

            var schemaRepository = new SchemaRepository(documentName);
            var entries = _serviceEntryProvider.GetALLEntries();
            var query=  _httpContextAccessor.HttpContext.Request.Query;
            if (query.ContainsKey("moduleId"))
            {
                var moduleId = query["moduleId"].ToString();
                var path= moduleId.AsSpan().Slice(0, moduleId.AsSpan().LastIndexOf(".")).ToString();
                entries = entries.Where(p => p.Descriptor.Id.StartsWith(path));
            }
            var mapRoutePaths = AppConfig.SwaggerConfig.Options?.MapRoutePaths;
            if (mapRoutePaths != null)
            {
                foreach (var path in mapRoutePaths)
                {
                    var entry = entries.Where(p => p.RoutePath == path.SourceRoutePath).FirstOrDefault();
                    if (entry != null)
                    {
                        entry.RoutePath = path.TargetRoutePath;
                        entry.Descriptor.RoutePath = path.TargetRoutePath;
                    }
                }
            }
            entries = entries
      .Where(apiDesc => _options.DocInclusionPredicateV2(documentName, apiDesc));


            var swaggerDoc = new OpenApiDocument
            {
                Info = info,
                Servers = GenerateServers(host, basePath),
                Paths = GeneratePaths(entries, schemaRepository),
                Components = new OpenApiComponents
                {
                    Schemas = schemaRepository.Schemas,
                    SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>(_options.SecuritySchemes)
                },
                SecurityRequirements = new List<OpenApiSecurityRequirement>(_options.SecurityRequirements)
            };

            var filterContext = new DocumentFilterContext(applicableApiDescriptions, _schemaGenerator, schemaRepository);
            foreach (var filter in _options.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            swaggerDoc.Components.Schemas = new SortedDictionary<string, OpenApiSchema>(swaggerDoc.Components.Schemas, _options.SchemaComparer);
            return swaggerDoc;
        }

        private IList<OpenApiServer> GenerateServers(string host, string basePath)
        {
            if (_options.Servers.Any())
            {
                return new List<OpenApiServer>(_options.Servers);
            }

            return (host == null && basePath == null)
                ? new List<OpenApiServer>()
                : new List<OpenApiServer> { new OpenApiServer { Url = $"{host}{basePath}" } };
        }

        private OpenApiPaths GeneratePaths(IEnumerable<ApiDescription> apiDescriptions, SchemaRepository schemaRepository)
        {
            var apiDescriptionsByPath = apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.RelativePathSansParameterConstraints());

            var paths = new OpenApiPaths();
            foreach (var group in apiDescriptionsByPath)
            {
                paths.Add($"/{group.Key}",
                    new OpenApiPathItem
                    {
                        Operations = GenerateOperations(group, schemaRepository)
                    });
            };

            return paths;
        }

        private OpenApiPaths GeneratePaths(IEnumerable<ServiceEntry> apiDescriptions, SchemaRepository schemaRepository)
        {
            var apiDescriptionsByPath = apiDescriptions.OrderBy(p => p.RoutePath)
                .GroupBy(apiDesc => apiDesc.Descriptor.Id);

            var paths = new OpenApiPaths();
            foreach (var group in apiDescriptionsByPath)
            {
                var key = $"/{group.Min(p => p.RoutePath).TrimStart('/')}".Trim();
                if(!paths.ContainsKey(key))
                paths.Add(key,
                    new OpenApiPathItem
                    {
                        Operations = GenerateOperations(group, schemaRepository)
                    }); 
                else
                    paths.Add($"/{group.Min(p => $" {p.RoutePath}( {string.Join("_", p.Parameters.Select(m => m.ParameterType.Name))})")}",
             new OpenApiPathItem
             {
                 Operations = GenerateOperations(group, schemaRepository)
             });

            };

            return paths;
        }

        private IDictionary<OperationType, OpenApiOperation> GenerateOperations(
            IEnumerable<ApiDescription> apiDescriptions,
            SchemaRepository schemaRepository)
        {
            var apiDescriptionsByMethod = apiDescriptions
                .OrderBy(_options.SortKeySelector)
                .GroupBy(apiDesc => apiDesc.HttpMethod);

            var operations = new Dictionary<OperationType, OpenApiOperation>();

            foreach (var group in apiDescriptionsByMethod)
            {
                var httpMethod = group.Key;

                if (httpMethod == null)
                    throw new SwaggerGeneratorException(string.Format(
                        "Ambiguous HTTP method for action - {0}. " +
                        "Actions require an explicit HttpMethod binding for Swagger/OpenAPI 3.0",
                        group.First().ActionDescriptor.DisplayName));

                if (group.Count() > 1 && _options.ConflictingActionsResolver == null)
                    throw new SwaggerGeneratorException(string.Format(
                        "Conflicting method/path combination \"{0} {1}\" for actions - {2}. " +
                        "Actions require a unique method/path combination for Swagger/OpenAPI 3.0. Use ConflictingActionsResolver as a workaround",
                        httpMethod,
                        group.First().RelativePath,
                        string.Join(",", group.Select(apiDesc => apiDesc.ActionDescriptor.DisplayName))));

                var apiDescription = (group.Count() > 1) ? _options.ConflictingActionsResolver(group) : group.Single();

                operations.Add(OperationTypeMap[httpMethod.ToUpper()], GenerateOperation(apiDescription, schemaRepository));
            };

            return operations;
        }

        private IDictionary<OperationType, OpenApiOperation> GenerateOperations(
         IEnumerable<ServiceEntry> apiDescriptions,
         SchemaRepository schemaRepository)
        {
             
            var operations = new Dictionary<OperationType, OpenApiOperation>();
            
            foreach (var entry in apiDescriptions)
            { 
                    var httpMethod = "";
                    var methodInfo = entry.Type.GetTypeInfo().DeclaredMethods.Where(p => p.Name == entry.MethodName).FirstOrDefault();
                    var parameterInfo = methodInfo.GetParameters();

                    if (entry.Methods.Count() == 0)
                    {
                        if (parameterInfo != null && parameterInfo.Any(p =>
                      !UtilityType.ConvertibleType.GetTypeInfo().IsAssignableFrom(p.ParameterType)))
                            httpMethod = "post";
                        else
                            httpMethod = "get";  
                        operations.Add(OperationTypeMap[httpMethod.ToUpper()], GenerateOperation(entry, schemaRepository));
                    }
                    else if(entry.Methods.Count() >0)
                    {
                        foreach(var method in entry.Methods)
                        operations.Add(OperationTypeMap[method.ToUpper()], GenerateOperation(entry, schemaRepository));
                    }
                    //var apiDescription = (group.Count() > 1) ? _options.ConflictingActionsResolver(group) : group.Single();
                     
            };

            return operations;
        }

        private OpenApiOperation GenerateOperation(ServiceEntry entry, SchemaRepository schemaRepository)
        {
            try
            {
                var operation = new OpenApiOperation();
                operation.Tags = GenerateOperationTags(entry);
                    operation.OperationId = _options.EntryOperationIdSelector(entry);
                operation.Parameters = GenerateParameters(entry, schemaRepository); 
                var methodInfo = entry.Type.GetTypeInfo().DeclaredMethods.Where(p => p.Name == entry.MethodName).FirstOrDefault();
                var filterContext = new OperationFilterContext(null, _schemaGenerator, schemaRepository, methodInfo,entry);
                foreach (var filter in _options.OperationFilters)
                {
                    filter.Apply(operation, filterContext);
                }
                operation.RequestBody = GenerateRequestBody(entry, schemaRepository);
                operation.Responses = GenerateResponses(entry, schemaRepository);
                operation.Deprecated = entry.Attributes.OfType<ObsoleteAttribute>().Any(); 

                return operation;
            }
            catch (Exception ex)
            {
                throw new SwaggerGeneratorException(
                    message: $"Failed to generate Operation for action - {entry.RoutePath}. See inner exception",
                    innerException: ex);
            }
        }

        private OpenApiOperation GenerateOperation(ApiDescription apiDescription, SchemaRepository schemaRepository)
        {
            try
            {
                var operation = new OpenApiOperation
                {
                    Tags = GenerateOperationTags(apiDescription),
                    OperationId = _options.OperationIdSelector(apiDescription),
                    Parameters = GenerateParameters(apiDescription, schemaRepository),
                    RequestBody = GenerateRequestBody(apiDescription, schemaRepository),
                    Responses = GenerateResponses(apiDescription, schemaRepository),
                    Deprecated = apiDescription.CustomAttributes().OfType<ObsoleteAttribute>().Any()
                };

                apiDescription.TryGetMethodInfo(out MethodInfo methodInfo);
                var filterContext = new OperationFilterContext(apiDescription, _schemaGenerator, schemaRepository, methodInfo);
                foreach (var filter in _options.OperationFilters)
                {
                    filter.Apply(operation, filterContext);
                }

                return operation;
            }
            catch (Exception ex)
            {
                throw new SwaggerGeneratorException(
                    message: $"Failed to generate Operation for action - {apiDescription.ActionDescriptor.DisplayName}. See inner exception",
                    innerException: ex);
            }
        }

        private IList<OpenApiTag> GenerateOperationTags(ServiceEntry apiDescription)
        {
            return _options.EntryTagsSelector(apiDescription)
                .Select(tagName => new OpenApiTag { Name = tagName })
                .ToList();
        }

        private IList<OpenApiTag> GenerateOperationTags(ApiDescription apiDescription)
        {
            return _options.TagsSelector(apiDescription)
                .Select(tagName => new OpenApiTag { Name = tagName })
                .ToList();
        }

        private IList<OpenApiParameter> GenerateParameters(ApiDescription apiDescription, SchemaRepository schemaRespository)
        {
            var applicableApiParameters = apiDescription.ParameterDescriptions
                .Where(apiParam =>
                {
                    return (!apiParam.IsFromBody() && !apiParam.IsFromForm())
                        && (!apiParam.CustomAttributes().OfType<BindNeverAttribute>().Any())
                        && (apiParam.ModelMetadata == null || apiParam.ModelMetadata.IsBindingAllowed);
                });

            return applicableApiParameters
                .Select(apiParam => GenerateParameter(apiParam, schemaRespository))
                .ToList();
        }

        private IList<OpenApiParameter> GenerateParameters(ServiceEntry apiDescription, SchemaRepository schemaRespository)
        {
            var applicableApiParameters = apiDescription.Parameters
                .Where(apiParam =>
                {
                    return apiParam != null &&
            UtilityType.ConvertibleType.GetTypeInfo().IsAssignableFrom(apiParam?.ParameterType);
        });

            return applicableApiParameters
                .Select(apiParam => GenerateParameter(apiParam, schemaRespository))
                 .Union(new OpenApiParameter[] { CreateServiceKeyParameter(schemaRespository) }).ToList();
        }

        private OpenApiParameter GenerateParameter(
            ApiParameterDescription apiParameter,
            SchemaRepository schemaRepository)
        {
            var name = _options.DescribeAllParametersInCamelCase
                ? apiParameter.Name.ToCamelCase()
                : apiParameter.Name;

            var location = (apiParameter.Source != null && ParameterLocationMap.ContainsKey(apiParameter.Source))
                ? ParameterLocationMap[apiParameter.Source]
                : ParameterLocation.Query;

            var isRequired = apiParameter.IsRequiredParameter();

            var schema = (apiParameter.ModelMetadata != null)
                ? GenerateSchema(
                    apiParameter.ModelMetadata.ModelType,
                    schemaRepository,
                    apiParameter.PropertyInfo(),
                    apiParameter.ParameterInfo())
                : new OpenApiSchema { Type = "string" };

            var parameter = new OpenApiParameter
            {
                Name = name,
                In = location,
                Required = isRequired,
                Schema = schema
            };

            var filterContext = new ParameterFilterContext(
                apiParameter,
                _schemaGenerator,
                schemaRepository,
                apiParameter.PropertyInfo(),
                apiParameter.ParameterInfo());

            foreach (var filter in _options.ParameterFilters)
            {
                filter.Apply(parameter, filterContext);
            }

            return parameter;
        }

        private OpenApiParameter GenerateParameter(
       ParameterInfo apiParameter,
       SchemaRepository schemaRepository)
        {
            var name = _options.DescribeAllParametersInCamelCase
                ? apiParameter.Name?.ToCamelCase()
                : apiParameter.Name;

            var location = ParameterLocation.Query;

            var isRequired = true;

            var schema = GenerateSchema(
                    apiParameter.ParameterType,
                    schemaRepository,
                    default,
                    apiParameter);
                 

            var parameter = new OpenApiParameter
            {
                Name = name,
                In = location,
                Required = isRequired,
                Schema = schema
            };

            var filterContext = new ParameterFilterContext(
                null,
                _schemaGenerator,
                schemaRepository,
              default,
               apiParameter);

            foreach (var filter in _options.ParameterFilters)
            {
                filter.Apply(parameter, filterContext);
            }
            return parameter;
        }

        private OpenApiParameter CreateServiceKeyParameter(SchemaRepository schemaRepository)
        { 
            var schema = GenerateSchema(
                 typeof(string),
                 schemaRepository);

            schema.Description = "ServiceKey";
         
            var parameter = new OpenApiParameter
            {
                Name = "ServiceKey",
                In = ParameterLocation.Query,
                Required = false,
                Schema = schema
            }; 
            return parameter;
        }

        private OpenApiSchema GenerateSchema(
            Type type,
            SchemaRepository schemaRepository,
            PropertyInfo propertyInfo = null,
            ParameterInfo parameterInfo = null)
        {
            try
            {
                return _schemaGenerator.GenerateSchema(type, schemaRepository, propertyInfo, parameterInfo);
            }
            catch (Exception ex)
            {
                throw new SwaggerGeneratorException(
                    message: $"Failed to generate schema for type - {type}. See inner exception",
                    innerException: ex);
            }
        }



        private OpenApiRequestBody GenerateRequestBody(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            OpenApiRequestBody requestBody = null;
            RequestBodyFilterContext filterContext = null;

            var bodyParameter = apiDescription.ParameterDescriptions
                .FirstOrDefault(paramDesc => paramDesc.IsFromBody());

            var formParameters = apiDescription.ParameterDescriptions
                .Where(paramDesc => paramDesc.IsFromForm());

            if (bodyParameter != null)
            {
                requestBody = GenerateRequestBodyFromBodyParameter(apiDescription, schemaRepository, bodyParameter);

                filterContext = new RequestBodyFilterContext(
                    bodyParameterDescription: bodyParameter,
                    formParameterDescriptions: null,
                    schemaGenerator: _schemaGenerator,
                    schemaRepository: schemaRepository);
            }
            else if (formParameters.Any())
            {
                requestBody = GenerateRequestBodyFromFormParameters(apiDescription, schemaRepository, formParameters);

                filterContext = new RequestBodyFilterContext(
                    bodyParameterDescription: null,
                    formParameterDescriptions: formParameters,
                    schemaGenerator: _schemaGenerator,
                    schemaRepository: schemaRepository);
            }

            if (requestBody != null)
            {
                foreach (var filter in _options.RequestBodyFilters)
                {
                    filter.Apply(requestBody, filterContext);
                }
            }

            return requestBody;
        }

        private OpenApiRequestBody GenerateRequestBody(
           ServiceEntry entry,
           SchemaRepository schemaRepository)
        {
            OpenApiRequestBody requestBody = null;
            RequestBodyFilterContext filterContext = null;

            var parameters = entry.Parameters;


            if (parameters != null && parameters.Any(p =>
                !UtilityType.ConvertibleType.GetTypeInfo().IsAssignableFrom(p.ParameterType) && p.ParameterType.Name != "HttpFormCollection"))
            {
                
                requestBody = GenerateRequestBodyFromBodyParameter(entry, schemaRepository, parameters);
                filterContext = new RequestBodyFilterContext(
               bodyParameterDescription: null,
               formParameterDescriptions: null,
               schemaGenerator: _schemaGenerator,
               schemaRepository: schemaRepository);

            }
            else if (parameters != null && parameters.Any(p =>
                !UtilityType.ConvertibleType.GetTypeInfo().IsAssignableFrom(p.ParameterType) && p.ParameterType.Name == "HttpFormCollection"))
            {
                requestBody = GenerateRequestBodyFromFormParameters(entry, schemaRepository, parameters);
                filterContext = new RequestBodyFilterContext(
                    bodyParameterDescription: null,
                    formParameterDescriptions: null,
                    schemaGenerator: _schemaGenerator,
                    schemaRepository: schemaRepository);
            }

            if (requestBody != null)
            {
                foreach (var filter in _options.RequestBodyFilters)
                {
                    filter.Apply(requestBody, filterContext);
                }
            }

            return requestBody;
        }

        private OpenApiRequestBody GenerateRequestBodyFromBodyParameter(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            ApiParameterDescription bodyParameter)
        {
            var contentTypes = InferRequestContentTypes(apiDescription);

            var isRequired = bodyParameter.IsRequiredParameter();

            var schema = GenerateSchema(
                bodyParameter.ModelMetadata.ModelType,
                schemaRepository,
                bodyParameter.PropertyInfo(),
                bodyParameter.ParameterInfo());

            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType
                        {
                            Schema = schema
                        }
                    ),
                Required = isRequired
            };
        }

        private OpenApiRequestBody GenerateRequestBodyFromBodyParameter(
    ServiceEntry entry,
    SchemaRepository schemaRepository,
    ParameterInfo[] bodyParameter)
        {
            var contentTypes =new string[] { "application/json"};

            var isRequired = true;
               var properties = new Dictionary<string, OpenApiSchema>(); 
            var requiredPropertyNames = new List<string>();

            foreach (var formParameter in bodyParameter)
            {
                var name = _options.DescribeAllParametersInCamelCase
                    ? formParameter.Name.ToCamelCase()
                    : formParameter.Name;

                var propertySchema = new OpenApiSchema { Type = "string" };

                if (formParameter.ParameterType != null)
                {

                    propertySchema = GenerateSchema(
                        formParameter.ParameterType,
                        schemaRepository,
                       default,
                        formParameter);
                }

                properties.Add(name, propertySchema);

                requiredPropertyNames.Add(name);
            }

            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = properties,
                Required = new SortedSet<string>(requiredPropertyNames)
            };
            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType
                        {
                            Schema = schema,
                            Encoding = schema.Properties.ToDictionary(
                                entry => entry.Key,
                                entry => new OpenApiEncoding { Style = ParameterStyle.DeepObject }
                            )
                        }
                    ),
                Required = isRequired
            };
        }

        private IEnumerable<string> InferRequestContentTypes(ApiDescription apiDescription)
        {
            // If there's content types explicitly specified via ConsumesAttribute, use them
            var explicitContentTypes = apiDescription.CustomAttributes().OfType<ConsumesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            if (explicitContentTypes.Any()) return explicitContentTypes;

            // If there's content types surfaced by ApiExplorer, use them
            var apiExplorerContentTypes = apiDescription.SupportedRequestFormats
                .Select(format => format.MediaType)
                .Where(x => x != null)
                .Distinct();
            if (apiExplorerContentTypes.Any()) return apiExplorerContentTypes;

            return Enumerable.Empty<string>();
        }

        private OpenApiRequestBody GenerateRequestBodyFromFormParameters(
ServiceEntry entry,
SchemaRepository schemaRepository,
ParameterInfo[] bodyParameter)
        {
            var contentTypes = new string[] { "multipart/form-data" };

            var isRequired = true;
            var properties = new Dictionary<string, OpenApiSchema>();
            var requiredPropertyNames = new List<string>();

            foreach (var formParameter in bodyParameter)
            {
                var name = _options.DescribeAllParametersInCamelCase
                    ? formParameter.Name.ToCamelCase()
                    : formParameter.Name;

                var propertySchema = new OpenApiSchema { Type = "string" };
                if (typeof(IEnumerable<KeyValuePair<string, StringValues>>).IsAssignableFrom(formParameter.ParameterType) &&
                formParameter.ParameterType.Name == "HttpFormCollection")
                {
                    propertySchema = new OpenApiSchema { Type = "file" };
                }
                else if (formParameter.ParameterType != null)
                {

                    propertySchema = GenerateSchema(
                        formParameter.ParameterType,
                        schemaRepository,
                       default,
                        formParameter);
                }

                properties.Add(name, propertySchema);

                requiredPropertyNames.Add(name);
            }
            var schema = new OpenApiSchema
            {
                Type = "object",
                Properties = properties,
                Required = new SortedSet<string>(requiredPropertyNames)
            };
             

            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                       contentType => new OpenApiMediaType
                       {
                           Schema = schema,
                           Encoding = schema.Properties.ToDictionary(
                                entry => entry.Key,
                                entry => new OpenApiEncoding { Style = ParameterStyle.Form }
                            )
                       }
                    ),
                Required = isRequired
            };
        }


        private OpenApiRequestBody GenerateRequestBodyFromFormParameters(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            IEnumerable<ApiParameterDescription> formParameters)
        {
            var contentTypes = InferRequestContentTypes(apiDescription);
            contentTypes = contentTypes.Any() ? contentTypes : new[] { "multipart/form-data" };

            var schema = GenerateSchemaFromFormParameters(formParameters, schemaRepository);

            return new OpenApiRequestBody
            {
                Content = contentTypes
                    .ToDictionary(
                        contentType => contentType,
                        contentType => new OpenApiMediaType
                        {
                            Schema = schema,
                            Encoding = schema.Properties.ToDictionary(
                                entry => entry.Key,
                                entry => new OpenApiEncoding { Style = ParameterStyle.Form }
                            )
                        }
                    )
            };
        }

        private OpenApiSchema GenerateSchemaFromFormParameters(
            IEnumerable<ApiParameterDescription> formParameters,
            SchemaRepository schemaRepository)
        {
            var properties = new Dictionary<string, OpenApiSchema>();
            var requiredPropertyNames = new List<string>();

            foreach (var formParameter in formParameters)
            {
                var name = _options.DescribeAllParametersInCamelCase
                    ? formParameter.Name.ToCamelCase()
                    : formParameter.Name;

                var schema = (formParameter.ModelMetadata != null)
                    ? GenerateSchema(
                        formParameter.ModelMetadata.ModelType,
                        schemaRepository,
                        formParameter.PropertyInfo(),
                        formParameter.ParameterInfo())
                    : new OpenApiSchema { Type = "string" };

                properties.Add(name, schema);

                if (formParameter.IsRequiredParameter())
                    requiredPropertyNames.Add(name);
            }

            return new OpenApiSchema
            {
                Type = "object",
                Properties = properties,
                Required = new SortedSet<string>(requiredPropertyNames)
            };
        }

        private OpenApiResponses GenerateResponses(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository)
        {
            var supportedResponseTypes = apiDescription.SupportedResponseTypes
                .DefaultIfEmpty(new ApiResponseType { StatusCode = 200 });

            var responses = new OpenApiResponses();
            foreach (var responseType in supportedResponseTypes)
            {
                var statusCode = responseType.IsDefaultResponse() ? "default" : responseType.StatusCode.ToString();
                responses.Add(statusCode, GenerateResponse(apiDescription, schemaRepository, statusCode, responseType));
            }
            return responses;
        }

        private OpenApiResponses GenerateResponses(
    ServiceEntry entry,
    SchemaRepository schemaRepository)
        {
            var description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch("200", entry.Key))
                .Value;
            var methodInfo = entry.Type.GetTypeInfo().DeclaredMethods.Where(p => p.Name == entry.MethodName).FirstOrDefault();
            var responses = new OpenApiResponses();
            var content = new Dictionary<string, OpenApiMediaType>();
            if (methodInfo.ReturnType != typeof(Task) && methodInfo.ReturnType != typeof(void))
            {
                content.TryAdd("application/json", new OpenApiMediaType
                {
                    Schema = GenerateSchema(typeof(HttpResultMessage<>).MakeGenericType(methodInfo.ReturnType.GenericTypeArguments), schemaRepository)
                });
    
            }
            else
            {  
             
                content.TryAdd("application/json",new OpenApiMediaType
                {
                    Schema =null
                });
              
            }

            responses.Add("200", new OpenApiResponse
            {
                Description = description,
                Content = content
     
            });
            return responses;
        }

        private OpenApiResponse GenerateResponse(
            ApiDescription apiDescription,
            SchemaRepository schemaRepository,
            string statusCode,
            ApiResponseType apiResponseType)
        {
            var description = ResponseDescriptionMap
                .FirstOrDefault((entry) => Regex.IsMatch(statusCode, entry.Key))
                .Value;

            var responseContentTypes = InferResponseContentTypes(apiDescription, apiResponseType);

            return new OpenApiResponse
            {
                Description = description,
                Content = responseContentTypes.ToDictionary(
                    contentType => contentType,
                    contentType => CreateResponseMediaType(apiResponseType.ModelMetadata, schemaRepository)
                )
            };
        }

        private IEnumerable<string> InferResponseContentTypes(ApiDescription apiDescription, ApiResponseType apiResponseType)
        {
            // If there's no associated model, return an empty list (i.e. no content)
            if (apiResponseType.ModelMetadata == null) return Enumerable.Empty<string>();

            // If there's content types explicitly specified via ProducesAttribute, use them
            var explicitContentTypes = apiDescription.CustomAttributes().OfType<ProducesAttribute>()
                .SelectMany(attr => attr.ContentTypes)
                .Distinct();
            if (explicitContentTypes.Any()) return explicitContentTypes;

            // If there's content types surfaced by ApiExplorer, use them
            var apiExplorerContentTypes = apiResponseType.ApiResponseFormats
                .Select(responseFormat => responseFormat.MediaType)
                .Distinct();
            if (apiExplorerContentTypes.Any()) return apiExplorerContentTypes;

            return Enumerable.Empty<string>();
        }

        private OpenApiMediaType CreateResponseMediaType(ModelMetadata modelMetadata, SchemaRepository schemaRespository)
        {
            return new OpenApiMediaType
            {
                Schema = GenerateSchema(modelMetadata.ModelType, schemaRespository)
            };
        }

        private static readonly Dictionary<string, OperationType> OperationTypeMap = new Dictionary<string, OperationType>
        {
            { "GET", OperationType.Get },
            { "PUT", OperationType.Put },
            { "POST", OperationType.Post },
            { "DELETE", OperationType.Delete },
            { "OPTIONS", OperationType.Options },
            { "HEAD", OperationType.Head },
            { "PATCH", OperationType.Patch },
            { "TRACE", OperationType.Trace }
        };

        private static readonly Dictionary<BindingSource, ParameterLocation> ParameterLocationMap = new Dictionary<BindingSource, ParameterLocation>
        {
            { BindingSource.Query, ParameterLocation.Query },
            { BindingSource.Header, ParameterLocation.Header },
            { BindingSource.Path, ParameterLocation.Path }
        };

        private static readonly Dictionary<string, string> ResponseDescriptionMap = new Dictionary<string, string>
        {
            { "1\\d{2}", "Information" },
            { "2\\d{2}", "Success" },
            { "304", "Not Modified" },
            { "3\\d{2}", "Redirect" },
            { "400", "Bad Request" },
            { "401", "Unauthorized" },
            { "403", "Forbidden" },
            { "404", "Not Found" },
            { "405", "Method Not Allowed" },
            { "406", "Not Acceptable" },
            { "408", "Request Timeout" },
            { "409", "Conflict" },
            { "4\\d{2}", "Client Error" },
            { "5\\d{2}", "Server Error" },
            { "default", "Error" }
        };
    }
}
