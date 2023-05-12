using System;
using System.Linq;
using System.Reactive.Linq;
using Xunit;

namespace Graph.Tests
{
    public class GraphTest
    {
        /// <summary>
        /// Teste original que constava no GitHub. Abaixo, consta outro teste com um grafo mais complexo,
        /// para mostrar que a solução funciona em qualquer grafo.
        /// </summary>
        [Fact]
        public void TestRoutesBetweenTwoPoints()
        {
            var links = new ILink<string>[]
            {
                new Link<string>("a","b"),
                new Link<string>("b","c"),
                new Link<string>("c","b"),
                new Link<string>("b","a"),
                new Link<string>("c","d"),
                new Link<string>("d","e"),
                new Link<string>("d","a"),
                new Link<string>("a","h"),
                new Link<string>("h","g"),
                new Link<string>("g","f"),
                new Link<string>("f","e"),
            };

            var graph = new Graph<string>(links);
            var paths = graph.RoutesBetween("a", "e");
            var list = paths.ToEnumerable().ToArray();
            Assert.Equal(2, list.Length);
            Assert.Contains(list, l => String.Join("-", l) == "a-b-c-d-e");
            Assert.Contains(list, l => String.Join("-", l) == "a-h-g-f-e");
        }

        /// <summary>
        /// Teste desenvolvido por mim para mostrar que a solução funciona em qualquer grafo.
        /// </summary>
        [Fact]
        public void TestRoutesBetweenTwoPoints_2()
        {
            var links = new ILink<string>[]
            {
                new Link<string>("a","e"),
                new Link<string>("a","b"),
                new Link<string>("b","c"),
                new Link<string>("c","b"),
                new Link<string>("b","a"),
                new Link<string>("c","h"),
                new Link<string>("d","e"),
                new Link<string>("a","d"),
                new Link<string>("a","h"),
                new Link<string>("h","g"),
                new Link<string>("g","f"),
                new Link<string>("f","e"),
                new Link<string>("b","e"),
                new Link<string>("c","e"),
                new Link<string>("a","f"),
                new Link<string>("h","e"),
            };

            var graph = new Graph<string>(links);
            var paths = graph.RoutesBetween("a", "e");
            var list = paths.ToEnumerable().ToArray();
            Assert.Equal(9, list.Length);
            Assert.Contains(list, l => String.Join("-", l) == "a-e");
            Assert.Contains(list, l => String.Join("-", l) == "a-b-e");
            Assert.Contains(list, l => String.Join("-", l) == "a-d-e");
            Assert.Contains(list, l => String.Join("-", l) == "a-h-e");
            Assert.Contains(list, l => String.Join("-", l) == "a-f-e");
            Assert.Contains(list, l => String.Join("-", l) == "a-b-c-e");
            Assert.Contains(list, l => String.Join("-", l) == "a-b-c-h-e");
            Assert.Contains(list, l => String.Join("-", l) == "a-h-g-f-e");
            Assert.Contains(list, l => String.Join("-", l) == "a-b-c-h-g-f-e");
        }
    }
}
