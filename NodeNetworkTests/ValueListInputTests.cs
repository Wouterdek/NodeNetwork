using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork.Toolkit.ValueNode;
using NodeNetwork.ViewModels;
using ReactiveUI;

namespace NodeNetworkTests
{
    [TestClass]
    public class ValueListInputTests
    {
        [TestMethod]
        public void TestValuePropagation()
        {
            //Setup
            var scheduler = new TestScheduler();

            var nodeA = new NodeViewModel();
            Subject<int> sourceA = new Subject<int>();
            var outputA = new ValueNodeOutputViewModel<int>
            {
                Value = sourceA
            };
            nodeA.Outputs.Add(outputA);

            var nodeB = new NodeViewModel();
            var inputB = new ValueListNodeInputViewModel<int>();
            nodeB.Inputs.Add(inputB);

            NetworkViewModel network = new NetworkViewModel();
            network.Nodes.AddRange(new []{nodeA, nodeB});

            network.Connections.Add(network.ConnectionFactory(inputB, outputA));

            //Define actions
            scheduler.Schedule(TimeSpan.FromTicks(10), () => sourceA.OnNext(1));
            scheduler.Schedule(TimeSpan.FromTicks(20), () => sourceA.OnNext(0));
            scheduler.Schedule(TimeSpan.FromTicks(30), () => sourceA.OnNext(1));
            scheduler.Schedule(TimeSpan.FromTicks(40), () => sourceA.OnNext(0));
            scheduler.Schedule(TimeSpan.FromTicks(50), () => sourceA.OnNext(2));
            var actual = scheduler.Start(() => inputB.Values.Connect().QueryWhenChanged(), created: 0, subscribed: 0, disposed: 100);

            //Assert
            Assert.AreEqual(actual.Messages.Count, 6);
            Assert.AreEqual(actual.Messages[0].Time, 1);
            Assert.IsTrue(actual.Messages[0].Value.Value.SequenceEqual(new[] { 0 }));
            Assert.AreEqual(actual.Messages[1].Time, 10);
            Assert.IsTrue(actual.Messages[1].Value.Value.SequenceEqual(new[] { 1 }));
            Assert.AreEqual(actual.Messages[2].Time, 20);
            Assert.IsTrue(actual.Messages[2].Value.Value.SequenceEqual(new[] { 0 }));
            Assert.AreEqual(actual.Messages[3].Time, 30);
            Assert.IsTrue(actual.Messages[3].Value.Value.SequenceEqual(new[] { 1 }));
            Assert.AreEqual(actual.Messages[4].Time, 40);
            Assert.IsTrue(actual.Messages[4].Value.Value.SequenceEqual(new[] { 0 }));
            Assert.AreEqual(actual.Messages[5].Time, 50);
            Assert.IsTrue(actual.Messages[5].Value.Value.SequenceEqual(new[] { 2 }));
        }

        public class Example : ReactiveObject
        {
            private int _value;
            public int Value
            {
                get => _value;
                set => this.RaiseAndSetIfChanged(ref _value, value);
            }
        }

        [TestMethod]
        public void TestDD()
        {
            SourceList<Example> list = new SourceList<Example>();
            var valueList = list.Connect()
                .AutoRefresh(e => e.Value)
                .Transform(e => e.Value, true)
                .AsObservableList();

            var obj = new Example {Value = 0};
            list.Add(obj);
            obj.Value = 1;
            Assert.AreEqual(valueList.Items.First(), 1);
        }
    }
}
