using Amazon.DynamoDBv2.Model;
using System.Collections;

namespace Bolt.DynamoDbClient;

internal static class AttributeValueMapper
{
    private static bool IsString(Type type) => type == typeof(string);
    private static bool IsGuid(Type type) => type == typeof(Guid) || type == typeof(Guid?);
    private static bool IsBool(Type type) => type == typeof(bool) || type == typeof(bool?);
    private static bool IsInt(Type type) => type == typeof(int) || type == typeof(int?);
    private static bool IsLong(Type type) => type == typeof(long) || type == typeof(long?);
    private static bool IsDouble(Type type) => type == typeof(double) || type == typeof(double?);
    private static bool IsDecimal(Type type) => type == typeof(decimal) || type == typeof(decimal?);
    private static bool IsFloat(Type type) => type == typeof(float) || type == typeof(float?);
    private static bool IsDateTime(Type type) => type == typeof(DateTime) || type == typeof(DateTime?);
    private static bool IsDateTimeOffset(Type type) => type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?);

    private static DateTime ParseDateTime(string source)
    {
        var date = DateTime.Parse(source);
        return date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
    }

    private static DateTimeOffset ParseDateTimeOffset(string source)
    {
        var date = DateTime.Parse(source);
        return date.Kind == DateTimeKind.Utc ? date : date.ToUniversalTime();
    }

    private static DateTime GetDateTime(AttributeValue attr)
    {
        return ParseDateTime(attr.S);
    }
    private static DateTimeOffset GetDateTimeOffset(AttributeValue attr)
    {
        return ParseDateTimeOffset(attr.S);
    }

    public static object? MapTo(Type type, AttributeValue attr, int level = 0)
    {
        if (attr.NULL) return null;
        
        if (IsString(type)) return attr.S;
        if (IsGuid(type)) return new Guid(attr.S);
        if (type.IsEnum) return Enum.Parse(type, attr.S);
        
        if (Nullable.GetUnderlyingType(type) is { IsEnum: true } underlyingEnumType)
        {
            return string.IsNullOrWhiteSpace(attr.S) ? null : Enum.Parse(underlyingEnumType, attr.S);
        }
        
        if (IsDateTime(type)) return GetDateTime(attr);
        if (IsDateTimeOffset(type)) return GetDateTimeOffset(attr);
        if (IsInt(type)) return int.Parse(attr.N);
        if (IsLong(type)) return long.Parse(attr.N);
        if (IsDecimal(type)) return decimal.Parse(attr.N);
        if (IsDouble(type)) return double.Parse(attr.N);
        if (IsFloat(type)) return float.Parse(attr.N);
        if (IsBool(type)) return attr.BOOL;

        if (type.IsArray)
        {
            return MapToArray(type, attr);
        }

        var metaData = DynamoDbItemMetaDataReader.Get(type);

        if(metaData.TypeInfoMetaData?.IsDictionary ?? false)
        {
            return MapToDictionary(type, attr);
        }

        if (metaData.TypeInfoMetaData?.IsCollection ?? false)
        {
            return MapToCollection(type, metaData.TypeInfoMetaData?.CollectionItemType, attr);
        }

        return MapToObject(type, attr);
    }


    private static object? MapToCollection(Type type, Type? elementType, AttributeValue attr)
    {
        if (attr.NULL) return null;
        if(elementType == null) return null;

        var listType = typeof(List<>).MakeGenericType(elementType);
        var result = Activator.CreateInstance(listType) as IList;

        if (result == null) return null;

        if (IsString(elementType))
        {
            foreach (var value in attr.SS)
            {
                result.Add(value);
            }
        }
        else if (IsGuid(elementType))
        {
            foreach (var value in attr.SS)
            {
                result.Add(Guid.Parse(value));
            }
        }
        else if (IsDateTime(elementType))
        {
            foreach (var value in attr.SS)
            {
                result.Add(ParseDateTime(value));
            }
        }

        else if (IsDateTimeOffset(elementType))
        {
            foreach (var value in attr.SS)
            {
                result.Add(ParseDateTimeOffset(value));
            }
        }

        else if (IsInt(elementType))
        {
            foreach (var value in attr.NS)
            {
                result.Add(int.Parse(value));
            }
        }

        else if (IsDecimal(elementType))
        {
            foreach (var value in attr.NS)
            {
                result.Add(decimal.Parse(value));
            }
        }

        else if (IsDouble(elementType))
        {
            foreach (var value in attr.NS)
            {
                result.Add(double.Parse(value));
            }
        }

        else if (IsLong(elementType))
        {
            foreach (var value in attr.NS)
            {
                result.Add(long.Parse(value));
            }
        }

        else if (IsFloat(elementType))
        {
            foreach (var value in attr.NS)
            {
                result.Add(float.Parse(value));
            }
        }

        else if (IsLong(elementType))
        {
            foreach (var value in attr.NS)
            {
                result.Add(long.Parse(value));
            }
        }
        else
        {
            foreach (var value in attr.L)
            {
                result.Add(MapTo(elementType, value));
            }
        }

        return result;
    }

    public static object? MapToDictionary(Type type, AttributeValue attr)
    {
        if(attr.M == null || attr.NULL) return null;

        var keyValueTypes = type.GetGenericArguments();
        if (keyValueTypes.Length != 2) return null;
        var keyType = keyValueTypes[0];
        var valueType = keyValueTypes[1];

        if (keyType != typeof(string)) return null;

        var dic = Activator.CreateInstance(type) as IDictionary;

        if(dic == null) return null;

        foreach( var keyValue in attr.M )
        {
            dic[keyValue.Key] = MapTo(valueType, keyValue.Value);
        }

        return dic;
    }

    private static object? MapToObject(Type type, AttributeValue attr)
    {
        if(attr.M == null || attr.NULL) return null;

        var metaData = DynamoDbItemMetaDataReader.Get(type);

        if(metaData.TypeInfoMetaData?.IsDictionary ?? false)
        {
            var dic = Activator.CreateInstance(type) as IDictionary;

            if (dic == null) return null;

            var dicValueType = metaData.TypeInfoMetaData?.DictionaryValueType;

            if (dicValueType != null)
            {
                foreach (var item in attr.M)
                {
                    dic[item.Key] = MapTo(dicValueType, item.Value);
                }
            }

            return dic;
        }

        var result = Activator.CreateInstance(type);

        foreach (var property in metaData.Properties)
        {
            var att = attr.M.GetValueOrDefault(property.ColumnName);

            if (att == null) continue;

            property.PropertyInfo.SetValue(result, MapTo(property.PropertyInfo.PropertyType, att));
        }

        return result;
    }

    private static object? MapToArray(Type type, AttributeValue attr)
    {
        if (attr.NULL) return null;
        
        var elementType = type.GetElementType();

        if(elementType == null) return null;

        if(IsString(elementType))
        {
            return MapToStringArray(elementType, attr, (val) => val);
        }
        
        if (IsGuid(elementType))
        {
            return MapToStringArray(elementType, attr, (val) => new Guid(val));
        }

        if (IsDateTime(elementType))
        {
            return MapToStringArray(elementType, attr, (val) => ParseDateTime(val));
        }

        if (IsDateTimeOffset(elementType))
        {
            return MapToStringArray(elementType, attr, (val) => ParseDateTimeOffset(val));
        }

        if (elementType.IsEnum)
        {
            return MapToStringArray(elementType, attr, (val) => Enum.Parse(elementType, val));
        }

        if (IsInt(elementType))
        {
            return MapToNumericArray(elementType, attr, v => int.Parse(v));
        }

        if (IsDecimal(elementType))
        {
            return MapToNumericArray(elementType, attr, v => decimal.Parse(v));
        }

        if (IsDouble(elementType))
        {
            return MapToNumericArray(elementType, attr, v => double.Parse(v));
        }

        if (IsFloat(elementType))
        {
            return MapToNumericArray(elementType, attr, v => float.Parse(v));
        }

        if (IsLong(elementType))
        {
            return MapToNumericArray(elementType, attr, v => long.Parse(v));
        }


        return MapToObjectArray(elementType, attr);
    }

    private static object? MapToObjectArray(Type elementType, AttributeValue attr)
    {
        if (attr.L == null || attr.NULL) return null;

        var collection = Array.CreateInstance(elementType, attr.L.Count);

        var index = 0;
        foreach (var item in attr.L)
        {
            collection.SetValue(MapTo(elementType, item), index);
            index++;
        }

        return collection;
    }

    private static object? MapToStringArray(Type elementType, AttributeValue attr, Func<string, object> getValueFromString)
    {
        if (attr.SS == null || attr.NULL) return null;

        var collection = Array.CreateInstance(elementType, attr.SS.Count);

        var index = 0;
        foreach (var item in attr.SS)
        {
            collection.SetValue(getValueFromString(item), index);
            index++;
        }

        return collection;
    }

    private static object? MapToNumericArray(Type elementType, AttributeValue attr, Func<string, object> getValueFromString)
    {
        if (attr.NS == null || attr.NULL) return null;

        var collection = Array.CreateInstance(elementType, attr.NS.Count);

        var index = 0;
        foreach (var item in attr.NS)
        {
            collection.SetValue(getValueFromString(item), index);
            index++;
        }

        return collection;
    }

    public static AttributeValue? MapFrom(object value)
    {
        if (value == null) return null;

        if (value is string strValue) return MapFromString(strValue);
        if (value is DateTime dateTime) return MapFromString(dateTime.ToUtcString());
        if (value is DateTimeOffset dateTimeOffset) return MapFromString(dateTimeOffset.DateTime.ToUtcString());
        if (value is Guid guidValue) return MapFromString(guidValue.ToString());
        if (value.GetType().IsEnum) return MapFromString(value.ToString() ?? string.Empty);
        if(IsNumeric(value)) return MapFromNumber(value);
        if(value is bool boolValue) return MapFromBoolean(boolValue);
        if (value is IDictionary) return MapFromDictionary(value);
        if(value is IEnumerable) return MapFromEnumerable(value);
        
        return MapFromObject(value);
    }

    private static AttributeValue? MapFromDictionary(object value)
    {        
        var args = value.GetType().GetGenericArguments();

        if (args.Length != 2) return null;

        if (args[0] != typeof(string)) return null;

        if(value is IDictionary dictionary)
        {
            var result = new Dictionary<string, AttributeValue>();

            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key == null) continue;

                var item = entry.Value;

                if(item == null) continue;

                var att = MapFrom(item);

                if(att != null)
                {
                    result[entry.Key.ToString() ?? string.Empty] = att;
                }
            }

            return new AttributeValue
            {
                M = result
            };
        }

        return null;
    }

    private static AttributeValue MapFromObject(object value)
    {
        var properties = value.GetType().GetProperties().Where(x => x.CanRead);

        var result = new Dictionary<string, AttributeValue>();

        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(value, null);

            if(propertyValue == null) continue;

            var att = MapFrom(propertyValue);

            if(att != null)
            {
                result[property.Name] = att;
            }
        }

        return new AttributeValue
        {
            M = result
        };
    }

    private static AttributeValue? MapFromEnumerable(object value)
    {
        if(value is IEnumerable<string> strArr)
        {
            return new AttributeValue
            {
                SS = strArr.ToList()
            };
        }

        if (value is IEnumerable<Guid> guidArr)
        {
            return new AttributeValue
            {
                SS = guidArr.Select(x => x.ToString()).ToList()
            };
        }

        if (value is IEnumerable<int> intArr)
        {
            return new AttributeValue
            {
                NS = intArr.Select(x => x.ToString()).ToList()
            };
        }

        if (value is IEnumerable<double> doubleArr)
        {
            return new AttributeValue
            {
                NS = doubleArr.Select(x => x.ToString()).ToList()
            };
        }

        if (value is IEnumerable<decimal> dcmArr)
        {
            return new AttributeValue
            {
                NS = dcmArr.Select(x => x.ToString()).ToList()
            };
        }

        if (value is IEnumerable<long> lngArr)
        {
            return new AttributeValue
            {
                NS = lngArr.Select(x => x.ToString()).ToList()
            };
        }


        if(value is IEnumerable collection)
        {
            var attributes = new List<AttributeValue>();

            foreach(var item in collection)
            {
                if(item != null)
                {
                    var att = MapFrom(item);

                    if(att != null)
                    {
                        attributes.Add(att);
                    }
                }
            }

            return new AttributeValue
            {
                L = attributes
            };
        }

        return null;
    }

    private static AttributeValue? MapFromBoolean(bool value)
    {
        return new AttributeValue
        {
            BOOL = value
        };
    }

    private static AttributeValue? MapFromString(string value)
    {
        return new AttributeValue
        {
            S = value
        };
    }

    private static AttributeValue? MapFromNumber(object value)
    {
        return new AttributeValue
        {
            N = value.ToString()
        };
    }

    private static bool IsNumeric(object value)
    {
        return value is sbyte
                    || value is byte
                    || value is short
                    || value is ushort
                    || value is int
                    || value is uint
                    || value is long
                    || value is ulong
                    || value is float
                    || value is double
                    || value is decimal;
    }
}



