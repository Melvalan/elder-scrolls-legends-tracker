﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ESLTracker.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESLTrackerTests;
using System.Reflection;
using ESLTracker.Utils;
using Moq;
using System.Collections;

namespace ESLTracker.DataModel.Tests
{
    [TestClass()]
    public class GameTests : BaseTest
    {
        [TestMethod()]
        public void GameTest001_CheckIfDatePopulated()
        {
            Game g = new Game();

            Assert.IsTrue(g.Date.Year > 1999);
        }

        [TestMethod()]
        public void EqualsTest001_x_Equals_x_True()
        {
            Game g = new Game();

            PopulateObject(g, StartProp);

#pragma warning disable RECS0088 // Comparing equal expression for equality is usually useless
            Assert.IsTrue(g.Equals(g));
#pragma warning restore RECS0088 // Comparing equal expression for equality is usually useless
        }

        [TestMethod()]
        public void EqualsTest002_x_Equals_y_same_as_y_eqals_x_diff_values()
        {
            Game x = new Game();
            Game y = new Game();

            PopulateObject(x, StartProp);
            PopulateObject(y, EditProp);

            Assert.AreEqual(x.Equals(y), y.Equals(x));
        }

        [TestMethod()]
        public void EqualsTest003_x_Equals_y_same_as_y_eqals_x_same_values()
        {
            Game x = new Game();
            Game y = new Game();

            PopulateObject(x, StartProp);
            PopulateObject(y, StartProp);

            Assert.AreEqual(x.Equals(y), y.Equals(x));
        }

        [TestMethod()]
        public void EqualsTest004_x_Equals_null_False()
        {
            Game x = new Game();

            PopulateObject(x, StartProp);

            Assert.AreEqual(false, x.Equals(null));
        }


        [TestMethod()]
        public void EqualsTest005_valuesTest()
        {
            //pass same date time to constructors!
            Mock<ITrackerFactory> factory = new Mock<ITrackerFactory>();
            factory.Setup(f => f.GetDateTimeNow()).Returns(DateTime.Now);

            Game x = new Game(factory.Object);
            Game y = new Game(factory.Object);

            Assert.IsTrue(x.Equals(y), "Equals failed for bare objects");

            foreach (PropertyInfo p in typeof(Game).GetProperties())
            {
                if (p.CanWrite)
                {


                    p.SetValue(x, StartProp[p.PropertyType]);
                    p.SetValue(y, EditProp[p.PropertyType]);

                    Assert.IsFalse(x.Equals(y), "Property {0} failed. x={1};y={2};", p.Name, p.GetValue(x), p.GetValue(y));
                }
            }
        }

        [TestMethod]
        public void CloneTest()
        {
            Game game = new Game();
            PopulateObject(game, StartProp);

            game.PropertyChanged += (s, e) => { };

            Game clone = game.Clone() as Game;

            foreach (PropertyInfo p in typeof(Game).GetProperties())
            {
                if (p.CanWrite)
                {
                    TestContext.WriteLine("Checking prop:{0}.{1};{2}", p.Name, p.GetValue(game), p.GetValue(clone));
                    if (p.PropertyType == typeof(string))
                    {
                        //http://stackoverflow.com/questions/506648/how-do-strings-work-when-shallow-copying-something-in-c
                        continue;
                    }
                    if (p.Name == nameof(Game.Deck))  //dont want to clone deck
                    {
                        continue;
                    }
                    if (p.PropertyType.GetInterface(nameof(ICollection)) != null)
                    {
                        CollectionAssert.AreNotEqual(p.GetValue(game) as ICollection, p.GetValue(clone) as ICollection, new ReferenceComparer());
                    }
                    else
                    {
                        Assert.IsFalse(Object.ReferenceEquals(p.GetValue(game), p.GetValue(clone)));
                    }
                }
            }

            foreach (EventInfo ev in typeof(CardInstance).GetEvents())
            {
                FieldInfo fieldTheEvent = GetAllFields(typeof(CardInstance)).Where(f => f.Name == ev.Name).First();
                Assert.IsNull(fieldTheEvent.GetValue(clone));
            }
        }
    }
}