using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FluentAssertions;
using MasjidBandung.Models;
using MasjidBandung.Services;
using Xunit;

namespace Tests;

public class TestData : IEnumerable<object?[]> {
    public IEnumerator<object?[]> GetEnumerator() {
        var inputs = new List<MotorCommandRequest> {
            new() {
                Coreography = "Test1",
                Position = new double[] {100, 100, 100, 100},
                Color = new[] {"#ffffff", "#ffffff", "#ffffff", "#ffffff"},
                Speed = null,
                Time = 10
            },
            new() {
                Coreography = "Test2",
                Position = new double[] {50, 100, 100, 100},
                Color = new[] {"#ffffff", "#ffffff", "#ffffff", "#ffffff"},
                Speed = null,
                Time = 10
            },
            new() {
                Coreography = "Test3",
                Position = new double[] {50, 100, 50, 50},
                Color = new[] {"#ffffff", "#ffffff", "#ffffff", "#ffffff"},
                Speed = null,
                Time = 10
            }
        };
        var posResults = new List<double[]> {
            new double[] {400, 400, 400, 400},
            new double[] {200, 400, 400, 400},
            new double[] {200, 400, 200, 200}
        };
        var speedResults = new List<int[]> {
            new[] {2400, 2400},
            new[] {2400, 2400},
            new[] {2400, 1200}
        };

        // var speedResult3
        for (int i = 0; i < inputs.Count; i++) {
            yield return new object?[] {inputs[i], posResults[i], inputs[i].Time, speedResults[i]};
        }

        // yield return new object[] {2, 2, 4};
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// public class UnitTest1 {
//     [Theory]
//     [ClassData(typeof(TestData))]
//     public void Test1(MotorCommandRequest cmd, double[] positions, int duration, int[] speeds) {
//         Debug.Assert(cmd.Position != null, "cmd.Position != null");
//         Debug.Assert(cmd.Time != null, "cmd.Time != null");
//         var result = MotorCommand.WithDuration(cmd.Position, cmd.Time.Value);
//         result.Speeds.Should().Equal(speeds, "Kecepatan tidak sesuai");
//         result.NewPositions.Should().Equal(positions, "Posisi tidak sesuai");
//         result.Duration.Should().Be(duration);
//         // Assert.Equal(result.NewPositions, position);
//         // Assert.Equal(result.Duration, duration);
//     }
// }
