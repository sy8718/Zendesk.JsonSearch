using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zendesk.JsonSearch.Logic.Consts;
using Zendesk.JsonSearch.Logic.CustomExceptions;
using Zendesk.JsonSearch.Logic.Helpers;
using Zendesk.JsonSearch.Models;
using Zendesk.JsonSearch.Models.EarlyBound;
using Zendesk.JsonSearch.Models.Helpers;

namespace Zendesk.JsonSearch.Logic
{
    /// <summary>
    /// As all data will be loaded into memory and shared during the whole application lifetime, the class needs to be static or use singleton 
    /// </summary>
    public sealed class Framework
    {
        // Use Lazy<T> to make threading safe
        private static readonly Lazy<Framework> lazy = new Lazy<Framework>(() => new Framework());
        public static Framework Instance { get { return lazy.Value; } }

        private Framework()
        {
        }

        public CacheModel.CachedEntityCollection CahcedEntityCollection { get; private set; }
        public CacheModel.CachedIndexingCollection CahcedIndexingCollection { get; private set; }
        public IConfiguration Configuration { get; private set; }
        public Metadata Metadata { get; private set; }



        #region Initialisation
        /// <summary>
        /// Initialise the app will load all data into memory
        /// </summary>
        /// <param name="configFile">Conifg file to be loaded. use default data if leave blank</param>
        public void Initialise(string configFile = Consts.ConfigConsts.ConfigFile)
        {
            InitialConfiguration(configFile);
            InitialMetadata();
            InitialEntitiesAndIndexings();

        }

        private void InitialConfiguration(string configFile)
        {
            Configuration = ConfigHelper.InitialiseConfig(configFile);
        }

        private void InitialMetadata()
        {
            var fileAddress = $"{Configuration[ConfigConsts.MetadataFileFolder]}/{Configuration[ConfigConsts.MetadataFileName]}";
            try
            {
                if (!File.Exists(fileAddress)) throw new FileNotFoundException();
                Metadata = JsonHelper.ReadJsonToModel<Metadata>(fileAddress);
            }
            catch (JsonDeseriliseException ex)
            {
                throw new JsonDeseriliseException($"Deserilise JSON file {fileAddress} failed. Please correct file and try again.");
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException($"File {fileAddress} does not exist.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Initilise file {fileAddress} failed. {ex.Message}");
            }
        }

