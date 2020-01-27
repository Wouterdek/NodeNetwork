using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DynamicData;
using ExampleCalculatorApp.ViewModels;
using ExampleCalculatorApp.ViewModels.Nodes;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodeNetwork.Toolkit.ValueNode;
using ReactiveUI;
using ReactiveUI.Testing;

namespace CalculatorTests
{
    [TestClass]
    public class NetworkValidationTests
    {
        [TestMethod]
        public void TestOutputNodeOnly()
        {
            MainViewModel main = new MainViewModel();
            OutputNodeViewModel outputNode = main.NetworkViewModel.Nodes.Items.OfType<OutputNodeViewModel>().First();

            Assert.AreEqual(0, outputNode.ResultInput.Value);
            Assert.IsTrue(main.NetworkViewModel.LatestValidation.IsValid);
        }

        [TestMethod]
        public void TestConstantToOutput()
        {
            ImmediateScheduler.Instance.With(_ =>
            {
                MainViewModel main = new MainViewModel();
                OutputNodeViewModel outputNode = main.NetworkViewModel.Nodes.Items.OfType<OutputNodeViewModel>().First();

                ConstantNodeViewModel constantNode = new ConstantNodeViewModel();
                constantNode.ValueEditor.Value = 5;
                main.NetworkViewModel.Nodes.Add(constantNode);
                main.NetworkViewModel.Connections.Add(main.NetworkViewModel.ConnectionFactory(outputNode.ResultInput, constantNode.Output));

                Assert.AreEqual(5, outputNode.ResultInput.Value);
                Assert.IsTrue(main.NetworkViewModel.LatestValidation.IsValid);
            });
        }

        [TestMethod]
        public void TestDivideToOutput()
        {
            ImmediateScheduler.Instance.With(_ =>
            {
                MainViewModel main = new MainViewModel();
                OutputNodeViewModel outputNode = main.NetworkViewModel.Nodes.Items.OfType<OutputNodeViewModel>().First();

                DivisionNodeViewModel divisionNode = new DivisionNodeViewModel();
                main.NetworkViewModel.Nodes.Add(divisionNode);
                main.NetworkViewModel.Connections.Add(main.NetworkViewModel.ConnectionFactory(outputNode.ResultInput, divisionNode.Output));

                Assert.AreEqual(null, outputNode.ResultInput.Value);
                Assert.IsFalse(main.NetworkViewModel.LatestValidation.IsValid);
            });
        }

        [TestMethod]
        public void TestConstantToDivideToOutput()
        {
            MainViewModel main = new MainViewModel();
            OutputNodeViewModel outputNode = main.NetworkViewModel.Nodes.Items.OfType<OutputNodeViewModel>().First();

            DivisionNodeViewModel divisionNode = new DivisionNodeViewModel();
            main.NetworkViewModel.Nodes.Add(divisionNode);
            main.NetworkViewModel.Connections.Add(main.NetworkViewModel.ConnectionFactory(outputNode.ResultInput, divisionNode.Output));

            ConstantNodeViewModel constantNode = new ConstantNodeViewModel();
            main.NetworkViewModel.Nodes.Add(constantNode);
            main.NetworkViewModel.Connections.Add(main.NetworkViewModel.ConnectionFactory(divisionNode.Input2, constantNode.Output));
            
            Assert.AreEqual(null, outputNode.ResultInput.Value);
            Assert.IsFalse(main.NetworkViewModel.LatestValidation.IsValid);
        }

