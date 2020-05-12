using DynamicData;
using NodeNetwork.Toolkit.Layout.ForceDirected;
using NodeNetwork.Toolkit.SuspensionDrivers;
using NodeNetwork.ViewModels;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.Serialization;

namespace NodeNetwork.Toolkit.ValueNode
{
    [DataContract]
    public class NetworkViewModelBuilder<TOutput> : ReactiveObject where TOutput : class, IAmTheOutputViewModel
    {
        [IgnoreDataMember] private readonly ForceDirectedLayouter autoLayouter;
        [IgnoreDataMember] private readonly ReplaySubject<Values<TOutput>> initialise = new ReplaySubject<Values<TOutput>>(1);
        [IgnoreDataMember] private CompositeDisposable CleanUp = new CompositeDisposable();
        [IgnoreDataMember] private Configuration config;

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        /// <value>The output.</value>
        [DataMember] private TOutput Output = default;

        /// <summary>
        /// Initializes a new instance of the NetworkViewModelBuilder class.
        /// </summary>
        public NetworkViewModelBuilder(ForceDirectedLayouter autoLayouter = null, Configuration config = null, int maxIterations = 10000)
        {
            if (InDesignMode) return;
            this.autoLayouter = autoLayouter ?? new ForceDirectedLayouter();
            this.config = config ?? new Configuration { Network = NetworkViewModel };
            NetworkViewModel = new NetworkViewModel() { Id = Guid.NewGuid().ToString(), Name = SuspensionDriver.CurrentKey };
            _ = NetworkViewModel.NetworkChanged.Select(x => false).Subscribe(x => IsSaved = x);


            AutoLayout = ReactiveCommand.Create(() => this.autoLayouter.Layout(this.config, maxIterations));

            // setup continuous Automatic layout
            StartAutoLayoutLive = ReactiveCommand.CreateFromObservable(() => Observable.StartAsync(ct => this.autoLayouter.LayoutAsync(this.config, ct)).TakeUntil(StopAutoLayoutLive));
            StopAutoLayoutLive = ReactiveCommand.Create(() => { }, StartAutoLayoutLive.IsExecuting);
        }


        /// <summary>
        /// Gets a value indicating whether this instance is in design mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is in design mode; otherwise, <c>false</c>.
        /// </value>
        [IgnoreDataMember] protected bool InDesignMode => DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());

        /// <summary>
        /// Gets or sets a value indicating whether [automatic clean up].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic clean up]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AutoCleanUp
        {
            get => _AutoCleanUp;
            set
            {
                this.RaiseAndSetIfChanged(ref _AutoCleanUp, value);
            }
        }
        [IgnoreDataMember] private bool _AutoCleanUp = true;


        /// <summary>
        /// Gets the automatic layout.
        /// </summary>
        /// <value>The automatic layout.</value>
        [IgnoreDataMember] public ReactiveCommand<Unit, Unit> AutoLayout { get; }

        /// <summary>
        /// Gets a value indicating whether this instance is saved.
        /// </summary>
        /// <value><c>true</c> if this instance is saved; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsSaved
        {
            get => _IsSaved;
            internal set
            {
                this.RaiseAndSetIfChanged(ref _IsSaved, value);
            }
        }
        [IgnoreDataMember] private bool _IsSaved = true;


        /// <summary>
        /// Gets the network view model.
        /// </summary>
        /// <value>The network view model.</value>
        [DataMember]
        public NetworkViewModel NetworkViewModel
        {
            get => _NetworkViewModel;
            internal set
            {
                this.RaiseAndSetIfChanged(ref _NetworkViewModel, value);
            }
        }
        [IgnoreDataMember] private NetworkViewModel _NetworkViewModel;

        /// <summary>
        /// Gets the on initialise.
        /// </summary>
        /// <value>The on initialise.</value>
        [IgnoreDataMember] public IObservable<Values<TOutput>> OnInitialise => initialise;

        /// <summary>
        /// Gets the start automatic layout live.
        /// </summary>
        /// <value>The start automatic layout live.</value>
        [IgnoreDataMember] public ReactiveCommand<Unit, Unit> StartAutoLayoutLive { get; }

        /// <summary>
        /// Gets the stop automatic layout live.
        /// </summary>
        /// <value>The stop automatic layout live.</value>
        [IgnoreDataMember] public ReactiveCommand<Unit, Unit> StopAutoLayoutLive { get; }

        /// <summary>
        /// Gets the suspension driver.
        /// </summary>
        /// <value>The suspension driver.</value>
        [IgnoreDataMember] public DictionaryToFileSuspensionDriver SuspensionDriver { get; } = new DictionaryToFileSuspensionDriver();

        /// <summary>
        /// Clears the specified default item.
        /// </summary>
        /// <typeparam name="N"></typeparam>
        /// <param name="defaultItem">The default item.</param>
        public void Clear<N>(N defaultItem) where N : NodeViewModel, TOutput
        {
            NetworkViewModel = new NetworkViewModel() { Id = Guid.NewGuid().ToString(), Name = SuspensionDriver.CurrentKey };
            Output = defaultItem;
            NetworkViewModel.Nodes.Add(defaultItem);
            ReInitialise();
        }

