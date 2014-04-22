namespace Raven.Magic.Client.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MagicDocumentSession;
    using Raven.Client;
    using Raven.Client.Embedded;
    using Raven.Tests.Helpers;
    using RavenDocument;
    using Xunit;

    public class Musician
    {
        public string Name { get; set; }
        public Band Band { get; set; }
        public IEnumerable<Instrument> Instruments { get; set; }
    }

    public class Band
    {
        public string Name { get; set; }
    }

    public class Instrument
    {
        public string Name { get; set; }
    }

    public class RavenDocumentTests : RavenTestBase
    {
        public IDocumentStore DocumentStore()
        {
            EmbeddableDocumentStore store = NewDocumentStore();
            //var store = new DocumentStore { Url = "http://localhost:8080" };
            //store.Initialize();

            return store;
        }

        public interface IThing
        {
            string Name { get; set; }
        }

        public class Thing : IThing
        {
            public string Name { get; set; }
        }

        [Fact]
        public void Can_Create_Proxy_From_Interface_And_Entity()
        {
            Assert.NotNull((new Thing() as IThing).Id("test"));
        }

        [Fact]
        public void Can_Create_Proxy_Object_From_Entity_Where_Id_IsKnown()
        {
            var musician = new Musician {Name = "TestGuys"};

            Musician result = musician.Id("CoolStuff");

            Assert.Equal("CoolStuff", result.Id());
            Assert.Equal(musician.Name, result.Name);
        }

        [Fact]
        public void Can_Get_Id_Of_Proxy_Entity()
        {
            const string expected = "sweetid";
            var entity = new Musician().Id(expected);

            var result = entity.Id();

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Entity_That_Isnt_A_Proxy_Returns_Null_For_Id()
        {
            Assert.Null(new Musician().Id());
        }

        public class TestWithId
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void Can_Store_Document_That_Has_An_Id()
        {
            using (IDocumentStore store = DocumentStore())
            {
                var testWithId = new TestWithId { Name = "TestGuys" };
                using (IDocumentSession session = store.OpenSession())
                {
                    session.Store(testWithId);
                    session.SaveChanges();
                }

                using (IDocumentSession session = store.OpenSession())
                {
                    Assert.Equal(testWithId.Name, session.Load<TestWithId>(testWithId.Id).Name);
                }
            }
        }

        [Fact]
        public void TestWithId_Can_Find_Natural_Id()
        {
            var test = new TestWithId {Id = "crazy"};
            Assert.Equal(test.Id, test.Id());
        }

        [Fact]
        public void Can_Create_Proxy_Object_From_Stored_Entity()
        {
            using (IDocumentStore store = DocumentStore())
            {
                var musician = new Musician {Name = "TestGuys"};
                using (IDocumentSession session = store.OpenSession())
                {
                    session.Store(musician);
                    Musician result = session.LoadId(musician);
                    Assert.Equal(musician.Name, result.Name);
                }
            }
        }

        [Fact]
        public void Can_Load_Entity_Using_Proxy_Object()
        {
            const string id = "SuchAGreatIdAndStuff";
            using (IDocumentStore store = DocumentStore())
            {
                var musician = new Musician {Name = "TestGuys"};
                using (IDocumentSession session = store.OpenSession())
                {
                    session.Store(musician, id);
                    session.SaveChanges();
                }

                using (IDocumentSession session = store.OpenSession())
                {
                    Assert.Equal(musician.Name, session.Load<Musician>(id).Name);
                }
            }
        }

        [Fact]
        public void Can_Map_Between_Different_Objects()
        {
            var musician = new Musician {Name = "TestGuys"};

            Musician result = musician.MapTo(new Musician()) as Musician;

            Assert.Equal(musician.Name, result.Name);
        }

        [Fact]
        public void Can_Query_A_Document_After_Storing()
        {
            using (IDocumentStore store = DocumentStore())
            {
                var musician = new Musician {Name = "TestGuys"};
                using (IDocumentSession session = store.OpenSession())
                {
                    session.Store(musician);
                    session.SaveChanges();
                }

                using (IDocumentSession session = store.OpenSession())
                {
                    Assert.True(session.Query<Musician>().Customize(a => a.WaitForNonStaleResults()).Any());
                }
            }
        }

        [Fact]
        public void Object_Graph_Converts_To_And_From_Json()
        {
            using (IDocumentStore store = DocumentStore())
            {
                const string id = "AmazingKey";
                using (IDocumentSession session = OpenSession(store))
                {
                    var band = new Band {Name = "Wooah"};
                    var instrument = new Instrument {Name = "Drums"};

                    session.Store(band);
                    session.Store(instrument);
                    session.Store(new Musician { Name = "Stuff", Band = session.LoadId(band), Instruments = new[] { session.LoadId(instrument) } }, id);

                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var result = session.Load<Musician>(id);
                    Assert.Equal("Wooah", session.Load<Band>(result.Band.Id()).Name);
                    Assert.True(result.Instruments.Any(a => session.Load<Instrument>(a.Id()).Name == "Drums"));
                }
            }
        }

        private static IDocumentSession OpenSession(IDocumentStore store)
        {
            return MagicDocumentSession.SetupDocumentStore(store.OpenSession());
        }
    }
}