        [TestMethod]
        public void TestConstantToDivideToOutputObservable()
        {
            MainViewModel main = new MainViewModel();
            OutputNodeViewModel outputNode = main.NetworkViewModel.Nodes.Items.OfType<OutputNodeViewModel>().First();

            outputNode.ResultInput.ValueChanged.Zip(Observable.Range(0, 100), (val, i) => (value: val, index: i)).Subscribe(t =>
            {
                var validation = main.NetworkViewModel.LatestValidation;

                switch (t.index)
                {
                    case 0:
                        Assert.AreEqual(0, t.value);
                        Assert.IsTrue(validation.IsValid);
                        Assert.IsTrue(validation.NetworkIsTraversable);
                        return;
                    case 1:
                        Assert.AreEqual(null, t.value);
                        Assert.IsFalse(validation.IsValid);
                        Assert.IsTrue(validation.NetworkIsTraversable);
                        return;
                    case 2:
                        Assert.AreEqual(0, t.value);
                        Assert.IsTrue(validation.IsValid);
                        Assert.IsTrue(validation.NetworkIsTraversable);
                        return;
                }
            });

            DivisionNodeViewModel divisionNode = new DivisionNodeViewModel();
            main.NetworkViewModel.Nodes.Add(divisionNode);
            main.NetworkViewModel.Connections.Add(main.NetworkViewModel.ConnectionFactory(outputNode.ResultInput, divisionNode.Output));

            ConstantNodeViewModel constantNode = new ConstantNodeViewModel();
            constantNode.ValueEditor.Value = 1;
            main.NetworkViewModel.Nodes.Add(constantNode);
            main.NetworkViewModel.Connections.Add(main.NetworkViewModel.ConnectionFactory(divisionNode.Input2, constantNode.Output));
        }

        [TestMethod]
        public void TestInvalidToValidChange()
        {
            MainViewModel main = new MainViewModel();
            OutputNodeViewModel outputNode = main.NetworkViewModel.Nodes.Items.OfType<OutputNodeViewModel>().First();

            outputNode.ResultInput.ValueChanged.Zip(Observable.Range(0, 100), (val, i) => (value: val, index: i)).Subscribe(t =>
            {
                var validation = main.NetworkViewModel.LatestValidation;

                switch (t.index)
                {
                    case 0:
                        Assert.AreEqual(0, t.value);
                        Assert.IsTrue(validation.IsValid);
                        Assert.IsTrue(validation.NetworkIsTraversable);
                        return;
                    case 1:
                        Assert.AreEqual(null, t.value);
                        Assert.IsFalse(validation.IsValid);
                        Assert.IsTrue(validation.NetworkIsTraversable);
                        return;
                    case 2:
                        Assert.AreEqual(0, t.value);
                        Assert.IsTrue(validation.IsValid);
                        Assert.IsTrue(validation.NetworkIsTraversable);
                        return;
                }
            });

            DivisionNodeViewModel divisionNode = new DivisionNodeViewModel();
            main.NetworkViewModel.Nodes.Add(divisionNode);
            var connection = main.NetworkViewModel.ConnectionFactory(outputNode.ResultInput, divisionNode.Output);
            main.NetworkViewModel.Connections.Add(connection);
            main.NetworkViewModel.Connections.Remove(connection);
        }

        [TestMethod]
        public void TestInvalidToValidChange2()
        {
            MainViewModel main = new MainViewModel();
            OutputNodeViewModel outputNode = main.NetworkViewModel.Nodes.Items.OfType<OutputNodeViewModel>().First();

            outputNode.ResultInput.ValueChanged.Zip(Observable.Range(0, 100), (val, i) => (value: val, index: i)).Subscribe(t =>
            {
                var validation = main.NetworkViewModel.LatestValidation;

                switch (t.index)
                {
                    case 0:
                        Assert.AreEqual(0, t.value);
                        Assert.IsTrue(validation.IsValid);
                        Assert.IsTrue(validation.NetworkIsTraversable);
                        return;
                    case 1:
                        Assert.AreEqual(null, t.value);
                        Assert.IsFalse(validation.IsValid);
                        Assert.IsTrue(validation.NetworkIsTraversable);
                        return;
                    case 2:
                        Assert.AreEqual(1, t.value);
                        Assert.IsTrue(validation.IsValid);
                        Assert.IsTrue(validation.NetworkIsTraversable);
                        return;
                    default: throw new Exception("too many updates");
                }
            });

            DivisionNodeViewModel divisionNode = new DivisionNodeViewModel();
            ((IntegerValueEditorViewModel)divisionNode.Input1.Editor).Value = 1;
            main.NetworkViewModel.Nodes.Add(divisionNode);

            ConstantNodeViewModel constantNode = new ConstantNodeViewModel();
            constantNode.ValueEditor.Value = 1;
            main.NetworkViewModel.Nodes.Add(constantNode);

            var connection1 = main.NetworkViewModel.ConnectionFactory(outputNode.ResultInput, divisionNode.Output);
            main.NetworkViewModel.Connections.Add(connection1);

            var connection2 = main.NetworkViewModel.ConnectionFactory(divisionNode.Input2, constantNode.Output);
            main.NetworkViewModel.Connections.Add(connection2);
        }

