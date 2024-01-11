namespace RocketQL.Core.UnitTests.SchemaValidation;

public class IsInputTypeCompatibleWithValue : UnitTestBase
{
    [Theory]
    [InlineData("Int = null")]
    [InlineData("Int = 42")]
    [InlineData("Float = null")]
    [InlineData("Float = 42")]
    [InlineData("Float = 3.142")]
    [InlineData("String = null")]
    [InlineData("String = \"foo\"")]
    [InlineData("Boolean = null")]
    [InlineData("Boolean = true")]
    [InlineData("Boolean = false")]
    [InlineData("ID = null")]
    [InlineData("ID = \"foo\"")]
    [InlineData("Name = null")]
    [InlineData("Name = \"foo\"")]
    public void ValidNullableScalar(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("Int! = 42")]
    [InlineData("Float! = 42")]
    [InlineData("Float! = 3.142")]
    [InlineData("String! = \"foo\"")]
    [InlineData("Boolean! = true")]
    [InlineData("Boolean! = false")]
    [InlineData("ID! = \"foo\"")]
    [InlineData("Name! = \"foo\"")]
    public void ValidNonNullableScalar(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("Int = 3.142")]
    [InlineData("Int = \"foo\"")]
    [InlineData("Int = true")]
    [InlineData("Int = false")]
    [InlineData("Float = \"foo\"")]
    [InlineData("Float = true")]
    [InlineData("Float = false")]
    [InlineData("String = 42")]
    [InlineData("String = 3.142")]
    [InlineData("String = true")]
    [InlineData("String = false")]
    [InlineData("Boolean = 42")]
    [InlineData("Boolean = 3.142")]
    [InlineData("Boolean = \"foo\"")]
    [InlineData("ID = 42")]
    [InlineData("ID = 3.142")]
    [InlineData("ID = true")]
    [InlineData("ID = false")]
    [InlineData("Name = 42")]
    [InlineData("Name = 3.142")]
    [InlineData("Name = true")]
    [InlineData("Name = false")]
    public void InvalidNullableScalarWrongValueType(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("Int! = 3.142")]
    [InlineData("Int! = \"foo\"")]
    [InlineData("Int! = true")]
    [InlineData("Int! = false")]
    [InlineData("Float! = \"foo\"")]
    [InlineData("Float! = true")]
    [InlineData("Float! = false")]
    [InlineData("String! = 42")]
    [InlineData("String! = 3.142")]
    [InlineData("String! = true")]
    [InlineData("String! = false")]
    [InlineData("Boolean! = 42")]
    [InlineData("Boolean! = 3.142")]
    [InlineData("Boolean! = \"foo\"")]
    [InlineData("ID! = 42")]
    [InlineData("ID! = 3.142")]
    [InlineData("ID! = true")]
    [InlineData("ID! = false")]
    [InlineData("Name! = 42")]
    [InlineData("Name! = 3.142")]
    [InlineData("Name! = true")]
    [InlineData("Name! = false")]
    public void InvalidNonNullableScalarWrongValueType(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("Int! = null")]
    [InlineData("Float! = null")]
    [InlineData("String! = null")]
    [InlineData("Boolean! = null")]
    [InlineData("ID! = null")]
    [InlineData("Name! = null")]
    public void InvalidNonNullableScalarCannotBeNull(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("[Int] = null")]
    [InlineData("[Float] = null")]
    [InlineData("[String] = null")]
    [InlineData("[Boolean] = null")]
    [InlineData("[ID] = null")]
    [InlineData("[Name] = null")]
    [InlineData("[Int] = []")]
    [InlineData("[Float] = []")]
    [InlineData("[String] = []")]
    [InlineData("[Boolean] = []")]
    [InlineData("[ID] = []")]
    [InlineData("[Name] = []")]
    [InlineData("[Int] = [null]")]
    [InlineData("[Float] = [null]")]
    [InlineData("[String] = [null]")]
    [InlineData("[Boolean] = [null]")]
    [InlineData("[ID] = [null]")]
    [InlineData("[Name] = [null]")]
    [InlineData("[Int] = [42]")]
    [InlineData("[Float] = [42]")]
    [InlineData("[Float] = [3.142]")]
    [InlineData("[String] = [\"foo\"]")]
    [InlineData("[Boolean] = [true]")]
    [InlineData("[ID] = [\"foo\"]")]
    [InlineData("[Name] = [\"foo\"]")]
    [InlineData("[Int] = [null, 42]")]
    [InlineData("[Int] = [42, null]")]
    [InlineData("[Int] = [42, 43, 44, 45]")]
    public void ValidNullableListNullableScalar(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("[Int!] = null")]
    [InlineData("[Float!] = null")]
    [InlineData("[String!] = null")]
    [InlineData("[Boolean!] = null")]
    [InlineData("[ID!] = null")]
    [InlineData("[Name!] = null")]
    [InlineData("[Int!] = []")]
    [InlineData("[Float!] = []")]
    [InlineData("[String!] = []")]
    [InlineData("[Boolean!] = []")]
    [InlineData("[ID!] = []")]
    [InlineData("[Name!] = []")]
    [InlineData("[Int!] = [42]")]
    [InlineData("[Float!] = [42]")]
    [InlineData("[Float!] = [3.142]")]
    [InlineData("[String!] = [\"foo\"]")]
    [InlineData("[Boolean!] = [true]")]
    [InlineData("[ID!] = [\"foo\"]")]
    [InlineData("[Name!] = [\"foo\"]")]
    [InlineData("[Int!] = [42, 43, 44, 45]")]
    public void ValidNullableListNonNullableScalar(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("[Int]! = []")]
    [InlineData("[Float]! = []")]
    [InlineData("[String]! = []")]
    [InlineData("[Boolean]! = []")]
    [InlineData("[ID]! = []")]
    [InlineData("[Name]! = []")]
    [InlineData("[Int]! = [null]")]
    [InlineData("[Float]! = [null]")]
    [InlineData("[String]! = [null]")]
    [InlineData("[ID]! = [null]")]
    [InlineData("[Boolean]! = [null]")]
    [InlineData("[Name]! = [null]")]
    [InlineData("[Int]! = [42]")]
    [InlineData("[Float]! = [3.142]")]
    [InlineData("[String]! = [\"foo\"]")]
    [InlineData("[Boolean]! = [true]")]
    [InlineData("[ID]! = [\"foo\"]")]
    [InlineData("[Name]! = [\"foo\"]")]
    [InlineData("[Int]! = [null, 42]")]
    [InlineData("[Int]! = [42, null]")]
    [InlineData("[Int]! = [42, 43, 44, 45]")]
    public void ValidNonNullableListNullableScalar(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("[Int!]! = []")]
    [InlineData("[Float!]! = []")]
    [InlineData("[String!]! = []")]
    [InlineData("[Boolean!]! = []")]
    [InlineData("[ID!]! = []")]
    [InlineData("[Name!]! = []")]
    [InlineData("[Int!]! = [42]")]
    [InlineData("[Float!]! = [3.142]")]
    [InlineData("[String!]! = [\"foo\"]")]
    [InlineData("[Boolean!]! = [true]")]
    [InlineData("[ID!]! = [\"foo\"]")]
    [InlineData("[Name!]! = [\"foo\"]")]
    [InlineData("[Int!]! = [42, 43, 44, 45]")]
    public void ValidNonNullableListNonNullableScalar(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("[Int] = [3.142]")]
    [InlineData("[Int] = [\"foo\"]")]
    [InlineData("[Int] = [true]")]
    [InlineData("[Int] = [false]")]
    [InlineData("[Float] = [\"foo\"]")]
    [InlineData("[Float] = [true]")]
    [InlineData("[Float] = [false]")]
    [InlineData("[String] = [42]")]
    [InlineData("[String] = [3.142]")]
    [InlineData("[String] = [true]")]
    [InlineData("[String] = [false]")]
    [InlineData("[Boolean] = [42]")]
    [InlineData("[Boolean] = [3.142]")]
    [InlineData("[Boolean] = [\"foo\"]")]
    [InlineData("[ID] = [42]")]
    [InlineData("[ID] = [3.142]")]
    [InlineData("[ID] = [true]")]
    [InlineData("[ID] = [false]")]
    [InlineData("[Name] = [42]")]
    [InlineData("[Name] = [3.142]")]
    [InlineData("[Name] = [true]")]
    [InlineData("[Name] = [false]")]
    public void InvalidNullableListNullableScalar(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("[Int!] = [null]")]
    [InlineData("[Int!] = [3.14]")]
    [InlineData("[Int!] = [true]")]
    [InlineData("[Int!] = [false]")]
    [InlineData("[Int!] = [\"foo\"]")]
    [InlineData("[Float!] = [true]")]
    [InlineData("[String!] = [true]")]
    [InlineData("[Boolean!] = [\"foo\"]")]
    [InlineData("[ID!] = [3.142]")]
    [InlineData("[Name!] = [3.142]")]
    public void InvalidNullableListNonNullableScalar(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("[Int]! = null")]
    [InlineData("[Int]! = [3.14]")]
    [InlineData("[Int]! = [true]")]
    [InlineData("[Int]! = [false]")]
    [InlineData("[Int]! = [\"foo\"]")]
    [InlineData("[Float]! = [false]")]
    [InlineData("[String]! = [true]")]
    [InlineData("[Boolean]! = [\"foo\"]")]
    [InlineData("[ID]! = [3.142]")]
    [InlineData("[Name]! = [3.142]")]
    public void InvalidNonNullableListNullableScalar(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("[Int!]! = null")]
    [InlineData("[Int!]! = [null]")]
    [InlineData("[Int!]! = [3.14]")]
    [InlineData("[Int!]! = [true]")]
    [InlineData("[Int!]! = [false]")]
    [InlineData("[Int!]! = [\"foo\"]")]
    [InlineData("[Float!]! = [null]")]
    [InlineData("[String!]! = [true]")]
    [InlineData("[Boolean!] = [\"foo\"]")]
    [InlineData("[ID!]! = [3.142]")]
    [InlineData("[Name!]! = [3.142]")]
    public void InvalidNonNullableListNonNullableScalar(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("Country = null")]
    [InlineData("Country = AUS")]
    public void ValidNullableEnum(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("Country! = AUS")]
    public void ValidNonNullableEnum(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("Country = 42")]
    [InlineData("Country = 3.142")]
    [InlineData("Country = \"foo\"")]
    [InlineData("Country = true")]
    public void InvalidNullableEnumWrongValueType(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("Country! = 42")]
    [InlineData("Country! = 3.142")]
    [InlineData("Country! = \"foo\"")]
    [InlineData("Country! = true")]
    public void InvalidNonNullableEnumWrongValueType(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("Country! = null")]
    public void InvalidNonNullableEnumCannotBeNull(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("[Country] = null")]
    [InlineData("[Country] = []")]
    [InlineData("[Country] = [null]")]
    [InlineData("[Country] = [AUS]")]
    [InlineData("[Country] = [null, AUS]")]
    [InlineData("[Country] = [AUS, null]")]
    [InlineData("[Country] = [AUS, NZ, USA]")]
    public void ValidNullableListNullableEnum(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("[Country!] = null")]
    [InlineData("[Country!] = []")]
    [InlineData("[Country!] = [AUS]")]
    [InlineData("[Country!] = [AUS, NZ, USA]")]
    public void ValidNullableListNonNullableEnum(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("[Country]! = []")]
    [InlineData("[Country]! = [null]")]
    [InlineData("[Country]! = [AUS]")]
    [InlineData("[Country]! = [null, AUS]")]
    [InlineData("[Country]! = [AUS, null]")]
    [InlineData("[Country]! = [AUS, NZ, USA]")]
    public void ValidNonNullableListNullableEnumr(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("[Country!]! = []")]
    [InlineData("[Country!]! = [AUS]")]
    [InlineData("[Country!]! = [AUS, NZ, USA]")]
    public void ValidNonNullableListNonNullableEnum(string compare)
    {
        Valid(compare);
    }

