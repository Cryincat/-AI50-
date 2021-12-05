#
#   Hello World server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#



from main import build_model
from main import build_agent
from rl.agents import DQNAgent
from rl.memory import SequentialMemory
from rl.policy import LinearAnnealedPolicy,EpsGreedyQPolicy
from tensorflow.keras.optimizers import Adam
from PatrollingProblem.lib.environnement import Environnement
from PatrollingProblem.envs import PatrollingProblemEnv



init = 0
nb_actions = 8

# if(init==0):
#     dqn = DQNAgent()
#     dqn.load_weights("Weights/ev2-Grid5v3(corridor)-600k.h5f")

import UdpComms as U
import time
import ast
import numpy as np

# Create UDP socket to use for sending (and receiving)
sock = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, enableRX=True, suppressWarnings=True)

i = 0
nbiter_per_episode = 100

while True:
    # sock.SendData('Sent from Python: ' + str(i)) # Send this string to other application

    data = sock.ReadReceivedData() # read data

    if data != None: # if NEW data has been received since last ReadReceivedData function call
        i += 1        
        data = data.split('\n')
        agentId = data[0]
        agentPos = ast.literal_eval(data[1])
        tiles = data[2:]
        
        nodes = []
        nodesTimes = {}
        
        for t in tiles:
            if(t != ""):
                temp = t.split('), ')
                node = ast.literal_eval("("+ str(temp[0][2:]) + ")")
                nodes.append(node)
                nodesTimes[node] = ast.literal_eval(temp[1][:len(temp[1])-1])
            
        # nodes = [ast.literal_eval(x) for x in tiles] #[literal_eval(x[1:].split(',')[0]) for x in tiles]
        e = Environnement(nodes,nodesTimes)
        # e.Display()
        
        if(i==1):
            model = build_model(e)
            dqn = build_agent(model, Environnement.actions)
            
        dqn.load_weights("Weights/3piecesCouloirV2.h5f")#("Weights/JosseSmaller-150k.h5f")
        env = PatrollingProblemEnv(e,None,nbiter_per_episode)
        # scores = dqn.test(env,nb_episodes=1,visualize = True)
        
        e = Environnement(nodes,nodesTimes)
        flt = e.Flatten(agentPos)
        
        state = dqn.memory.get_recent_state(flt)
        q_values = np.array(dqn.compute_q_values(state))
        arr = np.array(q_values).astype('float64')
        arr -= np.min(arr)
        softmax = np.exp(arr)/sum(np.exp(arr))
        
        def randomChoice(l):
            import random
            sum = 0
            accumulatedprobabilities = []
            for i in range(0,len(l)):
                sum+=l[i]
                accumulatedprobabilities.append(sum)
            r = random.random()
            for i in range(0,len(accumulatedprobabilities)):
                if accumulatedprobabilities[i] >= r : return i
                
        a = Environnement.actions[randomChoice(softmax)]
        # print(softmax)
        # print(agentPos)
        # print(a)
        
        sock.SendData(agentId+";"+str(a))
        
    time.sleep(0.0001)
    

# import time
# import zmq

# context = zmq.Context()
# socket = context.socket(zmq.REP)
# socket.bind("tcp://*:5555")

# while True:
#     #  Wait for next request from client
#     message = socket.recv()
#     message = str(message)
#     message = message[2:len(message)-1]
#     print("Received request: %s" % message)

#     if(init == 0):
#         data = message.split('\\n')
#         data = data[:len(data)-1]
#         # data = message.split("\n")
#         from ast import literal_eval as make_tuple
#         data = [make_tuple(d) for d in data]

#         e = Environnement(data)
#             # env = Environnement()
#             # dqn = DQNAgent()
#             # dqn.load_weights("Weights/ev2-Grid5v3(corridor)-600k.h5f")
#     if(init == 1):
#         model = build_model(e)
#         dqn = build_agent(model, Environnement.actions)
#     if(init== 2):
#         if(message == "learn"):
#             print("Learning...")
#             nbiter_per_episode = 500
#             nb_steps = 150000
#             dqn.compile(Adam(learning_rate=0.0001))
#             env = PatrollingProblemEnv(e,None,nbiter_per_episode)
#             hist = dqn.fit(env, nb_steps=nb_steps, visualize=False, verbose=1)
#             Y = [i / nbiter_per_episode for i in hist.history['episode_reward']]
#             X = [x for x in range(len(Y))]
            
#             from matplotlib import pyplot as plt
#             plt.plot(X,Y)
#             dqn.save_weights("Weights/JosseSmaller-150k.h5f",overwrite=True)
#         else:
#             print("Loading...")
#             dqn.load_weights("Weights/JosseSmaller-150k.h5f")
            
#         #  Do some 'work'.
#         #  Try reducing sleep time to 0.01 to see how blazingly fast it communicates
#         #  In the real world usage, you just need to replace time.sleep() with
#         #  whatever work you want python to do, maybe a machine learning task?
#     init+=1
#     time.sleep(1)

#         # #  Send reply back to client
#         # #  In the real world usage, after you finish your work, send your output here
#     socket.send(b"World")