        [TestMethod, Timeout(5000)]
        public void TestProductRecursively()
        {
            ImmediateScheduler.Instance.With(_ =>
            {
                MainViewModel main = new MainViewModel();
                OutputNodeViewModel outputNode =
                    main.NetworkViewModel.Nodes.Items.OfType<OutputNodeViewModel>().First();

                ProductNodeViewModel productNodeA = new ProductNodeViewModel();
                main.NetworkViewModel.Nodes.Add(productNodeA);
                main.NetworkViewModel.Connections.Add(
                    main.NetworkViewModel.ConnectionFactory(outputNode.ResultInput, productNodeA.Output));

                ProductNodeViewModel productNodeB = new ProductNodeViewModel();
                main.NetworkViewModel.Nodes.Add(productNodeB);
                main.NetworkViewModel.Connections.Add(
                    main.NetworkViewModel.ConnectionFactory(productNodeA.Input1, productNodeB.Output));

                main.NetworkViewModel.Connections.Add(
                    main.NetworkViewModel.ConnectionFactory(productNodeB.Input1, productNodeA.Output));

                Assert.IsFalse(main.NetworkViewModel.LatestValidation.IsValid);
                Assert.AreEqual(null, outputNode.ResultInput.Value);
            });
        }

        [TestMethod/*, Timeout(5000)*/]
        public void TestLongChain()
        {
            new TestScheduler().With(_ =>
            {
                MainViewModel main = new MainViewModel();
                OutputNodeViewModel outputNode =
                    main.NetworkViewModel.Nodes.Items.OfType<OutputNodeViewModel>().First();

                ConstantNodeViewModel constantNode = new ConstantNodeViewModel();
                main.NetworkViewModel.Nodes.Add(constantNode);

                ProductNodeViewModel productNode = new ProductNodeViewModel();
                main.NetworkViewModel.Nodes.Add(productNode);

                DivisionNodeViewModel divisionNode = new DivisionNodeViewModel();
                main.NetworkViewModel.Nodes.Add(divisionNode);

                SubtractionNodeViewModel subtractionNode = new SubtractionNodeViewModel();
                main.NetworkViewModel.Nodes.Add(subtractionNode);

                SumNodeViewModel sumNode = new SumNodeViewModel();
                main.NetworkViewModel.Nodes.Add(sumNode);

                main.NetworkViewModel.Connections.Add(
                    main.NetworkViewModel.ConnectionFactory(subtractionNode.Input1, constantNode.Output));
                main.NetworkViewModel.Connections.Add(
                    main.NetworkViewModel.ConnectionFactory(sumNode.Input1, constantNode.Output));
                main.NetworkViewModel.Connections.Add(
                    main.NetworkViewModel.ConnectionFactory(divisionNode.Input1, subtractionNode.Output));
                main.NetworkViewModel.Connections.Add(
                    main.NetworkViewModel.ConnectionFactory(productNode.Input1, divisionNode.Output));
                main.NetworkViewModel.Connections.Add(
                    main.NetworkViewModel.ConnectionFactory(productNode.Input2, sumNode.Output));
                main.NetworkViewModel.Connections.Add(
                    main.NetworkViewModel.ConnectionFactory(outputNode.ResultInput, productNode.Output));

                constantNode.ValueEditor.Value = 10;
                ((IntegerValueEditorViewModel) subtractionNode.Input2.Editor).Value = 2;
                ((IntegerValueEditorViewModel) sumNode.Input2.Editor).Value = 4;
                ((IntegerValueEditorViewModel) divisionNode.Input2.Editor).Value = 4;

                //TODO: this is hacky, but test fails without it because the propagation updates are
                //scheduled after the assertion. Ideally, this should be resolved by using TestScheduler
                //but it isn't. However, it seems this is only a problem in tests.
                _.AdvanceByMs(100);

                Assert.AreEqual(28, outputNode.ResultInput.Value);
                Assert.IsTrue(main.NetworkViewModel.LatestValidation.IsValid);
            });
        }
    }
}
