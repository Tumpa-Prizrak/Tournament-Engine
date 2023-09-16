from os import system, listdir

curr_dir = r"C:\Users\as240\Downloads"

for file in listdir(curr_dir):
    system(f"python addBot.py {file}")
system("pause")
