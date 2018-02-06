``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.125)
Intel Core i7-6600U CPU 2.60GHz (Skylake), 1 CPU, 4 logical cores and 2 physical cores
Frequency=2742189 Hz, Resolution=364.6722 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host]     : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


```
|      Method |     Mean |     Error |    StdDev |
|------------ |---------:|----------:|----------:|
| BleOvertime | 5.693 ms | 0.1131 ms | 0.2837 ms |
