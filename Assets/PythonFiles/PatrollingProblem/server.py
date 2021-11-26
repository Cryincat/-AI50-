#
#   Hello World server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#

import time
import zmq

from main import build_model
from main import build_agent
from rl.agents import DQNAgent
from rl.memory import SequentialMemory
from rl.policy import LinearAnnealedPolicy,EpsGreedyQPolicy
from tensorflow.keras.optimizers import Adam
from PatrollingProblem.lib.environnement import Environnement
from PatrollingProblem.envs import PatrollingProblemEnv

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:5555")

init = 0
nb_actions = 8

# if(init==0):
#     dqn = DQNAgent()
#     dqn.load_weights("Weights/ev2-Grid5v3(corridor)-600k.h5f")

while True:
    #  Wait for next request from client
    message = socket.recv()
    message = str(message)
    message = message[2:len(message)-1]
    # print("Received request: %s" % message)

    if(init == 0):
        data = message.split('\\n')
        data = data[:len(data)-1]
        # data = message.split("\n")
        from ast import literal_eval as make_tuple
        data = [make_tuple(d) for d in data]

        e = Environnement(data)
            # env = Environnement()
            # dqn = DQNAgent()
            # dqn.load_weights("Weights/ev2-Grid5v3(corridor)-600k.h5f")
    if(init == 1):
        model = build_model(e)
        dqn = build_agent(model, Environnement.actions)
    if(init== 2):
        if(message == "learn"):
            nbiter_per_episode = 500
            nb_steps = 150000
            dqn.compile(Adam(learning_rate=0.0001))
            env = PatrollingProblemEnv(e,None,nbiter_per_episode)
            hist = dqn.fit(env, nb_steps=nb_steps, visualize=False, verbose=1)
            Y = [i / nbiter_per_episode for i in hist.history['episode_reward']]
            X = [x for x in range(len(Y))]
            
            from matplotlib import pyplot as plt
            plt.plot(X,Y)
            dqn.save_weights("Weights/JosseSmaller-150k.h5f",overwrite=True)
        else:
            dqn.load_weights("Weights/JosseSmaller-150k.h5f")
        #  Do some 'work'.
        #  Try reducing sleep time to 0.01 to see how blazingly fast it communicates
        #  In the real world usage, you just need to replace time.sleep() with
        #  whatever work you want python to do, maybe a machine learning task?
    init+=1
    time.sleep(1)

        # #  Send reply back to client
        # #  In the real world usage, after you finish your work, send your output here
    socket.send(b"init")
