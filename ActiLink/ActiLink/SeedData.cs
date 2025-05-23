﻿using System.Linq.Expressions;
using ActiLink.Events;
using ActiLink.Hobbies;
using ActiLink.Organizers;
using ActiLink.Organizers.BusinessClients;
using ActiLink.Organizers.Users;
using ActiLink.Shared.Model;
using ActiLink.Venues;
using Microsoft.EntityFrameworkCore;

namespace ActiLink
{
    internal static class SeedData
    {
        internal async static Task InitializeAndSaveAsync(DbContext context, CancellationToken cancellationToken)
        {
            Initialize(context);
            await context.SaveChangesAsync(cancellationToken);
        }

        internal static void InitializeAndSave(DbContext context)
        {
            Initialize(context);
            context.SaveChanges();
        }

        private static void Initialize(DbContext context)
        {
            Hobby[] hobbies =
                [
                    new("Photography"),
                    new("Cooking"),
                    new("Gardening"),
                    new("Painting"),
                    new("Reading"),
                    new("Traveling"),
                    new("Cycling"),
                    new("Hiking"),
                    new("Fishing"),
                    new("Knitting"),
                    new("Writing"),
                    new("Dancing"),
                    new("Singing"),
                    new("Playing an instrument"),
                    new("Yoga"),
                    new("Meditation"),
                    new("Martial arts"),
                    new("Video gaming"),
                    new("Board gaming"),
                    new("Collecting"),
                    new("Crafting"),
                    new("Woodworking"),
                    new("Pottery"),
                    new("Sculpting"),
                    new("Origami"),
                    new("Calligraphy"),
                    new("Journaling"),
                    new("Blogging"),
                    new("Podcasting"),
                    new("Vlogging"),
                    new("Cosplaying"),
                    new("Role-playing games"),
                    new("Model building"),
                    new("3D printing"),
                    new("Electronics"),
                    new("Programming"),
                    new("Web development"),
                    new("Graphic design"),
                    new("Animation"),
                    new("Video editing"),
                    new("Photography editing"),
                    new("Music production"),
                    new("Fashion design"),
                    new("Interior design"),
                    new("Makeup artistry"),
                    new("Hair styling"),
                    new("Nail art"),
                    new("Fitness training"),
                    new("Weightlifting"),
                    new("Running"),
                    new("Swimming"),
                    new("Surfing"),
                    new("Skateboarding"),
                    new("Snowboarding"),
                    new("Skiing"),
                    new("Rock climbing"),
                    new("Bouldering"),
                    new("Parkour"),
                    new("Football"),
                    new("Basketball"),
                    new("Baseball"),
                    new("Tennis"),
                    new("Golf"),
                    new("Volleyball"),
                    new("Hockey"),
                    new("Cricket"),
                    new("Rugby"),
                    new("Badminton"),
                    new("Table tennis"),
                    new("Fencing"),
                    new("Archery"),
                    new("Sport"),
                    new("Art"),
                    new("Drinking"),
                    new("Partying")
                ];

            User[] users =
                [
                new("Emily Smith", "emily.smith@email.com"),
                new("James Johnson", "james.johnson@email.com"),
                new("Diego Hernández", "diego.hernandez@email.com"),
                new("Julien Moreau", "julien.moreau@email.com"),
                new("Sophie Lefèvre", "sofie.lefevre@email.com"),
                new("Michał Zieliński", "michal.zielinski@email.com"),
                new("Paweł Król", "pawel.krol@email.com"),
                new("Grace Miller", "grace.miller@email.com"),
                new("Dua Lipa", "dua.lipa@email.com"),
                new("Wojciech Kuna", "wojciech.kuna@email.com")
                ];

            BusinessClient[] businessClients =
                [
                new("Claire Dubois", "claire.dubois@email.com", "FR123456789"),
                new("José Martínez", "jose.martinez@email.com", "ES987654321"),
                new("Piotr Nowak", "piotr.nowak@email.com", "PL123456789"),
                new("William Davis", "william.davis@email.com", "GB987654321")
                ];

            CreateEventSeedObject[] eventsToCreate =
                [
                new("Photography Workshop",
                "A workshop to improve your photography skills.",
                DateTime.Now.AddDays(30),
                DateTime.Now.AddDays(30),
                new Location(52.21477909582091, 21.035060679070405),
                50.0m, 5, 20,
                GetHobbiesByNames(hobbies, ["Photography"])),

                new("Football Match",
                "Join us for a friendly football match.",
                DateTime.Now.AddDays(7),
                DateTime.Now.AddDays(7),
                new Location(52.2218541041488, 20.982731729238),
                0.0m, 10, 20,
                GetHobbiesByNames(hobbies, ["Football", "Sport"])),

                new("Cooking Class",
                "Learn to cook delicious French cuisine.",
                DateTime.Now.AddDays(14),
                DateTime.Now.AddDays(14),
                new Location(52.212336118433356, 20.956230925623117),
                100.0m, 5, 15,
                GetHobbiesByNames(hobbies, ["Cooking"])),

                new("Dua Lipa Concert",
                "Join me for an unforgettable night of music and fun!",
                DateTime.Now.AddDays(60),
                DateTime.Now.AddDays(60),
                new Location(52.211669714114336, 21.0102402691948),
                150.0m, 50, 100,
                GetHobbiesByNames(hobbies, ["Singing", "Dancing"])),

                new("Yoga Retreat",
                "A relaxing weekend retreat focused on yoga and meditation.",
                DateTime.Now.AddDays(5),
                DateTime.Now.AddDays(5),
                new Location(52.250437919836386, 20.979243284658697),
                50.0m, 10, 30,
                GetHobbiesByNames(hobbies, ["Yoga", "Meditation"])),

                new("Art Exhibition",
                "Explore the latest trends in contemporary art.",
                DateTime.Now.AddDays(45),
                DateTime.Now.AddDays(45),
                new Location(52.2395182003831, 21.011823205599995),
                20.0m, 0, 50,
                GetHobbiesByNames(hobbies, ["Painting", "Sculpting", "Art"])),

                new("Common Knitting Circle",
                "Join us for a cozy knitting circle where we can share tips and projects.",
                DateTime.Now.AddDays(10),
                DateTime.Now.AddDays(10),
                new Location(52.2331398605709, 21.024730327045962),
                0.0m, 5, 15,
                GetHobbiesByNames(hobbies, ["Knitting"])),

                new("Skiing Trip",
                "A weekend trip to the mountains for skiing and snowboarding.",
                DateTime.Now.AddDays(90),
                DateTime.Now.AddDays(90),
                new Location(46.98652595915743, 10.02809174417663), // Example coordinates for the Alps
                1000.0m, 20, 50,
                GetHobbiesByNames(hobbies, ["Skiing", "Snowboarding", "Sport"]))
                ];

            Event[] events =
                [
                CreateEvent(eventsToCreate[0], businessClients[3]),
                CreateEvent(eventsToCreate[1], users[2]),
                CreateEvent(eventsToCreate[2], businessClients[0]),
                CreateEvent(eventsToCreate[3], users[8]),
                CreateEvent(eventsToCreate[4], users[4]),
                CreateEvent(eventsToCreate[5], businessClients[1]),
                CreateEvent(eventsToCreate[6], users[0]),
                CreateEvent(eventsToCreate[7], businessClients[2])
                ];

            AddHobbies(users[0], hobbies, ["Photography", "Cooking", "Gardening", "Painting", "Reading"]);                             // Emily Smith
            AddHobbies(users[1], hobbies, ["Traveling", "Cycling", "Hiking", "Fishing", "Knitting"]);                                  // James Johnson 
            AddHobbies(users[2], hobbies, ["Football", "Sport", "Crafting", "Woodworking", "Fishing"]);                                // Diego Hernández
            AddHobbies(users[3], hobbies, ["Meditation", "Martial arts", "Video gaming", "Board gaming", "Collecting"]);               // Julien Moreau 
            AddHobbies(users[4], hobbies, ["Crafting", "Woodworking", "Pottery", "Sculpting", "Origami"]);                             // Sophie Lefèvre
            AddHobbies(users[5], hobbies, ["Calligraphy", "Journaling", "Blogging", "Podcasting", "Vlogging"]);                        // Michał Zieliński
            AddHobbies(users[6], hobbies, ["Cosplaying", "Role-playing games", "Model building", "3D printing", "Electronics"]);       // Paweł Król
            AddHobbies(users[7], hobbies, ["Photography", "Singing", "Dancing", "Fashion design", "Interior design"]);                 // Grace Miller
            AddHobbies(users[8], hobbies, ["Singing", "Dancing", "Playing an instrument", "Yoga", "Meditation"]);                      // Dua Lipa
            AddHobbies(users[9], hobbies, ["Fitness training", "Weightlifting", "Partying", "Swimming", "Drinking"]);                  // Wojciech Kuna

            SignUpEvent(users[0], events[0]);
            SignUpEvent(users[1], events[6]);
            SignUpEvent(users[2], events[7]);
            SignUpEvent(users[3], events[4]);
            SignUpEvent(users[4], events[2]);
            SignUpEvent(users[5], events[1]);
            SignUpEvent(users[6], events[1]);
            SignUpEvent(users[7], events[5]);
            SignUpEvent(users[8], events[4]);
            SignUpEvent(users[9], events[4]);


            foreach (var user in users)
                SignUpEvent(user, events[3]);


            SeedSet(users, user => x => x.UserName == user.UserName);
            SeedSet(businessClients, client => x => x.UserName == client.UserName);
            SeedSet(hobbies, hobby => x => x.Name == hobby.Name);
            SeedSet(events, @event => x => x.Title == @event.Title);

            context.SaveChanges();

            SeedVenuesAndUpdateEvents(context);

            void SeedSet<T>(IEnumerable<T> entities, Func<T, Expression<Func<T, bool>>> matchPredicateFactory) where T : class
            {
                var set = context.Set<T>();

                foreach (var entity in entities)
                {
                    var matchPredicate = matchPredicateFactory(entity);
                    if (!set.Any(matchPredicate))
                        set.Add(entity);
                }
            }
        }