    [Theory]
    [InlineData("[Country] = [42]")]
    [InlineData("[Country] = [3.142]")]
    [InlineData("[Country] = [\"foo\"]")]
    [InlineData("[Country] = [true]")]
    [InlineData("[Country] = [false]")]
    public void InvalidNullableListNullableEnum(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("[Country!] = [null]")]
    [InlineData("[Country!] = [42]")]
    [InlineData("[Country!] = [3.14]")]
    [InlineData("[Country!] = [true]")]
    [InlineData("[Country!] = [false]")]
    [InlineData("[Country!] = [\"foo\"]")]
    public void InValidNullableListNonNullableEnum(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("[Country]! = null")]
    [InlineData("[Country]! = [42]")]
    [InlineData("[Country]! = [3.14]")]
    [InlineData("[Country]! = [true]")]
    [InlineData("[Country]! = [false]")]
    [InlineData("[Country]! = [\"foo\"]")]
    public void InValidNonNullableListNullableEnum(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("[Country!]! = null")]
    [InlineData("[Country!]! = [null]")]
    [InlineData("[Country!]! = [42]")]
    [InlineData("[Country!]! = [3.14]")]
    [InlineData("[Country!]! = [true]")]
    [InlineData("[Country!]! = [false]")]
    [InlineData("[Country!]! = [\"foo\"]")]
    public void InValidNonNullableListNonNullableEnum(string compare)
    {
        Invalid(compare);
    }

