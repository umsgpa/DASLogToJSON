using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

public class JsonTransformer
{
    public static string TransformDateTimeFormat(
        string originalJson, 
        IEnumerable<string> propertyNames,
        StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        var jsonNode = JsonNode.Parse(originalJson);
        var propertyNameSet = new HashSet<string>(propertyNames, StringComparer.FromComparison(comparisonType));

        if (jsonNode is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is JsonObject jsonObject)
                {
                    TransformDateTimeProperties(jsonObject, propertyNameSet);
                }
            }
        }
        else if (jsonNode is JsonObject singleObject)
        {
            TransformDateTimeProperties(singleObject, propertyNameSet);
        }
        
        return jsonNode != null 
            ? jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) 
            : string.Empty;
    }

    private static void TransformDateTimeProperties(JsonObject jsonObject, HashSet<string> propertyNames)
    {
        // Get all property names that match our target names
        foreach (var propertyName in propertyNames)
        {
            if (jsonObject.TryGetPropertyValue(propertyName, out var dateNode) && 
                dateNode != null)
            {
                if (dateNode is JsonValue jsonValue && jsonValue.TryGetValue<string>(out var dateString) && dateString != null)
                {
                    jsonObject[propertyName] = new JsonObject
                    {
                        ["$date"] = dateString
                    };
                }
            }
        }
    }
}