        private static void SeedVenuesAndUpdateEvents(DbContext context)
        {
            var venueDefinitions = new List<(string Name, string OwnerUsername, string Description, Location Location, string Address, string AssociatedEventTitle)>
            {
                (
                    "Art Gallery",
                    "Claire Dubois",
                    "A modern art gallery in the city center.",
                    new Location(52.2395182003831, 21.011823205599995),
                    "123 Art Street",
                    "Art Exhibition"
                ),
                (
                    "Yoga Studio",
                    "José Martínez",
                    "A peaceful yoga studio for relaxation.",
                    new Location(52.250437919836386, 20.979243284658697),
                    "456 Yoga Lane",
                    "Yoga Retreat"
                ),
                (
                    "Mountain Resort",
                    "Piotr Nowak",
                    "A luxurious resort in the mountains.",
                    new Location(46.98652595915743, 10.02809174417663),
                    "789 Mountain Road",
                    "Skiing Trip"
                )
            };

            var createdVenues = new Dictionary<string, Venue>();
            var venueSet = context.Set<Venue>();

            foreach (var venueDef in venueDefinitions)
            {
                var existingVenue = venueSet.AsNoTracking()
                    .FirstOrDefault(v => v.Name == venueDef.Name && v.Address == venueDef.Address);

                if (existingVenue == null)
                {
                    try
                    {
                        var owner = GetBusinessClientByUsername(context, venueDef.OwnerUsername);

                        var venue = new Venue(
                            owner,
                            venueDef.Name,
                            venueDef.Description,
                            venueDef.Location,
                            venueDef.Address
                        );

                        venueSet.Add(venue);
                        createdVenues[venueDef.Name] = venue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd podczas tworzenia venue {venueDef.Name}: {ex.Message}");
                    }
                }
                else
                {
                    createdVenues[venueDef.Name] = existingVenue;
                }
            }

            context.SaveChanges();

            foreach (var venueDef in venueDefinitions)
            {
                if (createdVenues.ContainsKey(venueDef.Name) && !string.IsNullOrEmpty(venueDef.AssociatedEventTitle))
                {
                    try
                    {
                        var venue = createdVenues[venueDef.Name];
                        var eventToUpdate = GetEventByTitle(context, venueDef.AssociatedEventTitle);

                        if (eventToUpdate != null)
                        {
                            context.Entry(eventToUpdate).State = EntityState.Modified;
                            context.Entry(eventToUpdate).Reference(e => e.Venue).CurrentValue = venue;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd podczas aktualizacji eventu {venueDef.AssociatedEventTitle}: {ex.Message}");
                    }
                }
            }

            context.SaveChanges();
        }

        private static BusinessClient GetBusinessClientByUsername(DbContext context, string username)
        {
            return context.Set<BusinessClient>().FirstOrDefault(bc => bc.UserName == username)
                ?? throw new InvalidOperationException($"BusinessClient with username {username} not found");
        }

        private static Event GetEventByTitle(DbContext context, string title)
        {
            return context.Set<Event>().FirstOrDefault(e => e.Title == title)
                ?? throw new InvalidOperationException($"Event with title {title} not found");
        }

        private static IEnumerable<Hobby> GetHobbiesByNames(IEnumerable<Hobby> hobbies, IEnumerable<string> names) => hobbies.Where(h => names.Contains(h.Name));
        private static Event CreateEvent(CreateEventSeedObject seedEvent, Organizer organizer)
        {
            var (title, description, startDate, endDate, location, price, minParticipants, maxParticipants, relatedHobbies) = seedEvent;
            var createdEvent = new Event(organizer, title, description, startDate, endDate, location, price, minParticipants, maxParticipants, relatedHobbies);
            organizer.Events.Add(createdEvent);
            return createdEvent;
        }

        private static void AddHobbies(User user, Hobby[] hobbies, IEnumerable<string> hobbyNames)
        {
            var hobbiesToAdd = GetHobbiesByNames(hobbies, hobbyNames);
            foreach (var hobby in hobbiesToAdd)
                user.Hobbies.Add(hobby);
        }

        private static void SignUpEvent(User user, Event eventToSignUp)
        {
            user.SignedUpEvents.Add(eventToSignUp);
            eventToSignUp.SignUpList.Add(user);
        }


        private record CreateEventSeedObject
            (string Title, string Description, DateTime StartDate, DateTime EndDate, Location Location, decimal Price, int MinParticipants, int MaxParticipants, IEnumerable<Hobby> RelatedHobbies);
    }
}
