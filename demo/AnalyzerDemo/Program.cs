﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var list = new[] { 1 };

            var abc = list.Where(x => x > 1);

            list?
                .Select(
                    x =>
                        x > list.First() ? 2 : 0);

            TestMethod(abc)?.Where(x => x < 10);

            list
                .Select(
                    x =>
                        list
                            .Select(
                                y =>
                                    y + 1));

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

            list.Where(x => { return x > 1; });
        }

        private static IEnumerable<int> TestMethod(IEnumerable<int> list)
        {
            return list.Where(x => x > 1);
        }
    }
}
