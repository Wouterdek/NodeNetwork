using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork.Utilities;
using ReactiveUI;
using ReactiveUI.Legacy;

namespace NodeNetworkTests
{
    [TestClass]
    public class ReactiveExtensionsTests
    {
        [TestMethod]
        public void TestThrottleWhen()
        {
            //Setup
            var scheduler = new TestScheduler();
            Subject<int> numberValues = new Subject<int>();
            Subject<bool> flagValues = new Subject<bool>();

            //Define actions
            scheduler.Schedule(TimeSpan.FromTicks(1), () => flagValues.OnNext(false));
            scheduler.Schedule(TimeSpan.FromTicks(10), () => numberValues.OnNext(0));
            scheduler.Schedule(TimeSpan.FromTicks(20), () => numberValues.OnNext(1));
            scheduler.Schedule(TimeSpan.FromTicks(30), () => flagValues.OnNext(true));
            scheduler.Schedule(TimeSpan.FromTicks(40), () => numberValues.OnNext(2));
            scheduler.Schedule(TimeSpan.FromTicks(50), () => numberValues.OnNext(3));
            scheduler.Schedule(TimeSpan.FromTicks(60), () => numberValues.OnNext(4));
            scheduler.Schedule(TimeSpan.FromTicks(70), () => flagValues.OnNext(false));
            scheduler.Schedule(TimeSpan.FromTicks(71), () => flagValues.OnNext(true)); 
            scheduler.Schedule(TimeSpan.FromTicks(72), () => flagValues.OnNext(false));
            scheduler.Schedule(TimeSpan.FromTicks(80), () => numberValues.OnNext(5));
            scheduler.Schedule(TimeSpan.FromTicks(90), () => numberValues.OnNext(6));
            var actual = scheduler.Start(() => numberValues.ThrottleWhen(flagValues), 0, 0, 100);

            //Assert
            var expected = new[]
            {
                ReactiveTest.OnNext(10, 0),
                ReactiveTest.OnNext(20, 1),
                ReactiveTest.OnNext(70, 4),
                ReactiveTest.OnNext(80, 5),
                ReactiveTest.OnNext(90, 6)

            };
            ReactiveAssert.AreElementsEqual(expected, actual.Messages);
        }

		class DummyTestingClass : ReactiveObject
        {
            public string Identifier { get; set; }
            public Subject<string> TestObservable = new Subject<string>();
        }
    }
}
