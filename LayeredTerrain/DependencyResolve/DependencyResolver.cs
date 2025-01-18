using System;
using System.Collections.Generic;
using System.Linq;

namespace LayeredTerrain.DependencyResolve
{
    internal class DependencyResolver
    {
        private readonly Dictionary<string, int> items = new Dictionary<string, int>();
        private readonly List<HashSet<int>> adj = new List<HashSet<int>>();
        private readonly List<HashSet<int>> dependencies = new List<HashSet<int>>();
        private readonly List<string> strings = new List<string>();

        public void AddItem(string item)
        {
            if (items.ContainsKey(item))
                return;

            strings.Add(item);
            items[item] = items.Count;
            adj.Add(new HashSet<int>());
            dependencies.Add(new HashSet<int>());
        }

        public void AddDependencies(string dependent, params string[] dependencies)
        {
            foreach (var dep in dependencies)
                AddDependency(dependent, dep);
        }

        public void AddDependency(string dependent, string dependency)
        {
            if (!items.ContainsKey(dependent))
                throw new ArgumentException($"'{dependent}' has not been added to this DependencyResolver", nameof(dependent));

            if (!items.ContainsKey(dependency))
                throw new ArgumentException($"'{dependency}' has not been added to this DependencyResolver", nameof(dependency));

            adj[items[dependency]].Add(items[dependent]);
            dependencies[items[dependent]].Add(items[dependency]);
        }

        public IEnumerable<string> GetDependencies(string dependent)
        {
            return dependencies[items[dependent]].Select(i => strings[i]);
        }

        public string[] Solve()
        {
            int[] indegrees = new int[items.Count];
            Queue<int> q = new Queue<int>();
            string[] result = new string[items.Count];
            int resultPtr = 0;

            for (int i = 0; i < items.Count; i++)
                foreach (var n in adj[i])
                    indegrees[n] += 1;

            for (int i = 0; i < indegrees.Length; i++)
                if (indegrees[i] == 0)
                    q.Enqueue(i);

            while (q.Count > 0)
            {
                var item = q.Dequeue();
                result[resultPtr++] = strings[item];

                foreach (var n in adj[item])
                {
                    indegrees[n] -= 1;

                    if (indegrees[n] == 0)
                        q.Enqueue(n);
                }
            }

            if (resultPtr != result.Length)
            {
                string itemsString = string.Join(", ", strings);
                string adjacency = "";
                for (var i = 0; i < adj.Count; i++)
                {
                    adjacency += strings[i] + ": [ ";

                    foreach (var item in adj[i])
                        adjacency += strings[item] + " ";

                    adjacency += "]\n";
                }
                string resultString = string.Join(", ", result);

                throw new InvalidOperationException($"Circular dependency found\nItems: {itemsString}\nAdjacency Lists:\n{adjacency}\nResult: {resultString}\nResult Length: {result.Length}\nResult Pointer: {resultPtr}");
            }

            return result;
        }
    }
}
