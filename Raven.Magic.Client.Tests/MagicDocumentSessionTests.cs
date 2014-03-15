namespace Raven.Magic.Client.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using DocumentCollections;
    using MagicDocumentSession;
    using Raven.Client;
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

        // Figure out a way to do includes in magic session since we can't overload MultiIncludeWithLoader.

        // Consider having iincludewithloader interface return interface instead of concrete types.

        // Create extension methods for IQueryable (e.g. First FirstOrDefault)

        [Fact(Skip = "Not working yet.")]
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

        [Fact(Skip = "Not working yet.")]
        public void When_Using_Include_In_A_Magic_Session_Load_The_Navigation_Property_Is_Loaded()
        {
            const string expected = "leg";
            using (IDocumentStore store = NewDocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Person {Limbs = session.List(new[] {new Limb {Name = expected}})});
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    Person person = session.Query<Person>().Include<Person>(a => a.Limbs).Customize(a => a.WaitForNonStaleResults()).First();
                    Assert.Equal(expected, person.Limbs.First().Name);
                }
            }
        }

        public class Idea
        {
            public Person Person { get; set; }
        }

        public class Limb
        {
            public string Name { get; set; }
        }

        public class Person
        {
            public string Name { get; set; }
            public IEnumerable<Limb> Limbs { get; set; }
        }
    }
}