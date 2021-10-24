#
#   Hello World server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

import time
import zmq

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

print("initialing Python Server !")
seconds = time.time()

while int(time.time() - seconds) < 5:
    print(float(time.time() - seconds))
    time.sleep(0.1)
