# BASeTris

![BASeTris Title Screen](https://github.com/BCProgramming/BASeTris/blob/master/screenshot/basetris_title.png?raw=true)

A Tetris Engine/Clone built in C#.

Contains Handlers for Tetris, Tetris 2, Dr. Mario, Tetris Attack, Columns, and a number of variants. Most of these were effectively me testing the limits of the underlying engine. Dr.Mario doesn't clear lines, but instead blocks "pop" and they can also fall. 
Tetris 2 is unique as well as some assumptions one might make for the Dr.Mario implementation won't work there, due to the larger tetromino size.

Columns was interesting, too. In that game, there are only three block vertical columns, and "rotating" actually rotates the blocks within that column. This was an interesting challenge but it actually just made use of an ancient design decision I had made whereby Nomino "rotations" can be encoded not only to not rotate but with any number of elements. So that aspect was actually surprisingly easy, As the Nomino's just had to have three rotations where the block positions changed.

Tetris Attack is a quite different game too. You don't have blocks falling at all; instead, you have a selection cursor and you can swap the positions of blocks on screen. This was fun to implement. I made the "selection cursor" the Active group and overrode the standard functionality to make it function different than usual. This was fundamentally a sort of "unit test" to see just how flexible the IGameCustomizationHandler interface design I had started using was.


Dr.Mario, Tetris 2, and Tetris Attack have different modes as well, Dr.Mario can have 6 viruses for example, or use the 3 Viruses I designed, which I called "Mr.Mario"; Similarly Tetris 2 can use 6 block colours. At this rate what I need to do is create a new setup such that game handlers can have some sort of customization screen before they actually launch up into the gameplay.

Other planned additions I have in mind are a new theme based on the NES Tetris from Tengen, as well as a new game mode called "Reverse Tetris" where you need to clear a nearly full field of blocks from the top down by slotting tetrominoes in from the top.




This project makes use of some of my other libraries: [BCRendering](https://github.com/BCProgramming/BCRendering), [Elementizer](https://github.com/BCProgramming/Elementizer), and [BASeScores](https://github.com/BCProgramming/BASeScores).

BASeTris started partly because I was somewhat disappointed in the availability and features of official Tetris offerings. My idea was a sort of weird Tetris All-stars thing where you could flip between various themes, or it mixed things up and used for example both NES and SNES line clears and so on.

There is a game mode which effectively allows any number of block sized nominoes. This is made available through a bunch of game modes such as Pentris and Hextris, with "Joke" versions such as Centris. The field size is larger for larger block counts, and the Nominoes are actually generated on the fly as well.








I have a Jenkins CI configured which will upload new versions as needed every two days. The latest build's installer can be directly accessed via [this](https://bc-programming.com/blogs/?smd_process_download=1&download_id=2717) URL.

