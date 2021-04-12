namespace FSharpBackTest

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open Program
open Program.Requests
open Program.Logic

[<TestClass>]
type TestClass () =

    [<TestMethod>]
    member this.BuildCandle () =
        let swaps = [{amount0In = 0.0;
        amount0Out = 174.44608272526096;
        amount1In = 0.080949600322100662;
        amount1Out = 0.0;
        id = "";
        timestamp = 1618057552L;
        };{amount0In = 1000.0;
        amount0Out = 0.0;
        amount1In = 0.0;
        amount1Out = 0.46126405945605964;
        id = "";
        timestamp = 1618057497L;
        };{amount0In = 166.13505235610526;
        amount0Out = 0.0;
        amount1In = 0.0;
        amount1Out = 0.076633573809508873;
        id = "";
        timestamp = 1618057483L;
        };{amount0In = 0.0;
        amount0Out = 2154.9964695952181;
        amount1In = 1.0;
        amount1Out = 0.0;
        id = "";
        timestamp = 1618057416L;
        }]

        let res0 = 61679033.1148644m
        let res1 = 28535.6279902582m

        let expected = {_open = 0.0004625946443501111078511599m;
        close = 0.0004626471354879463556190697m;
        datetime = DateTime(1970, 01, 01);
        high = 0.0004626445106256093336472552m;
        low = 0.0004625946443501111078511599m;
        resolutionSeconds = 60;
        uniswapPairId = "";
        volume = 1.6188472335876695m;}

        let actual = buildCandle(swaps, res0, res1)

        Assert.AreEqual(expected, actual);
