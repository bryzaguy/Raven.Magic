namespace Raven.Magic.Client.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using DocumentCollections;
    using Imports.Newtonsoft.Json;
    using MagicDocumentSession;
    using Raven.Client;
    using Raven.Client.Embedded;
    using Raven.Tests.Helpers;
    using RavenDocument;
    using Xunit;

    public class MagicDocumentSessionTests : RavenTestBase
    {
        public class Person
        {
            public string Name { get; set; }
            public IEnumerable<Limb> Limbs { get; set; }
        }

        public class Limb
        {
            public string Name { get; set; }
        }

        public class Idea
        {
            public Person Person { get; set; }
        }

        [Fact]
        public void Can_Add_More_Json_Conversion_Logic_To_Store_With_Magic_Sessions()
        {
            // Arrange
            JsonSerializer expected = null;
            using (EmbeddableDocumentStore store = NewDocumentStore())
            {
                // Act
                store.WithMagic(a => expected = a);
                using (IDocumentSession session = store.OpenSession())
                {
                    session.Store(new Limb()); // Just doing this crap so that the
                    session.SaveChanges(); // json customize code is called

                    Assert.NotNull(expected);
                }
            }
        }

        [Fact]
        public void Can_Create_Magic_Session_From_Document_Store()
        {
            using (IDocumentStore store = NewDocumentStore().WithMagic())
            {
                using (IDocumentSession session = store.OpenMagicSession())
                {
                    Assert.IsAssignableFrom<MagicDocumentSession>(session);
                }
            }
        }

        [Fact]
        public void Can_Load_Entity_With_Id_Attached()
        {
            const string id = "testID";
            using (IDocumentStore store = NewDocumentStore().WithMagic())
            {
                using (IDocumentSession session = store.OpenMagicSession())
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
            using (IDocumentStore store = NewDocumentStore().WithMagic())
            {
                using (IDocumentSession session = store.OpenMagicSession())
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
            using (IDocumentStore store = NewDocumentStore().WithMagic())
            {
                using (IDocumentSession session = store.OpenMagicSession())
                {
                    session.Store(new Idea {Person = session.Property(new Person {Name = expected})});
                    session.SaveChanges();
                }

                using (IDocumentSession session = store.OpenMagicSession())
                {
                    Idea idea = session.Query<Idea>().Include<Idea>(a => a.Person).Customize(a => a.WaitForNonStaleResults()).First();
                    Assert.Equal(expected, idea.Person.Name);
                }
            }
        }

        [Fact(Skip = "Not working yet.")]
        public void When_Using_Include_In_A_Magic_Session_Load_The_Navigation_Property_Is_Loaded()
        {
            const string expected = "leg";
            using (IDocumentStore store = NewDocumentStore().WithMagic())
            {
                using (IDocumentSession session = store.OpenMagicSession())
                {
                    session.Store(new Person {Limbs = session.List(new[] {new Limb {Name = expected}})});
                    session.SaveChanges();
                }

                using (IDocumentSession session = store.OpenMagicSession())
                {
                    Person person = session.Query<Person>().Include<Person>(a => a.Limbs).Customize(a => a.WaitForNonStaleResults()).First();
                    Assert.Equal(expected, person.Limbs.First().Name);
                }
            }
        }
    }
}