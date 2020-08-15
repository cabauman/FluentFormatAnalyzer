using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var imAReallyLooongVariableDeclaration = 0;
            var list = new[] { 1 };

            var abc = list.Where(x => x > 1);

            TestMethod(
                abc)
                .Where(x => x > 1);

            list
                .Select(
                    x =>
                        list
                            .Select(
                                y =>
                                    y + 1))
                .Where(
                    x =>
                        x.First() > 1);

            list
                .Where(x =>
            x > 1);

            list
                .Where(
                    x => x > 1);

            list
                .Where(
                    x =>
                        list
                            .Skip(1)
                            .First()
                        > imAReallyLooongVariableDeclaration);

            list
                .Where(
                    x =>
                        list
                            .First()
                        >=
                        list
                            .Skip(1)
                            .Last());

            list
                .Where(
                    x =>
                        x
                        == list
                            .Skip(1)
                            .First());

            list
                .Where(
                    x =>
                        list
                            .Skip(1)
                            .First()
                        == list
                            .Skip(1)
                            .First());

            list
                .Select(
                    x =>
                        x > 1
                        ? list
                            .Skip(1)
                            .First()
                        : list
                            .Skip(1)
                            .Last());

            list
                .Select(
                    x =>
                        x
                        > list
                            .Skip(1)
                            .First()
                        ? list
                            .Skip(1)
                            .First()
                        : list
                            .Skip(1)
                            .Last());

            list
                .Select(
                    x =>
                        list.Last()
                        > list.Last()
                        ? list.First()
                        : list.Last());

            list
                .Select(x => new object())
                .Select(
                    x =>
                        x
                        ?? list
                            .Take(1)
                            .First());

            list
                .Aggregate(
                    0,
                    (a, b) =>
                        a + b);

            list
                .GroupBy(
                    x =>
                        x,
                    x =>
                        x);

            list
                .Where(
                x =>
                {
                    return x > 1;
                });
        }

        private static IEnumerable<int> TestMethod(IEnumerable<int> list)
        {
            return list.Where(x => x > 1);
        }
    }
}
