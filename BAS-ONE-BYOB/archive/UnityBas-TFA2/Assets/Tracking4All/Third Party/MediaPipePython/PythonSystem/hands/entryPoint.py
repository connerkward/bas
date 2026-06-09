
from hands.hand import HandThread
import hands.global_vars as global_vars
import time
from sys import exit

def main():
    thread = HandThread()
    thread.start()

    i = input()
    print("Exiting…")        
    global_vars.KILL_THREADS = True
    time.sleep(0.5)
    exit()