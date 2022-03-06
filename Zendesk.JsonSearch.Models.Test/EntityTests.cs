using NUnit.Framework;
using System;
using System.Collections.Generic;
using Zendesk.JsonSearch.Models.EarlyBound;

namespace Zendesk.JsonSearch.Models.Test
{
    public class EntityTests
    {
        Entity user;
        Entity ticket;
        [SetUp]
        public void Setup()
        {
            user = new Entity("1", "user");
            user.name = "Francisca Rasmussen";
            user.SetAttribute("name", "Francisca Rasmussen");
            user.SetAttribute("created_at", "2016-04-15T05:19:46-10:00");
            user.SetAttribute("verified", true);

            ticket = new Entity("436bf9b0-1147-4c0a-8439-6f79833bff5b", "ticket");
            ticket.name = "A Catastrophe in Korea (North)";
            ticket.SetAttribute("created_at", "2016-04-28T11:19:34-10:00");
            ticket.SetAttribute("type", "incident");
            ticket.SetAttribute("subject", "A Catastrophe in Korea (North)");
            ticket.SetAttribute("assignee_id", "24");
            ticket.SetAttribute("assignee", new EntityReference { _id = "24", entityName = "user" });
            ticket.SetAttribute("tags", new List<string> { "Ohio", "Pennsylvania", "American Samoa", "Northern Mariana Islands" });
        }

        #region Happy pass - with correct values
        [Test]
        public void TestGetAttributeWithCorrectName()
        {
            Assert.IsTrue(user.GetAttribute<string>("name") == "Francisca Rasmussen");
            Assert.IsTrue(ticket.GetAttribute<List<string>>("tags").Count == 4);
            Assert.IsTrue(ticket.GetAttribute<List<string>>("tags")[0] == "Ohio");
        }

        [Test]
        public void TestSetAttributeWithExistingName()
        {
            user.SetAttribute("verified", false);
            Assert.IsTrue(user.GetAttribute<bool>("verified") == false);
        }

        [Test]
        public void TestSetAttributeWithNonExistingName()
        {
            user.SetAttribute("newname", false);
            Assert.IsTrue(user.GetAttribute<bool>("newname") == false);
        }

        [Test]
        public void TestToEntityReference()
        {
            var userReference = user.ToEntityReference();
            var ticketReference = ticket.ToEntityReference();
            Assert.IsTrue(userReference._id == "1" && userReference.entityName == "user" && userReference.name == "Francisca Rasmussen");
            Assert.IsTrue(ticketReference._id == "436bf9b0-1147-4c0a-8439-6f79833bff5b" && ticketReference.entityName == "ticket" && ticketReference.name == "A Catastrophe in Korea (North)");
        }

        [Test]
        public void TestToEntity()
        {
            var eUser = user.ToEntity<User>();
            var eTicket = ticket.ToEntity<Ticket>();
            Assert.IsTrue(eUser._id == "1" && eUser.entityName == "user" && eUser.name == "Francisca Rasmussen"
                && DateTime.Compare(eUser.created_at, System.DateTime.Parse("2016-04-15T05:19:46-10:00")) == 0 && eUser.verified == true);
            Assert.IsTrue(eTicket._id == "436bf9b0-1147-4c0a-8439-6f79833bff5b" && eTicket.entityName == "ticket" && eTicket.name == "A Catastrophe in Korea (North)"
              && DateTime.Compare(eTicket.created_at.Value, DateTime.Parse("2016-04-28T11:19:34-10:00")) == 0 && eTicket.assignee._id == "24"
              && eTicket.assignee.entityName == "user" && eTicket.tags.Count == 4 && eTicket.tags[0] == "Ohio");
        }

        #endregion

        #region Unhappy pass - with wrong values

        [Test]
        public void TestGetAttributeWithWrongName()
        {
            Assert.IsTrue(user.GetAttribute<string>("wrongname") == null);
        }

        [Test]
        public void TestToEntityWithNoEntityName()
        {
            var entity = new Entity();
            entity._id = "1";
            entity.name = "name";
            Assert.Catch<NotSupportedException>(() => entity.ToEntity<User>());
        }

        #endregion
    }
}