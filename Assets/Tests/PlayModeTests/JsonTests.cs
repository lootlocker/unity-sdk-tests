﻿using LootLocker;
using LootLocker.Requests;
#if !LOOTLOCKER_USE_NEWTONSOFTJSON
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LLlibs.ZeroDepJson;
#endif
using NUnit.Framework;

namespace Tests
{
    public class MultiDimensionalArrayClass
    {
        public string[][] multiDimensionalArray { get; set; }
    }

    public class JsonTests
    {

        [Test]
        public void SDKCanDeserializeJson()
        {
            // Given
            const string validGuestSessionResponse =
                "{\n  \"success\": true,\n  \"session_token\": \"e6fa44946f077dd9fe67311ab3f188c596df9969\",\n  \"player_id\": 3,\n  \"public_uid\": \"TSEYDXD8\",\n  \"player_identifier\": \"uuid-11223344\",\n  \"player_created_at\": \"2022-05-30T07:56:01+00:00\",\n  \"check_grant_notifications\": true,\n  \"check_deactivation_notifications\": false,\n  \"seen_before\": true\n}";

            // When
            LootLockerSessionRequest deserializedSessionRequest =
                LootLockerJson.DeserializeObject<LootLockerSessionRequest>(validGuestSessionResponse);

            // Then
            Assert.NotNull(deserializedSessionRequest, "Not deserialized, is null");
            Assert.NotNull(deserializedSessionRequest.player_identifier,
                "Not deserialized, does not contain player_identifier property");
            Assert.AreEqual(deserializedSessionRequest.player_identifier, "uuid-11223344",
                "Not deserialized, does not contain player_identifier value");
        }

        [Test]
        public void SDKCanDeserializeComplexArrayObjectsJson()
        {
            // Given
            const string complexJson =
                "{\n\"success\": true,\n\"loadouts\": [\n{\n\"character\": {\n\"id\": 3015691,\n\"type\": \"Wizard\",\n\"name\": \"Bb32\",\n\"is_default\": true\n},\n\"loadout\": []\n}\n]\n}";

            // When
            LootLockerCharacterLoadoutResponse deserializedCharacterLoadoutResponse =
                LootLockerJson.DeserializeObject<LootLockerCharacterLoadoutResponse>(complexJson);

            // Then
            Assert.NotNull(deserializedCharacterLoadoutResponse, "Not deserialized, is null");
            Assert.NotNull(deserializedCharacterLoadoutResponse.GetCharacters(),
                "Not deserialized, does not contain characters");
            Assert.IsNotEmpty(deserializedCharacterLoadoutResponse.GetCharacters(),
                "Not deserialized, does not contain characters");
            Assert.AreEqual(deserializedCharacterLoadoutResponse.GetCharacter("Bb32").type, "Wizard",
                "Not deserialized, does not contain the correct character");
        }

        [Test]
        public void SDKCanDeserializeMultidimensionalArrayObjectsJson()
        {
            // Given
            const string multiDimJson =
                "{\n\"multiDimensionalArray\": [[\"1-1\", \"1-2\", \"1-3\", \"1-4\"], [\"2-1\", \"2-2\", \"2-3\"], [\"3-1\", \"3-2\"]]\n}\n]\n}";

            // When
            MultiDimensionalArrayClass deserializedMultiDimensionalArray =
                LootLockerJson.DeserializeObject<MultiDimensionalArrayClass>(multiDimJson);

            // Then
            Assert.NotNull(deserializedMultiDimensionalArray, "Not deserialized, is null");
            Assert.NotNull(deserializedMultiDimensionalArray.multiDimensionalArray,
                "Not deserialized, does not contain multi dimensional array");
            Assert.IsNotEmpty(deserializedMultiDimensionalArray.multiDimensionalArray,
                "Not deserialized, does not contain multi dimensional array");
            Assert.AreEqual("2-2", deserializedMultiDimensionalArray.multiDimensionalArray[1][1],
                "Not deserialized, does not contain the correct value");
        }

