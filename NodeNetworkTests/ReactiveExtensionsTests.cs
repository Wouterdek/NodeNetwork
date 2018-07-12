using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork.Utilities;
using ReactiveUI;

namespace NodeNetworkTests
{
    [TestClass]
    public class ReactiveExtensionsTests
    {
        [TestMethod]
        public void TestObserveEach()
        {
            //Setup
            var scheduler = new TestScheduler();
            var dummy1 = new DummyTestingClass { Identifier = "A" };
            var dummy2 = new DummyTestingClass { Identifier = "B" };
            ReactiveList<DummyTestingClass> list = new ReactiveList<DummyTestingClass>
            {
                dummy1
            };

            //Define actions
            scheduler.Schedule(TimeSpan.FromTicks(10), () => dummy1.TestObservable.OnNext("First test event"));
            scheduler.Schedule(TimeSpan.FromTicks(20), () => list.Add(dummy2));
            scheduler.Schedule(TimeSpan.FromTicks(30), () => dummy2.TestObservable.OnNext("Second test event"));
            scheduler.Schedule(TimeSpan.FromTicks(40), () => list.Remove(list[0]));
            scheduler.Schedule(TimeSpan.FromTicks(50), () => dummy1.TestObservable.OnNext("Third test event"));
            scheduler.Schedule(TimeSpan.FromTicks(60), () => list.Remove(list[0]));
            var actual = scheduler.Start(() => list.ObserveEach(d => d.TestObservable), created: 0, subscribed: 0, disposed: 100);

            //Assert
            var expected = new[]
            {
                ReactiveTest.OnNext(10, (dummy1, "First test event")),
                ReactiveTest.OnNext(30, (dummy2, "Second test event"))
            };
            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

        [TestMethod]
        public void TestObserveEachDelayedAdd()
        {
            //Setup
            var scheduler = new TestScheduler();
            var dummy1 = new DummyTestingClass { Identifier = "A" };
            var dummy2 = new DummyTestingClass { Identifier = "B" };
            ReactiveList<DummyTestingClass> list = new ReactiveList<DummyTestingClass>();
            var observable = list.ObserveEach(d => d.TestObservable);
            list.Add(dummy1);

            //Define actions
            scheduler.Schedule(TimeSpan.FromTicks(10), () => dummy1.TestObservable.OnNext("First test event"));
            var actual = scheduler.Start(() => observable, created: 0, subscribed: 0, disposed: 100);

            //Assert
            var expected = new[]
            {
                ReactiveTest.OnNext(10, (dummy1, "First test event"))
            };
            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

        [TestMethod]
        public void TestObserveEachDelayedRemove()
        {
            //Setup
            var scheduler = new TestScheduler();
            var dummy1 = new DummyTestingClass { Identifier = "A" };
            var dummy2 = new DummyTestingClass { Identifier = "B" };
            ReactiveList<DummyTestingClass> list = new ReactiveList<DummyTestingClass> { dummy1 };
            var observable = list.ObserveEach(d => d.TestObservable);
            list.Remove(dummy1);

            //Define actions
            scheduler.Schedule(TimeSpan.FromTicks(10), () => dummy1.TestObservable.OnNext("First test event"));
            var actual = scheduler.Start(() => observable, created: 0, subscribed: 0, disposed: 100);

            //Assert
            Assert.IsTrue(actual.Messages.Count == 0);
        }

        [TestMethod]
        public void TestObserveEachSuppressedClear()
        {
            //Setup
            var scheduler = new TestScheduler();
            var dummy1 = new DummyTestingClass { Identifier = "A" };
            var dummy2 = new DummyTestingClass { Identifier = "B" };
            ReactiveList<DummyTestingClass> list = new ReactiveList<DummyTestingClass>
            {
                dummy1
            };

            //Define actions
            scheduler.Schedule(TimeSpan.FromTicks(10), () =>
            {
                using (list.SuppressChangeNotifications())
                {
                    list.Clear();
                }
            });
            scheduler.Schedule(TimeSpan.FromTicks(20), () => dummy1.TestObservable.OnNext("First test event"));
            var actual = scheduler.Start(() => list.ObserveEach(d => d.TestObservable), created: 0, subscribed: 0, disposed: 100);

            //Assert
            Assert.IsTrue(actual.Messages.Count == 0);
        }

        class DummyTestingClass : ReactiveObject
        {
            public string Identifier { get; set; }
            public Subject<string> TestObservable = new Subject<string>();
        }
    }
}