    [Theory]
    [InlineData("input T { f: Int }",                                   "T = null")]
    [InlineData("input T { f: Int }",                                   "T = { }")]
    [InlineData("input T { f: Int }",                                   "T = { f: null }")]
    [InlineData("input T { f: Int }",                                   "T = { f: 42 }")]
    [InlineData("input T { f: Float }",                                 "T = { f: 3.14 }")]
    [InlineData("input T { f: Boolean }",                               "T = { f: true }")]
    [InlineData("input T { f: String }",                                "T = { f: \"foo\" }")]
    [InlineData("input T { f: ID }",                                    "T = { f: null }")]
    [InlineData("input T { f: ID }",                                    "T = { f: \"foo\" }")]
    [InlineData("input T { f: Name }",                                  "T = { f: \"foo\" }")]
    [InlineData("input T { f: Country }",                               "T = { f: null }")]
    [InlineData("input T { f: Country }",                               "T = { f: AUS }")]
    [InlineData("input T { f: Int g:ID h:Name }",                       "T = { f: 42 g: \"foo\" h: \"foo\" }")]
    [InlineData("input T { f: Int g:ID h:Name }",                       "T = { f: null g: null h: null }")]
    [InlineData("input T { f: Int g: T }",                              "T = { }")]
    [InlineData("input T { f: Int g: T }",                              "T = { g: null }")]
    [InlineData("input T { f: Int g: T }",                              "T = { g: { } }")]
    [InlineData("input T { f: Int g: T }",                              "T = { g: { f: 42 } }")]
    [InlineData("input T { f: Int g: T }",                              "T = { g: { f: 42 g: { f: 43 } } }")]
    [InlineData("input T { f: Int g: T }",                              "T = { g: { f: 42 g: { g: { f: 43 } } } }")]
    public void ValidNullableObject(string types, string compare)
    {
        Valid(types, compare);
    }