        [Test]
        public void SDKCanSerializeMultidimensionalArrayObjectsJson()
        {
            // Given
            MultiDimensionalArrayClass mdArray = new MultiDimensionalArrayClass();
            mdArray.multiDimensionalArray = new[]
                { new[] { "1-1", "1-2", "1-3", "1-4" }, new[] { "2-1", "2-2", "2-3" }, new[] { "3-1", "3-2" } };

            // When
            string serializedJson = LootLockerJson.SerializeObject(mdArray);

            // Then
            Assert.NotNull(serializedJson, "Not serialized, is null");
            Assert.AreNotEqual("{}", serializedJson, "Not serialized, empty");
            Assert.IsTrue(serializedJson.Contains("multiDimensionalArray"),
                "Not Serialized, does not contain multiDimensionalArray property");
            Assert.IsTrue(serializedJson.Contains("3-1"), "Not Serialized, does not contain 3-1 value");
        }

        [Test]
        public void SDKCanSerializeJson()
        {
            // Given
            LootLockerSessionRequest SessionRequest = new LootLockerSessionRequest("uuid-11223344");

            // When
            string serializedJson = LootLockerJson.SerializeObject(SessionRequest);

            // Then
            Assert.NotNull(serializedJson, "Not serialized, is null");
            Assert.AreNotEqual("{}", serializedJson, "Not serialized, empty");
            Assert.IsTrue(serializedJson.Contains("player_identifier"),
                "Not Serialized, does not contain player_identifier property");
            Assert.IsTrue(serializedJson.Contains("uuid-11223344"),
                "Not Serialized, does not contain player_identifier value");
        }

#if !LOOTLOCKER_USE_NEWTONSOFTJSON
        [Test]
        public void JsonTestsSimplePasses()
        { 
            Assert.AreEqual("true", Json.Serialize(true));
            Assert.AreEqual("false", Json.Serialize(false));
            Assert.AreEqual("12345678", Json.Serialize(12345678));
            Assert.AreEqual("12345678901234567890", Json.Serialize(12345678901234567890));
            Assert.AreEqual("1234567890123456789.0123456789", Json.Serialize(1234567890123456789.01234567890m));
            Assert.AreEqual("12345678", Json.Serialize((uint)12345678));
            Assert.AreEqual("128", Json.Serialize((byte)128));
            Assert.AreEqual("-56", Json.Serialize((sbyte)-56));
            Assert.AreEqual("-56", Json.Serialize((short)-56));
            Assert.AreEqual("12345", Json.Serialize((ushort)12345));
            Assert.AreEqual("\"héllo world\"", Json.Serialize("héllo world"));
            var ts = new TimeSpan(12, 34, 56, 7, 8);
            Assert.AreEqual("11625670080000", Json.Serialize(ts));
            Assert.AreEqual("\"13:10:56:07.008\"", Json.Serialize(ts, new JsonOptions { SerializationOptions = JsonSerializationOptions.TimeSpanAsText }));
            var guid = Guid.NewGuid();
            Assert.AreEqual("\"" + guid + "\"", Json.Serialize(guid));
            Assert.AreEqual("\"https://github.com/smourier/ZeroDepJson\"", Json.Serialize(new Uri("https://github.com/smourier/ZeroDepJson")));
            Assert.AreEqual("2", Json.Serialize(UriKind.Relative));
            Assert.AreEqual("\"Relative\"", Json.Serialize(UriKind.Relative, new JsonOptions { SerializationOptions = JsonSerializationOptions.EnumAsText }));
            Assert.AreEqual("\"x\"", Json.Serialize('x'));
            Assert.AreEqual("1234.56775", Json.Serialize(1234.56775f));
            Assert.AreEqual("1234.5678", Json.Serialize(1234.5678d));
        }

        [Test]
        public void TestList()
        {
            var list = new List<Customer>();
            for (var i = 0; i < 10; i++)
            {
                var customer = new Customer();
                customer.Index = i;
                list.Add(customer);
            }

            var json = Json.Serialize(list);
            var list2 = Json.Deserialize<List<Customer>>(json);
            var json2 = Json.Serialize(list2);
            Assert.AreEqual(json, json2);
        }

