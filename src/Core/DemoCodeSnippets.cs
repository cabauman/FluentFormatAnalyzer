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
            var list = new[] { 1 };

            var abc = list.Where(x => x > 1);

            var abc1 = list.Where(x => list.First() > list.Last());

            var abc2 = list
                .Where(
                    x => list.First() > list.Last());

            var abc3 = list
                .Where(
                    x =>
                        list.First() > list.Last());

            var abc4 = list
                .Where(
                    x =>
                        list.First() >
                        list.Last());

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

            var t = list.ToString() != list
                .Length
                .ToString()
                    ? list
                        .Length
                        .ToString()
                    : list
                        .Length
                        .ToString();

            var u = list.ToString() != list.ToString()
                ? list
                    .Length
                    .ToString()
                : list
                    .Length
                    .ToString();

            var v = list
                .Length
                .ToString() != null
                    ? list
                        .Length
                        .ToString()
                    : list
                        .Length
                        .ToString();

            var w = list
                .Length
                .ToString() != list
                .Length
                .ToString()
                    ? list
                        .Length
                        .ToString()
                    : list
                        .Length
                        .ToString();

            var z = list
                .Length
                .ToString() != list
                    .Length
                    .ToString()
                        ? list
                            .Length
                            .ToString()
                        : list
                            .Length
                            .ToString();

            var z1 =
                list
                    .Length
                    .ToString() !=
                list
                    .Length
                    .ToString()
                ? list
                    .Length
                    .ToString()
                : list
                    .Length
                    .ToString();

            var z2 =
                list
                    .Length
                    .ToString()
                != list
                    .Length
                    .ToString()
                ? list
                    .Length
                    .ToString()
                : list
                    .Length
                    .ToString();

            var z3 = list.Length
                > list.Length
                ? list.Length
                : list.Length;

            var z4 = list.Length != list.Length
                ? list.Length
                : list.Length;

            var z5 = list.Length
                > list
                    .Skip(1)
                    .Count();

            var z6 =
                list
                    .Skip(1)
                    .Count()
                > list
                    .Skip(1)
                    .Count();

            var z7 =
                list
                    .Skip(1)
                    .Count()
                == list
                    .Count();
        }

        private static IEnumerable<int> TestMethod(IEnumerable<int> list)
        {
            return list.Where(x => x > 1);
        }
    }
}
