using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Zendesk.JsonSearch.Logic.CustomExceptions;
using Zendesk.JsonSearch.Models;

namespace Zendesk.JsonSearch.Logic.Helpers
{
    internal static class JsonHelper
    {
        internal static List<Entity> ReadJson(string filePath, EntityMetadata metadata)
        {
            try
            {
                var entities = new List<Entity>();

                var fileContent = File.ReadAllText(filePath);
                var oArray = JsonConvert.DeserializeObject<JArray>(fileContent, new JsonSerializerSettings { DateParseHandling = DateParseHandling.None });

                if (oArray.First != null)
                {
                    var currentEntity = oArray.First;
                    do
                    {
                        var entity = new Entity(metadata.EntityName);
                        var properties = ((JObject)currentEntity).Properties();
                        foreach (var property in properties)
                        {
                            if (property.Value.Type == JTokenType.Integer)
                            {
                                entity.SetAttribute(property.Name, property.Value.ToObject<int>().ToString());
                            }
                            else if (property.Value.Type == JTokenType.String)
                            {
                                entity.SetAttribute(property.Name, property.Value.ToObject<string>());
                            }
                            else if (property.Value.Type == JTokenType.Array)
                            {
                                entity.SetAttribute(property.Name, property.Value.ToObject<List<string>>());
                            }
                            else
                            {
                                entity.SetAttribute(property.Name, property.Value.ToObject<object>());
                            }

                            if (property.Name == metadata.NameProperty) entity.name = property.Value.ToObject<string>();

                        }
                        entities.Add(entity);
                        currentEntity = currentEntity.Next;
                    } while (currentEntity != null);
                }
                return entities;
            }
            catch (Exception ex)
            {
                throw new JsonDeseriliseException(ex.Message);
            }
        }

        internal static T ReadJsonToModel<T>(string filePath)
        {
            try
            {
                var fileContent = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(fileContent);
            }
            catch(Exception ex)
            {
                throw new JsonDeseriliseException(ex.Message);
            }
        }
    }
}