        [Test]
        public void TestDictionary()
        {
            var dic = new Dictionary<Guid, Customer>();
            for (var i = 0; i < 10; i++)
            {
                var customer = new Customer();
                customer.Index = i;
                customer.Name = "This is a name 这是一个名字" + Environment.TickCount;
                var address1 = new Address();
                address1.ZipCode = 75000;
                address1.City = new City();
                address1.City.Name = "Paris";
                address1.City.Country = new Country();
                address1.City.Country.Name = "France";

                var address2 = new Address();
                address2.ZipCode = 10001;
                address2.City = new City();
                address2.City.Name = "New York";
                address2.City.Country = new Country();
                address2.City.Country.Name = "USA";

                customer.Addresses = new[] { address1, address2 };

                dic[customer.Id] = customer;
            }

            var json1 = Json.Serialize(dic);
            var list2 = (Dictionary<string, object>)Json.Deserialize(json1);
            var json2 = Json.Serialize(list2);
            Assert.AreEqual(json1, json2);

            var customers = list2.Values.Cast<Dictionary<string, object>>().ToList();
            var json3 = Json.Serialize(customers);
            var list3 = Json.Deserialize<List<Customer>>(json3);
            var json4 = Json.Serialize(list3);
            Assert.AreEqual(json3, json4);
        }

        [Test]
        public void TestCyclic()
        {
            var person = new Person { Name = "foo" };
            var persons = new Person[] { person, person };
            try
            {
                var json = Json.Serialize(persons);
                Assert.Fail();
            }
            catch (JsonException ex)
            {
                Assert.IsTrue(ex.Code == 9);
            }
        }

        [Test]
        public void TestCyclicCustom()
        {
            var person = new Person { Name = "héllo" };
            var persons = new Person[] { person, person };
            var options = new CustomOptions();
            var json = Json.Serialize(persons, options);
            Assert.IsTrue(json == "[{\"Name\":\"héllo\"},{\"Name\":\"héllo\"}]");
        }
    }

    class CustomOptions : JsonOptions
    {
        public CustomOptions()
        {
            ObjectGraph = new CustomObjectGraph();
        }

        private class CustomObjectGraph : IDictionary<object, object>, Json.IOptionsHolder
        {
            private readonly Dictionary<object, int> _hash = new Dictionary<object, int>();

            public JsonOptions Options { get; set; }

            public void Add(object key, object value)
            {
                _hash[key] = Options.SerializationLevel;
            }

            public bool ContainsKey(object key)
            {
                if (!_hash.TryGetValue(key, out var level))
                    return false;

                if (Options.SerializationLevel == level)
                    return false;

                return true;
            }

            public object this[object key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public ICollection<object> Keys => throw new NotImplementedException();
            public ICollection<object> Values => throw new NotImplementedException();
            public int Count => throw new NotImplementedException();
            public bool IsReadOnly => throw new NotImplementedException();
            public void Add(KeyValuePair<object, object> item) => throw new NotImplementedException();
            public void Clear() => throw new NotImplementedException();
            public bool Contains(KeyValuePair<object, object> item) => throw new NotImplementedException();
            public void CopyTo(KeyValuePair<object, object>[] array, int arrayIndex) => throw new NotImplementedException();
            public IEnumerator<KeyValuePair<object, object>> GetEnumerator() => throw new NotImplementedException();
            public bool Remove(object key) => throw new NotImplementedException();
            public bool Remove(KeyValuePair<object, object> item) => throw new NotImplementedException();
            public bool TryGetValue(object key, out object value) => throw new NotImplementedException();
            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        }
    }

    class Person
    {
        public string Name { get; set; }
    }

    public class Customer
    {
        public Customer()
        {
            Id = Guid.NewGuid();

        }

        public Guid Id { get; }
        public int Index { get; set; }
        public string Name { get; set; }

        public Address[] Addresses { get; set; }

        public override string ToString() => Name;
    }

    public class Address
    {
        public City City { get; set; }
        public int ZipCode { get; set; }

        public override string ToString() => ZipCode.ToString();
    }

    public class City
    {
        public string Name { get; set; }
        public Country Country { get; set; }

        public override string ToString() => Name;
    }

    public class Country
    {
        public string Name { get; set; }

        public override string ToString() => Name;
    }
#else
    }
#endif
}