        private void InitialEntitiesAndIndexings()
        {
            var directoryAddress = Configuration[ConfigConsts.EntitiesFileFolder];
            try
            {
                if (!Directory.Exists(directoryAddress)) throw new DirectoryNotFoundException();

                CahcedEntityCollection = new CacheModel.CachedEntityCollection();
                CahcedEntityCollection.EntityCollection = new Dictionary<string, CacheModel.CachedEntities>();
                CahcedIndexingCollection = new CacheModel.CachedIndexingCollection();
                CahcedIndexingCollection.IndexingCollection = new Dictionary<string, Dictionary<string, CacheModel.CachedIndexings>>();

                foreach (var file in Directory.GetFiles(Configuration[ConfigConsts.EntitiesFileFolder]))
                {
                    try
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file);
                        var entitymetadata = Metadata.Entities.Where(e => e.FileName == fileName).FirstOrDefault();
                        var entities = JsonHelper.ReadJson(file, entitymetadata);
                        CahcedEntityCollection.EntityCollection.Add(fileName, new CacheModel.CachedEntities(entities));
                        CahcedIndexingCollection.IndexingCollection.Add(fileName, new Dictionary<string, CacheModel.CachedIndexings>());
                        var indexingProperties = Metadata.Entities.FirstOrDefault(e => e.FileName == fileName).IndexingProperties;
                        if (indexingProperties != null && indexingProperties.Any())
                        {
                            foreach (var fieldName in indexingProperties)
                            {
                                var cachedIndexings = new CacheModel.CachedIndexings(entities, fieldName);
                                CahcedIndexingCollection.IndexingCollection[fileName].Add(fieldName, cachedIndexings);
                            }
                        }
                    }
                    catch (JsonDeseriliseException ex)
                    {
                        throw new JsonDeseriliseException($"Deserilise JSON file {directoryAddress}/{file} failed. Please correct file and try again.");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Initilise file {directoryAddress}/{file} failed. {ex.Message}");
                    }
                }
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new DirectoryNotFoundException($"Directory {directoryAddress} does not exist.");
            }

        }
        #endregion

        #region Search

        public string GetSearchResult(string entityToSearch, string propertyToSearch, object valueToSearch)
        {
            var entities = Search(entityToSearch, propertyToSearch, valueToSearch);
            return entities == null || !entities.Any() ? TextConsts.NoResult : Entity.ToConsoleString(entities);
        }

        public List<Entity> Search(string entityToSearch, string propertyToSearch, object valueToSearch)
        {
            if (!CahcedEntityCollection.EntityCollection.ContainsKey(entityToSearch)) return null;
            if (propertyToSearch == nameof(Entity._id))
            {
                var entity = SearchById(entityToSearch, valueToSearch.ToString());
                if (entity == null) return null;
                IncludeRelatedEntities(entity);
                return new List<Entity> { entity };
            }
            else
            {
                var entities = SearchByProperty(entityToSearch, propertyToSearch, valueToSearch);
                if (entities == null || !entities.Any()) return null;
                foreach (var entity in entities)
                {
                    IncludeRelatedEntities(entity);
                }
                return entities;
            }
        }

        /// <summary>
        /// Search by id field. id field is the key of the dictionary so needs to be treated separately to improve performance
        /// </summary>

        private Entity SearchById(string entityToSearch, object id)
        {
            if (id == null) return null;
            if (CahcedEntityCollection != null
                && CahcedEntityCollection.EntityCollection[entityToSearch] != null
                && CahcedEntityCollection.EntityCollection[entityToSearch].Entities.ContainsKey(id))
            {
                return CahcedEntityCollection.EntityCollection[entityToSearch]?.Entities[id];
            }
            return null;
        }

        private List<Entity> SearchByProperty(string entityToSearch, string propertyToSearch, object valueToSearch)
        {
            if (propertyToSearch == nameof(Entity._id))
            {
                var entity = SearchById(entityToSearch, valueToSearch);
                return entity == null ? null : new List<Entity> { entity };
            }
            var entities = new List<Entity>(); ;
            if (CahcedIndexingCollection.IndexingCollection.ContainsKey(entityToSearch) && CahcedIndexingCollection.IndexingCollection[entityToSearch].ContainsKey(propertyToSearch))
            {
                entities = SearchWithIndexing(entityToSearch, propertyToSearch, valueToSearch);
            }
            else
            {
                entities = SearchWithoutIndexing(entityToSearch, propertyToSearch, valueToSearch);
            }
            return entities;
        }


        private List<Entity> SearchWithIndexing(string entityToSearch, string propertyToSearch, object valueToSearch)
        {
            if (CahcedIndexingCollection.IndexingCollection.ContainsKey(entityToSearch)
                && CahcedIndexingCollection.IndexingCollection[entityToSearch].ContainsKey(propertyToSearch)
                && CahcedIndexingCollection.IndexingCollection[entityToSearch][propertyToSearch].Indexings != null)
            {
                if (CahcedIndexingCollection.IndexingCollection[entityToSearch][propertyToSearch].Indexings.ContainsKey(valueToSearch))
                {
                    var ids = CahcedIndexingCollection.IndexingCollection[entityToSearch][propertyToSearch].Indexings[valueToSearch];
                    return CahcedEntityCollection.EntityCollection[entityToSearch].Entities.Where(e => ids.Any(id => id == e.Key)).Select(e => e.Value).ToList();
                }
                else if(string.IsNullOrWhiteSpace(valueToSearch.ToString()))
                {
                    //To search field with no value, return all entity id that is not existing in the indexing cache
                    var idsToIgnore = CahcedIndexingCollection.IndexingCollection[entityToSearch][propertyToSearch].Indexings.Values;
                    return CahcedEntityCollection.EntityCollection[entityToSearch].Entities.Where(e => idsToIgnore.All(ids => ids.All(id => id != e.Key))).Select(e => e.Value).ToList();
                }
            }
            return null;
        }

        private List<Entity> SearchWithoutIndexing(string entityToSearch, string propertyToSearch, object valueToSearch)
        {
            var entities = CahcedEntityCollection.EntityCollection[entityToSearch].Entities;
            var filteredEntities = new List<Entity>();
            foreach (var entity in entities)
            {
                var propertyValue = entity.Value.GetAttribute<object>(propertyToSearch);
                if (propertyValue != null)
                {
                    if (propertyValue is List<string>)
                    {
                        if ((propertyValue as List<string>).Any(v => v == valueToSearch.ToString())) filteredEntities.Add(entity.Value);
                    }
                    else
                    {
                        if (propertyValue.Equals(valueToSearch)) filteredEntities.Add(entity.Value);
                    }
                }
                else
                {
                    if (String.IsNullOrWhiteSpace(valueToSearch.ToString())) filteredEntities.Add(entity.Value);
                }
            }
            return filteredEntities.Any() ? filteredEntities : null;
        }


        private void IncludeRelatedEntities(Entity entity)
        {
            var stringBuilder = new StringBuilder();
            var fromRelationships = Metadata.Relationships?.Where(r => r.FromEntity == entity.entityName).ToList();
            var toRelationships = Metadata.Relationships?.Where(r => r.ToEntity == entity.entityName).ToList();

            if (fromRelationships != null && fromRelationships.Any())
            {
                foreach (var fromRelationship in fromRelationships)
                {
                    var relatedEntities = GetFromRelatedEntity(entity, fromRelationship);
                    if (relatedEntities != null && relatedEntities.Any())
                    {
                        entity.Attributes.Add(new KeyValuePair<string, object>(GetFileNameByEntityName(fromRelationship.ToEntity), relatedEntities.Select(e => e.name).ToList()));
                    }
                }
            }
            if (toRelationships != null && toRelationships.Any())
            {
                foreach (var toRelationship in toRelationships)
                {
                    var relatedEntity = GetToRelatedEntity(entity, toRelationship)?.FirstOrDefault();
                    if (relatedEntity != null)
                    {
                        entity.Attributes.Add(new KeyValuePair<string, object>(toRelationship.ToProperty.Replace("_id", "_name"), relatedEntity.name));
                    }
                }
            }
        }


        private List<Entity> GetFromRelatedEntity(Entity entity, Relationship relationship)
        {
            return SearchByProperty(GetFileNameByEntityName(relationship.ToEntity), relationship.ToProperty, entity.GetAttribute<object>(relationship.FromProperty));
        }


        private List<Entity> GetToRelatedEntity(Entity entity, Relationship relationship)
        {
            return SearchByProperty(GetFileNameByEntityName(relationship.FromEntity), relationship.FromProperty, entity.GetAttribute<string>(relationship.ToProperty));
        }


        private string GetFileNameByEntityName(string entityName)
        {
            return Metadata.Entities.First(e => e.EntityName == entityName).FileName;
        }

        public List<EntityMetadata> GetEntityMetadatas()
        {
            if (Metadata == null) Initialise();
            return Metadata.Entities;
        }
        #endregion
    }
}
