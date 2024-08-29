import asyncio
import multiprocessing
import multiprocessing.sharedctypes
from queue import Empty
import time
import sys 
import os

from IO.mandrake_io.sockets.socketio_server import SocketIOServer as Server


QUEUE_GET_TIMEOUT_SLEEP = 0.1
QUEUE_GET_TIMEOUT = 1
SOCKETIO_EVENT_NAME = "keypoints"

def start_socketio_server(
    mp_udp_queue: multiprocessing.Queue, mp_stop_event=None, verbose=False
):
    server = Server(verbose=verbose)

    async def process_queue():
        while not mp_stop_event.is_set():
            try:
                queued_udp_msg = mp_udp_queue.get(
                    timeout=QUEUE_GET_TIMEOUT
                )  # Set a timeout to avoid blocking indefinitely
                # print("sent")
                await server.send_message_all(
                    message=queued_udp_msg, event_name=SOCKETIO_EVENT_NAME
                )
            except Empty:
                await asyncio.sleep(
                    QUEUE_GET_TIMEOUT_SLEEP
                )  # Sleep to prevent a tight loop
                # print("Queue is empty.")
            except Exception as e:
                print(f"Error while processing queue: {str(e)}")

    async def main():
        await asyncio.gather(
            server.start(),  # Start the server
            process_queue(),  # Run the queue processing concurrently with the server
        )

    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        loop.run_until_complete(main())
    finally:
        loop.run_until_complete(server.stop())
        mp_stop_event.set()
        loop.close()


# if __name__ == "__main__":
#     network_queue = multiprocessing.Queue()
#     mp_stop_event = multiprocessing.Event()

#     socket_thread = multiprocessing.Process(
#         target=start_socketio_server,
#         kwargs={"mp_udp_queue": network_queue, "mp_stop_event": mp_stop_event},
#         daemon=True,
#     )
#     data_thread = multiprocessing.Process(
#         target=generate_empty_data,
#         kwargs={"mp_udp_queue": network_queue, "mp_stop_event": mp_stop_event},
#         daemon=True,
#     )

#     socket_thread.start()
#     data_thread.start()

#     # emulate core loop 
#     while True:
#         pass
