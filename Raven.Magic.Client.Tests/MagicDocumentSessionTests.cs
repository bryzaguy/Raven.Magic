﻿namespace Raven.Magic.Client.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using DocumentCollections;
    using MagicDocumentSession;
    using Raven.Client;
    using Raven.Client.Indexes;
    using Raven.Tests.Helpers;
    using RavenDocument;
    using Xunit;

    public class MagicDocumentSessionTests : RavenTestBase
    {
        [Fact]
        public void Can_Create_Magic_Session_From_Document_Store()
        {
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    Assert.IsAssignableFrom<MagicDocumentSession>(session);
                }
            }
        }

        [Fact]
        public void Loading_Invalid_Key_Returns_Null_Object()
        {
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    Assert.Null(session.Load<Limb>("crapkeystuffs"));
                }
            }
        }

        [Fact]
        public void Object_With_Key_Can_Save_Into_Database()
        {
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Limb().Id("crapkeystuffs"));
                    session.SaveChanges();
                }
            }
        }
        

        [Fact]
        public void Loaded_Object_Property_Change_Is_Saved()
        {
            const string id = "crapkeystuffs";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Limb(){Name = "test1"}.Id(id));
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var result = session.Load<Limb>(id);
                    result.Name = "test2";
                    session.SaveChanges();
                    Assert.Equal(2, session.Advanced.NumberOfRequests);
                }
            }
        }


        [Fact]
        public void Queried_Object_Property_Change_Is_Saved()
        {
            const string id = "crapkeystuffs";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Limb(){Name = "test1"}, id);
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var result = session.Query<Limb>().Customize(a => a.WaitForNonStaleResults()).First();
                    result.Name = "test2";
                    session.SaveChanges();
                    Assert.Equal(2, session.Advanced.NumberOfRequests);
                }
            }
        }

        [Fact]
        public void Can_Load_Entity_With_Id_Attached()
        {
            const string id = "testID";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Limb(), id);
                    Assert.Equal(id, session.Load<Limb>(id).Id());
                }
            }
        }

        [Fact]
        public void Can_Load_Entity_With_Includes_And_Id_Attached()
        {
            const string id = "testID";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Person(), id);
                    session.SaveChanges();
                    Assert.Equal(id, session.Include<Person>(a => a.Limbs).Load<Person>(id).Id());
                }
            }
        }

        [Fact]
        public void Can_Automatically_Hydrate_Included_Properties_During_Load()
        {
            const string code = "awesome";
            const string key = "animal";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Animal { Sounds = session.List(new[] { new AnimalSound() { Code = code } }) }, key);
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var animal = session.Include<Animal>(a => a.Sounds).Load(key);
                    Assert.Equal(code, animal.Sounds.First().Code);
                }
            }
        }

        [Fact]
        public void Can_Automatically_Hydrate_Included_Properties_From_Embedded_Collections_During_Load()
        {
            const string code = "awesome";
            const string key = "animal";
            var sound = new Sound { Code = code };

            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(sound);
                    session.Store(new Animal { Sounds = new[] { new AnimalSound() { Sound = session.LoadId(sound) } } }, key);
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var animal = session.Include<Animal>(a => a.Sounds.Select(b => b.Sound)).Load<Animal>(key);
                    Assert.Equal(code, animal.Sounds.First().Sound.Code);
                }
            }
        }

        [Fact]
        public void Can_Automatically_Hydrate_Included_Properties_From_Value_Objects_During_Load()
        {
            const string code = "awesome";
            const string key = "pet";

            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Pet() { Animal = new Animal { Sounds = session.List(new[] { new AnimalSound() { Code = code } }) } }, key);
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var pet = session.Include<Pet>(a => a.Animal.Sounds).Load<Pet>(key);
                    Assert.Equal(code, pet.Animal.Sounds.First().Code);
                }
            }
        }

        [Fact]
        public void Can_Automatically_Hydrate_Included_Property_During_Load()
        {
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Pet { Animal = session.Property(new Animal() { Name = "awesome" }) }, "pet");
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var pet = session.Include<Pet>(a => a.Animal).Load<Pet>("pet");
                    Assert.Equal("awesome", pet.Animal.Name);
                }
            }
        }


        [Fact]
        public void Can_Load_Entity_With_Property_That_Is_In_A_Separate_Document()
        {
            const string expected = "guyfox";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Idea {Person = session.Property(new Person {Name = expected})});
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    Idea idea = session.Query<Idea>().Include<Idea>(a => a.Person).Customize(a => a.WaitForNonStaleResults()).First();
                    Assert.Equal(expected, idea.Person.Name);
                }
            }
        }

        private static MagicDocumentSession OpenSession(IDocumentStore store)
        {
            return new MagicDocumentSession(store.OpenSession());
        }

        [Fact]
        public void When_Using_Include_In_A_Magic_Session_Load_The_Navigation_Property_Is_Loaded()
        {
            const string expected = "leg";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Person { Limbs = session.List(new[] { new Limb { Name = expected } }) });
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    Person person = session.Query<Person>().Include<Person>(a => a.Limbs).Customize(a => a.WaitForNonStaleResults()).First();
                    Assert.Equal(expected, person.Limbs.First().Name);
                }
            }
        }

        [Fact]
        public void Query_With_Where_Should_Return_Proxy_Objects()
        {
            const string id = "thisiddude", name = "STUFSS";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Person { Name = name }, id);
                    session.SaveChanges();

                    var result = session.Query<Person>().Customize(a => a.WaitForNonStaleResults()).Where(a => a.Name == name).Take(1).Skip(0).ToList();
                    Assert.NotNull(result.First().Id());
                }
            }
        }
        
        [Fact]
        public void Query_First_Should_Return_Proxy_Objects()
        {
            const string id = "thisiddude", name = "STUFSS";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Person { Name = name }, id);
                    session.SaveChanges();
                    Assert.NotNull(session.Query<Person>().Customize(a => a.WaitForNonStaleResults()).First(a => a.Name == name).Id());
                }
            }
        }
        
        [Fact]
        public void Query_Include_Should_Ignore_Embedded_Objects()
        {
            const string name = "STUFSS";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    var show = new Limb {BelongsTo = new Person {Name = name}};
                    session.Store(show);
                    session.SaveChanges();

                    var result = session.Query<Limb>()
                        .Include(a => a.BelongsTo)
                        .Customize(a => a.WaitForNonStaleResults())
                        .ToList();

                    Assert.NotNull(result.Select(a => a.BelongsTo).SingleOrDefault(a => a.Id() == null && a.Name == name));
                }
            }
        }
        
        [Fact]
        public void When_Query_Include_Is_Null_Leaves_Proxy_Object()
        {
            const string id = "thisiddude", testId = "thissweetidyo";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    var limb = new Limb { BelongsTo = new Person().Id(testId)};
                    session.Store(limb, id);
                    session.SaveChanges();
                    Assert.NotNull(session.Query<Limb>().Include<Limb>(a => a.BelongsTo).Customize(a =>a.WaitForNonStaleResults()).First().BelongsTo);
                }
            }
        }

        [Fact]
        public void When_Load_Include_Is_Null_Leaves_Proxy_Object()
        {
            const string id = "thisiddude", testId = "thissweetidyo";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    var limb = new Limb { BelongsTo = new Person().Id(testId)};
                    session.Store(limb, id);
                    session.SaveChanges();
                    Assert.NotNull(session.Include<Limb>(a => a.BelongsTo).Load<Limb>(id).BelongsTo);
                }
            }
        }

        [Fact]
        public void Query_Can_Include_Deep_Object_Graph_References()
        {
            const string id = "thisiddude", name = "STUFSS";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    var show = new Shows {Crowds = new[] {new Crowd {People = session.List(new[] {new Person {Name = name}}).ToArray()}}};
                    session.Store(show, id);
                    session.SaveChanges();
                    Assert.NotNull(session.Query<Shows>().Include(a => a.Crowds.Select(b => b.People)).Customize(a => a.WaitForNonStaleResults()).FirstOrDefault());
                }
            }
        }

        public class TestIndex : AbstractIndexCreationTask<Shows, TestIndex.Result> 
        {
            public class Result
            {
                public bool IsReduced { get; set; }
            }

            public TestIndex()
            {
                Map = shows => from show in shows
                               select new { IsReduced = true };
            }
        }

        [Fact]
        public void Able_To_Project_From_Index() 
        {
            using (var store = NewDocumentStore())
            {
                store.ExecuteIndex(new TestIndex());

                using (var session = OpenSession(store))
                {
                    session.Store(new Shows());
                    session.SaveChanges();
                    Assert.NotEmpty(session.Query<TestIndex.Result, TestIndex>()
                        .Customize(a => a.WaitForNonStaleResults())
                        .ProjectFromIndexFieldsInto<TestIndex.Result>()
                        .ToList());
                }
            }
        }
        
        [Fact]
        public void Query_To_List_Includes_References()
        {
            const string name = "thistest";
            using (var store = NewDocumentStore())
            {
                using (var session = OpenSession(store))
                {
                    session.Store(new Person {Limbs = session.List(new[] {new Limb {Name = name}})});
                    session.SaveChanges();

                    Assert.Equal(name, session.Query<Person>()
                        .Include(a => a.Limbs)
                        .Customize(a => a.WaitForNonStaleResults())
                        .First().Limbs.First().Name);
                }
            }
        }


        [Fact]
        public void Query_Can_Include_Multiple_Deep_Object_Graph_References()
        {
            const string id = "thisiddude", name = "STUFSS";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    var show = new Shows 
                    {
                        Crowds = new[] 
                        {
                            new Crowd {People = session.List(new[] {new Person {Name = name}, new Person {Name = name}}).ToArray()},
                            new Crowd {People = session.List(new[] {new Person {Name = name}, new Person {Name = name}}).ToArray()}
                        }
                    };

                    session.Store(show, id);
                    session.SaveChanges();
                    Assert.NotNull(session.Query<Shows>().Include(a => a.Crowds.Select(b => b.People)).Customize(a => a.WaitForNonStaleResults()).FirstOrDefault());
                }
            }
        }

        [Fact]
        public void Query_Should_Return_Proxy_Object()
        {
            const string id = "thisiddude", name = "STUFSS";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Person { Name = name }, id);
                    session.SaveChanges();
                    Assert.NotNull(session.Query<Person>().Customize(a => a.WaitForNonStaleResults()).ToList().First().Id());
                }
            }
        }
        
        public class Idea
        {
            public Person Person { get; set; }
        }
        
        public class Crowd
        {
            public Person[] People { get; set; }
        }

        public class Shows
        {
            public IEnumerable<Crowd> Crowds { get; set; }
        }

        public class Limb
        {
            public Person BelongsTo { get; set; }
            public string Name { get; set; }
        }

        public class Person
        {
            public string Name { get; set; }
            public IEnumerable<Limb> Limbs { get; set; }
        }
    }
}