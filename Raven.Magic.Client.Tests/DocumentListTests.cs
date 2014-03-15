namespace Raven.Magic.Client.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using DocumentCollections;
    using MagicDocumentSession;
    using Moq;
    using Raven.Client;
    using Raven.Tests.Helpers;
    using RavenDocument;
    using Xunit;

    public class Sound
    {
        public string Code { get; set; }
    }

    public class AnimalSound
    {
        public string Code { get; set; }
        public Sound Sound { get; set; }
    }

    public class Animal
    {
        public string Name { get; set; }
        public IList<AnimalSound> Sounds { get; set; }
    }

    public class Pet
    {
        public string Name { get; set; }
        public Animal Animal { get; set; }
    }

    public class DocumentListTests : RavenTestBase
    {
        public IDocumentStore DocumentStore()
        {
            return NewDocumentStore();
            //_store = new DocumentStore { Url = "http://localhost:8080" }.WithMagicProxies();
            //_store.Initialize();
            //return store;
        }

        private static Mock<IDocumentSession> SessionMock()
        {
            var session = new Mock<IDocumentSession>();
            var operations = new Mock<ISyncAdvancedSessionOperation>();
            session.SetupGet(a => a.Advanced).Returns(operations.Object);
            return session;
        }

        [Fact]
        public void Session_List_Stores_Returns_Document_List()
        {
            // Arrange
            var session = new Mock<IDocumentSession>();

            // Act
            var animalsounds = session.Object.List<Animal>();

            // Assert
            Assert.IsAssignableFrom<DocumentList<Animal>>(animalsounds);
        }

        [Fact]
        public void Document_List_Stores_Items_In_Db_That_Are_Added()
        {
            // Arrange
            var session = SessionMock();
            
            // Act
            session.Object.List(new[] {new AnimalSound { Code = "testsound" }});

            // Assert
            session.Verify(a => a.Store(It.IsAny<AnimalSound>()), Times.Once);
        }

        [Fact]
        public void Document_List_Doesnt_Store_Items_That_Are_Proxies()
        {
            // Arrange
            var session = SessionMock();

            // Act
            session.Object.List(new[] { session.Object.LoadId(new Animal()) });

            // Assert
            session.Verify(a => a.Store(It.IsAny<AnimalSound>()), Times.Never);
        }

        [Fact]
        public void Raven_Queryable_Can_Be_Converted_To_Document_List()
        {
            // Arrange
            using (var session = DocumentStore().OpenSession())
            {
                // Act
                session.Store(new Animal());
                session.SaveChanges();
                var result = session.Query<Animal>().Customize(a => a.WaitForNonStaleResults()).ToList<Animal>(session);

                Assert.IsAssignableFrom<DocumentList<Animal>>(result);
            }
        }

        [Fact]
        public void Document_List_Can_Add_Items()
        {
            // Arrange
            using (IDocumentSession session = DocumentStore().OpenSession())
            {
                // Act
                var animalsounds = session.List<AnimalSound>();
                animalsounds.Add(new AnimalSound { Code = "testsound" });

                // Assert
                Assert.Equal(1, animalsounds.Count);
            }
        }

        [Fact]
        public void Can_Automatically_Hydrate_Included_Properties_During_Load()
        {
            const string code = "awesome";
            const string key = "animal";
            using (IDocumentStore store = DocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Animal { Sounds = session.List(new[] { new AnimalSound() { Code = code } }) }, key);
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var animal = session.LoadWithIncludes<Animal>(key, a => a.Sounds);
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

            using (IDocumentStore store = DocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(sound);
                    session.Store(new Animal { Sounds = new[] { new AnimalSound() { Sound = session.LoadId(sound) } } }, key);
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var animal = session.LoadWithIncludes<Animal>(key, a => a.Sounds.Select(b => b.Sound));
                    Assert.Equal(code, animal.Sounds.First().Sound.Code);
                }
            }
        }

        [Fact]
        public void Can_Automatically_Hydrate_Included_Properties_From_Value_Objects_During_Load()
        {
            const string code = "awesome";
            const string key = "pet";

            using (IDocumentStore store = DocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Pet() { Animal = new Animal { Sounds = session.List(new[] { new AnimalSound() {Code = code}})}}, key);
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var pet = session.LoadWithIncludes<Pet>(key, a => a.Animal.Sounds);
                    Assert.Equal(code, pet.Animal.Sounds.First().Code);
                }
            }
        }

        [Fact]
        public void Can_Automatically_Hydrate_Included_Property_During_Load()
        {
            using (IDocumentStore store = DocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    session.Store(new Pet { Animal = session.Property(new Animal() { Name = "awesome" }) }, "pet");
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    var pet = session.LoadWithIncludes<Pet>("pet", a => a.Animal);
                    Assert.Equal("awesome", pet.Animal.Name);
                }
            }
        }

        [Fact]
        public void Object_With_A_Document_List_Can_Be_Stored_And_Retrieved_From_Different_Sessions()
        {
            // Arrange
            using (var store = DocumentStore())
            {
                using (IDocumentSession session = OpenSession(store))
                {
                    var animalSounds = session.List(new[] {new AnimalSound {Code = "Thing"}, new AnimalSound {Code = "Thing2"}});
                    var animal = new Animal {Name = "Something", Sounds = animalSounds};

                    // Act
                    session.Store(animal);
                    session.SaveChanges();
                }

                using (IDocumentSession session = OpenSession(store))
                {
                    Animal animal = session.Query<Animal>().Include<Animal>(a => a.Sounds).Customize(a => a.WaitForNonStaleResults()).First();

                    // Assert
                    Assert.NotNull(session.List(animal.Sounds).First());
                }
            }
        }

        private static IDocumentSession OpenSession(IDocumentStore store)
        {
            return new MagicDocumentSession(store.OpenSession());
        }

        [Fact]
        public void Object_With_A_Document_List_Stores_Items_In_List_Into_Their_Own_Store()
        {
            // Arrange
            using (IDocumentSession session = DocumentStore().OpenSession())
            {
                var animal = new Animal {Name = "Something", Sounds = session.List(new []{new AnimalSound {Code = "Thing"}})};

                // Act
                session.Store(animal);
                session.SaveChanges();

                // Assert
                Assert.True(session.Query<AnimalSound>().Customize(a => a.WaitForNonStaleResults()).Any(), "The animalsounds should be stored in its own document collection.");
            }
        }
    }
}