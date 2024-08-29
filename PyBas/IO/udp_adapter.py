import asyncio
import multiprocessing
import multiprocessing.sharedctypes
from queue import Empty
import time
import sys 
import os

from IO.mandrake_io.sockets.udp_server import UDPServer as Server

QUEUE_GET_TIMEOUT_SLEEP = 0.1
QUEUE_GET_TIMEOUT = 1

def start_udp_server(
    mp_udp_queue: multiprocessing.Queue, mp_stop_event=None, verbose=False
):
    
    server = Server(verbose=verbose)
    client_address = ('127.0.0.1', 8000)  # Replace with the actual client address

    while not mp_stop_event.is_set():
        try:
            queued_udp_msg = mp_udp_queue.get(
                timeout=QUEUE_GET_TIMEOUT
            )  # Set a timeout to avoid blocking indefinitely
            print("sent")
            server.send_message(
                client_address,
                message=queued_udp_msg,
            )
        except Empty:
            time.sleep(
                QUEUE_GET_TIMEOUT_SLEEP
            )  # Sleep to prevent a tight loop
        except Exception as e:
            print(f"Error while processing queue: {str(e)}")