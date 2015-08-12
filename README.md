# potw_7

Consider the set of mazes on a 4x4 grid. The outside edges are filled in, but the 24 internal edges may or may not be 'walls.' Here's an example (excuse the ASCII):

```
 _ _ _ _ 
| |  _| |
| |     |
|  _| | |
|_|_ _|_|
```

Given there are 24 internal edges, this means there are 2^24 = 16,777,216 possible mazes.
Now consider the set of 'correct' mazes where every pair of squares has a unique path between them. The maze above is a 'correct' maze while this one is not:

```
 _ _ _ _ 
| | |_| |
|       |
|  _| | |
|_|_ _|_|
```

Out of the 16 some million possible mazes, how many are 'correct'? For a secondary problem, come up with an algorithm for determining if a given maze is 'correct.'
