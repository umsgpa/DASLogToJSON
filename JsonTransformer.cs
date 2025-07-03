using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

public class JsonTransformer
{
    public static string TransformDateTimeFormat(string originalJson)
    {
        // Parse the JSON into a JsonNode (which can be an array or object)
        var jsonNode = JsonNode.Parse(originalJson);
        
        if (jsonNode is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is JsonObject jsonObject)
                {
                    TransformDateTimeProperty(jsonObject);
                }
            }
        }
        else if (jsonNode is JsonObject singleObject)
        {
            TransformDateTimeProperty(singleObject);
        }
        
        return jsonNode != null 
            ? jsonNode.ToJsonString(new JsonSerializerOptions { WriteIndented = true }) 
            : string.Empty;
    }

    private static void TransformDateTimeProperty(JsonObject jsonObject)
    {
        if (jsonObject.TryGetPropertyValue("dateTime", out var dateTimeNode) && 
            dateTimeNode != null)
        {
            // Get the original date string
            string originalDate = dateTimeNode.ToString();
            
            // Create the new date object structure
            var newDateObject = new JsonObject
            {
                ["$date"] = originalDate
            };
            
            // Replace the original value with the new structure
            jsonObject["dateTime"] = newDateObject;
        }
    }
}