        /// <summary>
        /// Adds the output and ReInitialises.
        /// </summary>
        /// <typeparam name="N"></typeparam>
        /// <param name="defaultItem">The default item.</param>
        public void AddOutput<N>(N defaultItem) where N : NodeViewModel, TOutput
        {
            Output = defaultItem;
            NetworkViewModel.Nodes.Add(defaultItem);
            ReInitialise();
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            NetworkViewModel = new NetworkViewModel() { Id = Guid.NewGuid().ToString(), Name = SuspensionDriver.CurrentKey };
        }

        /// <summary>
        /// Loads the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Load(string key)
        {
            SuspensionDriver.CurrentKey = key ?? "default";
            SuspensionDriver.LoadState().Subscribe(Rebuild);
        }

        /// <summary>
        /// Loads the default.
        /// </summary>
        public void LoadDefault() => SuspensionDriver.LoadState().Subscribe(Rebuild);

        /// <summary>
        /// Rebuilds the specified view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public void Rebuild(object viewModel)
        {
            if (viewModel is NetworkViewModel vm)
            {
                NetworkViewModel.Name = vm.Name;
                NetworkViewModel.Left = vm.Left;
                NetworkViewModel.Top = vm.Top;
                NetworkViewModel.Nodes.Clear();
                NetworkViewModel.Connections.Clear();
                if (vm.CurrentNodes != null)
                {
                    var origId = vm.Id ?? vm.CurrentNodes.First().Id;
                    if (!string.IsNullOrWhiteSpace(origId))
                    {
                        // Set the original Id
                        NetworkViewModel.Id = origId;
                    }

                    foreach (var node in vm.CurrentNodes)
                    {
                        // set the Parents
                        node.Rebuild(NetworkViewModel);
                    }

                    // Add all the nodes
                    NetworkViewModel.Nodes.Edit(l => l.AddRange(vm.CurrentNodes));
                }
                if (vm.CurrentConnections != null && NetworkViewModel.CurrentInputs.Count() > 0 && NetworkViewModel.CurrentOutputs.Count() > 0)
                {
                    foreach (var con in vm.CurrentConnections)
                    {
                        // Set the parent
                        con.Rebuild(NetworkViewModel, null, null);

                        // Find the Input
                        var nivm = NetworkViewModel.CurrentInputs.FirstOrDefault(x => x.Id == con.Input.Id);

                        // Find the Output
                        var novm = NetworkViewModel.CurrentOutputs.FirstOrDefault(x => x.Id == con.Output.Id);

                        // Rebuild the Connection
                        con.Rebuild(NetworkViewModel, nivm, novm);
                    }
                    NetworkViewModel.Connections.Edit(l => l.AddRange(vm.CurrentConnections));
                }

                // Find the Output
                Output = NetworkViewModel.CurrentNodes?.Select(x => x as TOutput).FirstOrDefault(x => x != null);
                ReInitialise();
            }
        }

        /// <summary>
        /// Saves the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Save(string key)
        {
            SuspensionDriver.CurrentKey = key ?? "default";
            SuspensionDriver.SaveState(NetworkViewModel).Subscribe();
            IsSaved = true;
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void SaveCurrent()
        {
            SuspensionDriver.SaveState(NetworkViewModel).Subscribe();
            IsSaved = true;
        }

        /// <summary>
        /// Only use after calling Clear() without a default node
        /// </summary>
        public void ReInitialise()
        {
            if (AutoCleanUp)
            {
                CleanUp.Dispose();
                CleanUp = new CompositeDisposable();
            }
            else
            {
                CleanUp = null;
            }
            config = new Configuration
            {
                Network = NetworkViewModel
            };
            var t = new Values<TOutput>(NetworkViewModel, Output, CleanUp);
            initialise.OnNext(t);
        }
    }

    [DataContract]
    public class Values<TOutput> : ICancelable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Values{T}"/> class.
        /// </summary>
        /// <param name="networkViewModel">The network view model.</param>
        /// <param name="output">The output.</param>
        /// <param name="cleanup">The cleanup.</param>
        public Values(NetworkViewModel networkViewModel, TOutput output, CompositeDisposable cleanup)
        {
            NetworkViewModel = networkViewModel;
            Output = output;
            CleanUp = cleanup ?? new CompositeDisposable();
        }

        /// <summary>
        /// Gets the clean up.
        /// </summary>
        /// <value>The clean up.</value>
        [IgnoreDataMember] public CompositeDisposable CleanUp { get; }

        /// <summary>
        /// Gets a value that indicates whether the object is disposed.
        /// </summary>
        [IgnoreDataMember] public bool IsDisposed => CleanUp.IsDisposed;

        /// <summary>
        /// Gets the network view model.
        /// </summary>
        /// <value>The network view model.</value>
        [DataMember] public NetworkViewModel NetworkViewModel { get; }

        /// <summary>
        /// Gets the output.
        /// </summary>
        /// <value>The output.</value>
        [DataMember] public TOutput Output { get; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => CleanUp.Dispose();
    }
}