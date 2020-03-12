using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TableEntites.ComplexObjectsStamping
{
    public class ComplexStorageEntity<T> : TableEntity where T: new()
    {
        [StoreInAzureTable]
        public T ItemToStore { get; set; }
        public DateTime DeferralDate { get; set; }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);
            Deserialize(this, properties);
        }
        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var results = base.WriteEntity(operationContext);
            Serialize(this, results);
            return results;
        }

        private static void Serialize<TEntity>(TEntity entity, IDictionary<string, EntityProperty> results)
        {
            entity.GetType().GetProperties()
                .Where(a => a.GetCustomAttribute(typeof(StoreInAzureTableAttribute))!=null)
                .ToList()
                .ForEach(x =>
                {
                    if (!results.ContainsKey(x.Name))
                    {
                        results.Add(x.Name, new EntityProperty(JsonConvert.SerializeObject(x.GetValue(entity))));
                    }
                });
        }

        private static void Deserialize<TEntity>(TEntity entity, IDictionary<string, EntityProperty> properties)
        {
            //var currentAssembly = Assembly.GetExecutingAssembly();
            entity.GetType().GetProperties()
                .Where(a => a.GetCustomAttribute(typeof(StoreInAzureTableAttribute)) != null)
                .ToList()
                .ForEach(x => x.SetValue(entity, JsonConvert.DeserializeObject(properties[x.Name].StringValue, x.PropertyType)));
        }
    }

    internal class StoreInAzureTableAttribute : Attribute
    {

    }
}
