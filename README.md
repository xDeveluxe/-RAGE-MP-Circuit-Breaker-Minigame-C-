# Circuit Breaker for RAGE:MP
Minigame from the original heist update, done with scaleform without CEF.

Installing:
Simply put all files in client_packages\cs_packages\

To call that script:
Event name: 
CircuitBreakerStart

Event args:
Count of Lives (1 - 10)
Difficulty Level (0 - 4) (0 is beginner, 4 is expert)
Levels to complete (1 - 6)

C# Server Example:
player.TriggerEvent("CircuitBreakerStart", 5, 1, 6);

Game results events:
CircuitBreakerWIN - is called at client side, when player succeed at all levels, no args.
CircuitBreakerLOSE - is called at client side, when player quits (Q button) OR lost all his lives, no args.

If you are using JS client side:
You need to create 2 events (described above) at your client side to catch game results.

If you are using C# client side:
You can just modify 2 methods inside Main.cs, which already contains both methods to catch game results.

Minigame Preview, done on this system: 
https://www.youtube.com/watch?v=a2FbRkjS4R4
