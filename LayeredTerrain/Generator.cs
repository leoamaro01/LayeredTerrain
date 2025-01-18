using LayeredTerrain.DependencyResolve;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LayeredTerrain
{
    public class Generator
    {
        public Feature[] Features { get; private set; }
        private readonly Dictionary<string, Feature> namedFeatures;
        public Generator(in FeatureOptions[] features)
        {
            Features = new Feature[features.Length];
            namedFeatures = new Dictionary<string, Feature>();

            var nameIndexes = new Dictionary<string, int>();

            var dependencyResolver = new DependencyResolver();

            for (var i = 0; i < features.Length; i++)
            {
                nameIndexes.Add(features[i].FeatureName, i);
                dependencyResolver.AddItem(features[i].FeatureName);
            }

            foreach (var f in features)
                foreach (var l in f.LayerOptions)
                    dependencyResolver.AddDependencies(f.FeatureName, l.FeatureDependencies);

            var featureCreationOrder = dependencyResolver.Solve();

            foreach (var f in featureCreationOrder)
            {
                var index = nameIndexes[f];
                var opt = features[index];

                if (namedFeatures.ContainsKey(opt.FeatureName))
                    throw new ArgumentException("There should be no repeated feature names.", nameof(features));

                var deps = dependencyResolver.GetDependencies(opt.FeatureName);
                var depsNamesPairs = deps.Select(name => new KeyValuePair<string, Feature>(name, namedFeatures[name]));
                var dependenciesDict = new Dictionary<string, Feature>(depsNamesPairs);

                Features[index] = new Feature(opt, dependenciesDict);

                namedFeatures[opt.FeatureName] = Features[index];
            }


        }

        public Feature this[string name]
        {
            get
            {
                if (!namedFeatures.TryGetValue(name, out Feature? value))
                    throw new InvalidOperationException("There is no feature with that name.");

                return value;
            }
        }
    }
}
