using Amazon.DynamoDBv2.Model;
using Bolt.Common.Extensions;
using Shouldly;

namespace Bolt.DynamoDbClient.Tests;

public class AttributeValueMapper_MapTo_Should
{
    [Fact]
    public void Return_correct_string()
    {
        var att = new AttributeValue
        {
            S = "Hello"
        };

        var result = AttributeValueMapper.MapTo(typeof(string), att);

        result.ShouldNotBeNull();
        result.ToString().ShouldBe("Hello");
    }

    [Fact]
    public void Return_correct_int()
    {
        var att = new AttributeValue
        {
            N = "1"
        };

        var result = AttributeValueMapper.MapTo(typeof(int), att);

        result.ShouldNotBeNull();
        result.ShouldBe(1);
    }

    [Fact]
    public void Return_correct_dictionary_of_object()
    {
        var att = new AttributeValue 
        { 
            M = new Dictionary<string, AttributeValue>
            {
                ["4959100B-106C-4B04-AC98-A4040809A4C4"] = new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        ["Id"] = new AttributeValue
                        {
                            S = "4959100B-106C-4B04-AC98-A4040809A4C4"
                        },
                        ["_Name"] = new AttributeValue
                        {
                            S = "Ruhul"
                        }
                    }
                },
                ["34F68347-27AE-47ED-AA4B-1F08A29C38C4"] = new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        ["Id"] = new AttributeValue
                        {
                            S = "34F68347-27AE-47ED-AA4B-1F08A29C38C4"
                        },
                        ["_Name"] = new AttributeValue
                        {
                            S = "Amin"
                        }
                    }
                }
            }
        };

        var result = AttributeValueMapper.MapTo(typeof(Dictionary<string,Person>), att);

        result.ShouldNotBeNull();
        (result as Dictionary<string, Person>).Count.ShouldBe(2);
    }

    [Fact]
    public void Return_correct_array_of_object()
    {
        var att = new AttributeValue
        {
            L = new List<AttributeValue>
            {
                new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        ["Id"] = new AttributeValue
                        {
                            S = "4959100B-106C-4B04-AC98-A4040809A4C4"
                        },
                        ["_Name"] = new AttributeValue
                        {
                            S = "Ruhul"
                        }
                    }
                },
                new AttributeValue
                {
                    M = new Dictionary<string, AttributeValue>
                    {
                        ["Id"] = new AttributeValue
                        {
                            S = "4959100B-106C-4B04-AC98-A4040809A4C4"
                        },
                        ["_Name"] = new AttributeValue
                        {
                            S = "Amin"
                        }
                    }
                }
            }
        };

        var result = AttributeValueMapper.MapTo(typeof(Person[]), att);

        result.ShouldNotBeNull();
        (result as Person[]).Length.ShouldBe(2);
    }



    [Fact]
    public void Return_correct_array_of_string()
    {
        var att = new AttributeValue
        {
            SS = new List<string> { "hello", "world"}
        };

        var result = AttributeValueMapper.MapTo(typeof(string[]), att);

        result.ShouldNotBeNull();
        (result as string[]).Length.ShouldBe(2);
    }


    [Fact]
    public void Return_correct_array_of_int()
    {
        var att = new AttributeValue
        {
            NS = new List<string> { "1", "2" }
        };

        var result = AttributeValueMapper.MapTo(typeof(int[]), att);

        result.ShouldNotBeNull();
        (result as int[]).Length.ShouldBe(2);
    }
}

public class AttributeValueMapper_MapFrom_Should
{
    [Fact]
    public void Retrun_correct_value_for_string()
    {
        var result = AttributeValueMapper.MapFrom("testing");

        result.ShouldNotBeNull();
        result.S.ShouldBe("testing");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1.2)]
    public void Retrun_correct_value_for_number(object value)
    {
        var result = AttributeValueMapper.MapFrom(value);

        result.ShouldNotBeNull();
        result.N.ShouldBe(value.ToString());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Retrun_correct_value_for_bool(object value)
    {
        bool? boolValue = (bool?)value;
        var result = AttributeValueMapper.MapFrom(boolValue);

        result.ShouldNotBeNull();
        result.BOOL.ShouldBe((bool)value);
    }

    [Fact]
    public void Retrun_correct_value_for_nullable_bool()
    {
        var result = AttributeValueMapper.MapFrom((bool?)null);

        result.ShouldBeNull();
    }

    [Fact]
    public void Return_correct_value_for_array_string()
    {
        var names = new string[] { "ruhul", "amin" };
        var result = AttributeValueMapper.MapFrom(names);
        result.ShouldNotBeNull();
        result.SS.Count.ShouldBe(2);
    }



    [Fact]
    public void Return_correct_value_for_array_number()
    {
        var names = new int[] { 1, 2 };
        var result = AttributeValueMapper.MapFrom(names);
        result.ShouldNotBeNull();
        result.NS.Count.ShouldBe(2);
    }


    [Fact]
    public void Return_correct_value_for_array_object()
    {
        var names = new Person[] { new() { Id = new Guid("24A36D9C-3D59-45B5-B1D6-87AF24E2AD4B"), Name ="ruhul" }, };
        var result = AttributeValueMapper.MapFrom(names);
        result.ShouldNotBeNull();
        result.L.Count.ShouldBe(1);
    }


    [Fact]
    public void Return_correct_value_for_object()
    {
        var names = new { Name = "ruhul", Age = 99 };
        var result = AttributeValueMapper.MapFrom(names);
        result.ShouldNotBeNull();
        result.M.Count.ShouldBe(2);
    }


    [Fact]
    public void Return_correct_value_for_string_dictionary()
    {
        var data = new Dictionary<string, string> { ["VIC"] = "Victoria", ["TAS"] = "Tasmania" };
        var result = AttributeValueMapper.MapFrom(data);
        result.ShouldNotBeNull();
        result.M.Count.ShouldBe(2);
    }

    [Fact]
    public void Return_correct_value_for_guid_dictionary()
    {
        var data = new Dictionary<string, Guid?> { ["VIC"] = new Guid("24A36D9C-3D59-45B5-B1D6-87AF24E2AD4B"), ["TAS"] = null };
        var result = AttributeValueMapper.MapFrom(data);
        result.ShouldNotBeNull();
        result.M.Count.ShouldBe(1);
    }

    [Fact]
    public void Return_correct_value_for_dictionary_with_object()
    {
        var data = new Dictionary<string, Person> { ["1"] = new Person { Id = new Guid("24A36D9C-3D59-45B5-B1D6-87AF24E2AD4B"), Name = "name 1" } };
        var result = AttributeValueMapper.MapFrom(data);
        result.ShouldNotBeNull();
        result.M.Count.ShouldBe(1);
    }
}

public class Person
{
    public required Guid Id { get; init; }
    [DynamoDbColumn("_Name")]
    public required string Name { get; init; }
}