    [Theory]
    [InlineData("input T { f: Int! }",                                  "T = { f: 42 }")]
    [InlineData("input T { f: Float! }",                                "T = { f: 3.14 }")]
    [InlineData("input T { f: Boolean! }",                              "T = { f: true }")]
    [InlineData("input T { f: String! }",                               "T = { f: \"foo\" }")]
    [InlineData("input T { f: ID! }",                                   "T = { f: \"foo\" }")]
    [InlineData("input T { f: Name! }",                                 "T = { f: \"foo\" }")]
    [InlineData("input T { f: Country! }",                              "T = { f: AUS }")]
    [InlineData("input T { f: Int! g:ID! h:Name! }",                    "T = { f: 42 g: \"foo\" h: \"foo\" }")]
    [InlineData("input T { f: Int! g: T }",                             "T = { f: 42 }")]
    [InlineData("input T { f: Int! g: T }",                             "T = { f: 42 g: null }")]
    [InlineData("input T { f: Int! g: T }",                             "T = { f: 42 g: { f: 43 } }")]
    [InlineData("input T { f: Int! g: T }",                             "T = { f: 42 g: { f: 43 g: { f: 44 } } }")]
    public void ValidNonNullableObject(string types, string compare)
    {
        Valid(types, compare);
    }

    [Theory]
    [InlineData("input T { f: Int }",                                   "T = { f: 3.14 }")]
    public void InValidNullableObject(string types, string compare)
    {
        Invalid(types, compare);
    }

