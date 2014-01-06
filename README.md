# cs-benchmark

Simple C# Benchmark


## Sample Output

```
NORMAL :: 0,1050s (95,24mil. calls/sec) (100,0%)
VIRTUAL :: 0,1080s (92,59mil. calls/sec) (102,9%)
LAMBDA :: 0,0900s (111,11mil. calls/sec) (85,7%)
DIRECT DELEGATE :: 0,0840s (119,05mil. calls/sec) (80,0%)
REFLECT DELEGATE (System.Action) :: 0,0820s (121,95mil. calls/sec) (78,1%)
REFLECT DELEGATE (System.Delegate) :: 7,6740s (1,30mil. calls/sec) (7308,6%)
REFLECT INVOKE :: 2,7420s (3,65mil. calls/sec) (2611,4%)
Result: 70000000
```
