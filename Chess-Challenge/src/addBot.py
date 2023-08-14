import re
import os
import shutil

# enum PlayerType (challenge_controller_file; 24)
# func CreatePlayer (challenge_controller_file; 214) # PlayerType.EvilBot => new ChessPlayer(new EvilBot(), type, GameDurationMilliseconds),
# Dict engines (menu_ui_file; 16) # { "EvilBot", ChallengeController.PlayerType.EvilBot },

curr_dir = r"C:\Users\as240\Downloads"
bots_dir = r"C:\Users\as240\OneDrive\Рабочий стол\Prejects\Tournament-Engine\Chess-Challenge\src\Bots"
challenge_controller_file = r"C:\Users\as240\OneDrive\Рабочий стол\Prejects\Tournament-Engine\Chess-Challenge\src\Framework\Application\Core\ChallengeController.cs"
menu_ui_file = r"C:\Users\as240\OneDrive\Рабочий стол\Prejects\Tournament-Engine\Chess-Challenge\src\Framework\Application\UI\MenuUI.cs"


classname_re = re.compile(r" *public class (\w+) *: *IChessBot", re.RegexFlag.M)

while True:
    try:
        bot_files = list(filter(lambda x: x.endswith(".cs"), os.listdir(curr_dir)))

        for ind, file in enumerate(bot_files):
            print(f"{ind}. {file}")

        file: str = bot_files[int(input())]
        break
    except (ValueError, IndexError):
        continue

shutil.copyfile(os.path.join(curr_dir, file), os.path.join(bots_dir, file))

with open(os.path.join(bots_dir, file), "r") as f:
    try:
        classname = re.findall(classname_re, f.read())[0]
    except IndexError:
        print("Cannot find bot class")
        exit(1)

print(classname)

with open(challenge_controller_file, "r") as fr:
    data = fr.read().splitlines()
    data.insert(24, f"{classname},")
    data.insert(data.index(" " * 12 + "return type switch") + 2, f"PlayerType.{classname} => new ChessPlayer(new {classname}(), type, GameDurationMilliseconds),")

with open(challenge_controller_file, "w") as fw:
    fw.write("\n".join(data))


with open(menu_ui_file, "r") as fr:
    data = fr.read().splitlines()
    data.insert(16, "{ \"classname\", ChallengeController.PlayerType.classname },".replace("classname", classname))

with open(menu_ui_file, "w") as fw:
    fw.write("\n".join(data))

print("Bot added successfully!")