    [Theory]
    [InlineData("input T { f: Int! }",                                  "T = { }")]
    [InlineData("input T { f: Int! }",                                  "T = { f: null }")]
    [InlineData("input T { f: Int! }",                                  "T = { f: 3.14 }")]
    public void InValidNonNullableObject(string types, string compare)
    {
        Invalid(types, compare);
    }

    [Theory]
    [InlineData("input T { f: Int }",                                   "[T] = null")]
    [InlineData("input T { f: Int }",                                   "[T] = []")]
    [InlineData("input T { f: Int }",                                   "[T] = [null]")]
    [InlineData("input T { f: Int }",                                   "[T] = [{}]")]
    [InlineData("input T { f: Int }",                                   "[T] = [{ f: 42 }]")]
    [InlineData("input T { f: Int }",                                   "[T] = [{ f: 42 }, null, {}]")]
    [InlineData("input T { f: Int } input U { g: T }",                  "[U] = null")]
    [InlineData("input T { f: Int } input U { g: T }",                  "[U] = []")]
    [InlineData("input T { f: Int } input U { g: T }",                  "[U] = [null]")]
    [InlineData("input T { f: Int } input U { g: T }",                  "[U] = [{}]")]
    [InlineData("input T { f: Int } input U { g: T }",                  "[U] = [{ g: null }]")]
    [InlineData("input T { f: Int } input U { g: T }",                  "[U] = [{ g: {} }]")]
    [InlineData("input T { f: Int } input U { g: T }",                  "[U] = [{ g: { f: 42 } }]")]
    [InlineData("input T { f: Int } input U { g: [T] }",                "[U] = null")]
    [InlineData("input T { f: Int } input U { g: [T] }",                "[U] = []")]
    [InlineData("input T { f: Int } input U { g: [T] }",                "[U] = [{}]")]
    [InlineData("input T { f: Int } input U { g: [T] }",                "[U] = [{ g: null }]")]
    [InlineData("input T { f: Int } input U { g: [T] }",                "[U] = [{ g: [] }]")]
    [InlineData("input T { f: Int } input U { g: [T] }",                "[U] = [{ g: [{}] }]")]
    [InlineData("input T { f: Int } input U { g: [T] }",                "[U] = [{ g: [{ f: 42 }] }]")]
    public void ValidNullableObjectList(string types, string compare)
    {
        Valid(types, compare);
    }

