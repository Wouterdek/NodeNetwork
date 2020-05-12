using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using Newtonsoft.Json;
using ReactiveUI;

namespace NodeNetwork.Toolkit.SuspensionDrivers
{
    public class DictionaryToFileSuspensionDriver : ReactiveObject, ISuspensionDriver
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DictionaryToFileSuspensionDriver"/> class.
        /// </summary>
        /// <param name="initialKey">The initial key.</param>
        public DictionaryToFileSuspensionDriver(string initialKey = "default")
        {
            CurrentKey = initialKey;
            _expressionKeys.Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _expressions).Subscribe();
            _expressionKeys.Add(initialKey);
        }
        /// <summary>
        /// Gets or sets the current key.
        /// </summary>
        /// <value>The current key.</value>
        public string CurrentKey
        {
            get => _CurrentKey;
            internal set
            {
                this.RaiseAndSetIfChanged(ref _CurrentKey, value);
            }
        }
         private string _CurrentKey;

        /// <summary>
        /// Gets the expressions.
        /// </summary>
        /// <value>
        /// The expressions.
        /// </value>
        public ReadOnlyObservableCollection<string> Expressions => _expressions;
        private readonly ReadOnlyObservableCollection<string> _expressions;
        private SourceList<string> _expressionKeys = new SourceList<string>();

        /// <summary>
        /// Gets the expression list.
        /// </summary>
        /// <value>The expression list.</value>
        public Dictionary<string, string> ExpressionList { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets a value indicating whether this instance has expressions.
        /// </summary>
        /// <value><c>true</c> if this instance has expressions; otherwise, <c>false</c>.</value>
        public bool HasExpressions => ExpressionList.Count > 0;

        /// <summary>
        /// Invalidates the application state (i.e. deletes it from disk).
        /// </summary>
        /// <returns>A completed observable.</returns>
        public IObservable<Unit> InvalidateState()
        {
            if (ExpressionList.ContainsKey(CurrentKey))
            {
                ExpressionList.Remove(CurrentKey);
            }

            return Observable.Return(Unit.Default);
        }

        /// <summary>
        /// Loads all.
        /// </summary>
        /// <param name="folder">The folder.</param>
        public void LoadAll(string folder)
        {
            if (Directory.Exists(folder))
            {
                foreach (var file in Directory.GetFiles(folder, "*.expr"))
                {
                    var key = file.Replace(folder, "").Replace(".expr", "");
                    ExpressionList.Add(key, File.ReadAllText(file));
                }
                _expressionKeys.Edit(x =>
               {
                   x.Clear();
                   x.AddRange(ExpressionList.Keys);
               });

            }
            else
            {
                Directory.CreateDirectory(folder);
            }
        }

        /// <summary>
        /// Loads the application state from persistent storage.
        /// </summary>
        /// <returns>An object observable.</returns>
        public IObservable<object> LoadState()
        {
            if (ExpressionList.Count > 0)
            {
                var lines = ExpressionList.ContainsKey(CurrentKey) ? ExpressionList[CurrentKey] : ExpressionList.Values.First();
                try
                {
                    var state = JsonConvert.DeserializeObject<object>(lines, _settings);
                    return Observable.Return(state);
                }
                catch { }
                
            }
            return Observable.Empty<object>();
        }

        /// <summary>
        /// Saves all.
        /// </summary>
        /// <param name="folder">The folder.</param>
        public void SaveAll(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            foreach (var kvp in ExpressionList)
            {
                File.WriteAllText($"{folder}\\{kvp.Key}.expr", kvp.Value);
            }
        }

        /// <summary>
        /// Saves the application state to disk.
        /// </summary>
        /// <param name="state">The application state.</param>
        /// <returns>A completed observable.</returns>
        public IObservable<Unit> SaveState(object state)
        {
            var lines = JsonConvert.SerializeObject(state, Formatting.Indented, _settings);
            ExpressionList[CurrentKey ?? "default"] = lines;
            _expressionKeys.Edit(x =>
            {
                x.Clear();
                x.AddRange(ExpressionList.Keys);
            });
            return Observable.Return(Unit.Default);
        }
    }
}
