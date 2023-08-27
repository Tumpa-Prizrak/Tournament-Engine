# Tournament engine
## Overview
This project was created to make it easier to create matches between bots and add bots to the project
## Features
- Quickly add bots to a project.
- More convenient system of launching games.
- Convenient statistics on games during and after the match
## Getting Started
1. Clone the repository
2. Install [Python](http://python.org)
3. Configure addBot.py variables:
- **curr_dir** - *directory with bots to copy from*
- **bots_dir** - *directory for bots to copy to*
- **challenge_controller_file** - *challangeController.cs in this project*
- **menu_ui_file** - *same, but with menu_ui*
- Now, when you start addBot.py you can see all bot's files. Enter number of bot to add it to engine

4. Configure FEN.txt:
- Create Fens.txt file in *Chess-Challenge\\resourses*
- Fill it with [fens](https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation) for starting positions. Each fen should be on new line
- Standart fens can be found [here](https://github.com/SebLague/Chess-Challenge/blob/main/Chess-Challenge/resources/Fens.txt)

5. Configure C# files (optional):
- Change *Chess-Challenge\\src\\Framework\\Application\\Core\\Settings.cs*:
  - *GameDurationMilliseconds* and *IncrementMilliseconds* to change time format
  - *ScreenSizeBig* and *ScreenSizeSmall* to change application resolution
- Change *Chess-Challenge\\src\\Framework\\Application\\UI\\BoardTheme.cs* to change tile colors

6. Start project with `dotnet run`
- alternatively you can run `start tournament engine.bat` to run 4 instances at a time

7. Enjoy! Let me know if you have any other questions.