    [Theory]
    [InlineData("input T { f: Int }",                                   "[T]! = []")]
    [InlineData("input T { f: Int }",                                   "[T]! = [null]")]
    [InlineData("input T { f: Int }",                                   "[T]! = [{}]")]
    [InlineData("input T { f: Int }",                                   "[T]! = [{ f: 42 }]")]
    [InlineData("input T { f: Int }",                                   "[T]! = [{ f: 42 }, null, {}]")]
    [InlineData("input T { f: Int }",                                   "[T!] = null")]
    [InlineData("input T { f: Int }",                                   "[T!] = [{}]")]
    [InlineData("input T { f: Int }",                                   "[T!] = [{ f: 42 }]")]
    [InlineData("input T { f: Int }",                                   "[T!] = [{ f: 42 }, {}]")]
    [InlineData("input T { f: Int }",                                   "[T!]! = [{}]")]
    [InlineData("input T { f: Int }",                                   "[T!]! = [{ f: 42 }]")]
    [InlineData("input T { f: Int }",                                   "[T!]! = [{ f: 42 }, {}]")]    
    [InlineData("input T { f: Int } input U { g: T! }",                 "[U]! = []")]
    [InlineData("input T { f: Int } input U { g: T! }",                 "[U!]! = [{ g: {} }]")]
    [InlineData("input T { f: Int } input U { g: T! }",                 "[U!]! = [{ g: { f: 42 } }]")]
    [InlineData("input T { f: Int } input U { g: [T]! }",               "[U!]! = [{ g: [{ f: 42 }] }]")]    
    [InlineData("input T { f: Int! } input U { g: [T!]! }",             "[U!]! = [{ g: [{ f: 42 }] }]")]    
    public void ValidNonNullableObjectList(string types, string compare)
    {
        Valid(types, compare);
    }

    [Theory]
    [InlineData("input T { f: Int }",                                   "[T] = [ 42 ]")]
    [InlineData("input T { f: Int }",                                   "[T] = [ 3.14 ]")]
    [InlineData("input T { f: Int }",                                   "[T] = [ \"foo\"] ")]
    [InlineData("input T { f: Int }",                                   "[T] = [{ x: 42 }]")]
    [InlineData("input T { f: Int } input U { g: [T] }",                "[U] = [{ g: {} }]")]
    public void InvalidNullableObjectList(string types, string compare)
    {
        Invalid(types, compare);
    }

    [Theory]
    [InlineData("input T { f: Int }",                                   "[T]! = null")]
    [InlineData("input T { f: Int }",                                   "[T!] = [null]")]
    [InlineData("input T { f: Int }",                                   "[T!]! = null")]
    [InlineData("input T { f: Int }",                                   "[T!]! = [null]")]
    [InlineData("input T { f: Int }",                                   "[T!]! = [{ f: 42 }, null]")]
    [InlineData("input T { f: Int } input U { g: [T]! }",               "[U!]! = [{}]")]
    [InlineData("input T { f: Int } input U { g: [T]! }",               "[U!]! = [{ g: null }]")]
    [InlineData("input T { f: Int } input U { g: [T!]! }",              "[U!]! = [{ g: [null] }]")]
    [InlineData("input T { f: Int! } input U { g: [T!]! }",             "[U!]! = [{ g: [{}] }]")]
    public void InvalidNonNullableObjectList(string types, string compare)
    {
        Invalid(types, compare);
    }

    private static void Valid(string compare) 
    {
        SchemaValidationNoException("type Query { query: Int }  " +
                                    "scalar Name " +
                                    "enum Country { AUS NZ USA } " +
                                    "type foo { fizz(arg: " + compare + "): Int }");
    }
    private static void Valid(string types, string compare)
    {
        SchemaValidationNoException("type Query { query: Int }  " +
                                    "scalar Name " +
                                    "enum Country { AUS NZ USA } " +
                                    types + 
                                    " " +
                                    "type foo { fizz(arg: " + compare + "): Int }");
    }

    private static void Invalid(string compare)
    {
        SchemaValidationSingleException("type Query { query: Int } " +
                                        "scalar Name " +
                                        "enum Country { AUS NZ USA } " +
                                        "type foo { fizz(arg: " + compare + "): Int }", 
                                        "Argument 'arg' of field 'fizz' of object 'foo' has a default value incompatible with the type.");
    }

    private static void Invalid(string types, string compare)
    {
        SchemaValidationSingleException("type Query { query: Int } " +
                                        "scalar Name " +
                                        "enum Country { AUS NZ USA } " +
                                        types +
                                        "type foo { fizz(arg: " + compare + "): Int }",
                                        "Argument 'arg' of field 'fizz' of object 'foo' has a default value incompatible with the type.");
    }
}

