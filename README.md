A chess app written in C# WPF, with support for both a local game and a game against the computer.


**AI:**

- The AI uses the minimax algorithm, and currently uses a depth-limited search, although will be switched to iterative deepening search soon.
- Alpha-beta pruning and move-ordering optimisations have been applied to improve performance
- The evaluation function involves summing material point differences, and adding bonuses for pieces standing on beneficial squares (piece square table), but this decreases in weight as more pieces get captured

<img width="727" alt="image" src="https://github.com/JasjotSingh001/ChessAI/assets/53977657/7de4a996-db3c-4b5d-97a0-21dcf43b17c3">
