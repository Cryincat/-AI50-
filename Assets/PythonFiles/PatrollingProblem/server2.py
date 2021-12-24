#
#   Hello World server in Python
#   Binds REP socket to tcp://*:5555
#   Expects b"Hello" from client, replies with b"World"
#


import numpy as np
import ast
import time
import UdpComms as U
# from main import build_model
# from main import build_agent
from rl.agents import DQNAgent
from rl.memory import SequentialMemory
from rl.policy import LinearAnnealedPolicy, EpsGreedyQPolicy
from tensorflow.keras.optimizers import Adam
from PatrollingProblem.lib.environnement import Environnement
from PatrollingProblem.envs import PatrollingProblemEnv
from matplotlib import pyplot as plt

from PatrollingProblem.envs import PatrollingProblemEnv
from PatrollingProblem.lib.environnement import Environnement
import numpy as np

from rl.agents import DQNAgent
from rl.memory import SequentialMemory
from rl.policy import LinearAnnealedPolicy,EpsGreedyQPolicy

from tensorflow import keras
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense, Activation, Flatten
from tensorflow.keras.optimizers import Adam
from tensorflow.keras.optimizers import SGD

init = 0
nb_actions = 8

# if(init==0):
#     dqn = DQNAgent()
#     dqn.load_weights("Weights/ev2-Grid5v3(corridor)-600k.h5f")


# Create UDP socket to use for sending (and receiving)
sock = U.UdpComms(udpIP="127.0.0.1", portTX=8000, portRX=8001, tailleBuffer = 10240,
                  enableRX=True, suppressWarnings=True)

i = 0
nbAgent = 1
nbiter_per_episode = 500
agents = []

print("Server is now running")

while True:
    # sock.SendData('Sent from Python: ' + str(i)) # Send this string to other application

    data = sock.ReadReceivedData()  # read data

    if data != None:  # if NEW data has been received since last ReadReceivedData function call
        i += 1
        print("iter "+str(i) + " message received is : "+data)

        if(i == 1):
            learningFromScratch = data[0] == "t"
            data = data.split(",")
            nbAgent = int(data[1])
            if learningFromScratch:
                nbIter = int(data[2])
                fileWeights = data[3]
            else:
                fileWeights = data[2]
        else:
            data = data.split('\n')
            agentId = data[0]
            agentPos = ast.literal_eval(data[1])
            agents = []# agentPos = ast.literal_eval(data[1])
            agentsStr = data[2].split(";")
            for d in agentsStr:
                if d != "" : agents.append(ast.literal_eval(d))
            tiles = data[3:]

            nodes = []
            nodesTimes = {}

            for t in tiles:
                if(t != ""):
                    temp = t.split('), ')
                    node = ast.literal_eval("(" + str(temp[0][2:]) + ")")
                    nodes.append(node)
                    nodesTimes[node] = ast.literal_eval(
                        temp[1][:len(temp[1])-1])

            e = Environnement(agents,nodes, nodesTimes)
            env = PatrollingProblemEnv(e, agents ,len(agents), nbiter_per_episode)

            if(i == 2):
                
                def build_model(environnement):
                    print("Building Model ...")
                    input_shape = (1,) + (environnement.Shape(),)
                    model = Sequential()
                    # model.add(Input(shape = input_shape))
                    model.add(Flatten(input_shape = input_shape))
                    model.add(Activation('relu'))
                    # model.add(Dense(9))
                    # model.add(Activation('relu'))
                    # model.add(Dense(10))
                    model.add(Dense((4*environnement.Shape())))
                    model.add(Activation('relu'))
                    # model.add(Dense((environnement.Shape())))
                    # model.add(Activation('relu'))
                    model.add(Dense((2*environnement.Shape())))
                    model.add(Activation('relu'))
                    # model.add(Dense((environnement.Shape())))
                    # model.add(Activation('relu'))
                    # model.add(Dense((environnement.Shape())/2+nb_actions))
                    # model.add(Activation('relu'))
                    # model.add(Dense(24))
                    # model.add(Activation('relu'))
                    model.add(Dense(nb_actions))
                    model.add(Activation('softmax'))
                    print(model.summary())
                    return model
                
                def build_agent(model,actions):
                    print("Building Agent ...")
                    policy = LinearAnnealedPolicy(inner_policy=EpsGreedyQPolicy(), attr="eps", value_max=1.0,
                                                  value_min=0.1, value_test=0.2, nb_steps = 10000)
                    # policy = EpsGreedyQPolicy()
                    memory = SequentialMemory(limit=10000, window_length=1)
                    dqn = DQNAgent(model=model,memory=memory,policy=policy,enable_dueling_network=(True),
                                    dueling_type='avg',nb_actions=nb_actions,nb_steps_warmup=1000)
                    # dqn = DQNAgent(model=model,memory=memory,policy=policy,nb_actions=nb_actions)
                    
                    # initial_learning_rate = 0.1
                    # lr_schedule = keras.optimizers.schedules.ExponentialDecay(
                    #     initial_learning_rate,
                    #     decay_steps=10000,
                    #     decay_rate=0.96,
                    #     staircase=False)
                    lr_schedule = 0.0001

                    dqn.compile(Adam(learning_rate=lr_schedule))#0.0001))
                    return dqn
                
                model = build_model(e)
                dqn = build_agent(model, Environnement.actions)

                if(learningFromScratch):
                    print("Training the model ...")
                    hist = dqn.fit(env, nb_steps=nbIter,
                                   visualize=False, verbose=1)
                    Y = [i / nbiter_per_episode for i in hist.history['episode_reward']]
                    X = [x for x in range(len(Y))]

                    plt.plot(X, Y)

                    dqn.save_weights(fileWeights, overwrite=True)
                    break
                else:
                    dqn.load_weights(fileWeights)

                # ("Weights/JosseSmaller-150k.h5f")
                # env = PatrollingProblemEnv(e,None,nbiter_per_episode)
                # scores = dqn.test(env,nb_episodes=1,visualize = True)

            e = Environnement(agents,nodes, nodesTimes)
            flt = e.Flatten(e.agents.index(agentPos))

            state = dqn.memory.get_recent_state(flt)
            q_values = np.array(dqn.compute_q_values(state))
            arr = np.array(q_values).astype('float64')
            arr -= np.min(arr)
            
            for i in range(len(arr)):
                newPos = (agentPos[0] + Environnement.actions[i][0],agentPos[1] + Environnement.actions[i][1])
                if(newPos not in e.nodes):
                    arr[i] = 0
                    
            norm =  np.linalg.norm(arr)
            factor = 20
            normarr = arr / norm
            
                    
            softmax = np.exp(factor * normarr)/sum(np.exp(factor * normarr))
            
            def randomChoice(l):
                for i in range(len(l)):
                    if(np.isnan(l[i])) : l[i] = 0
                import random
                sum = 0
                accumulatedprobabilities = []
                for i in range(0, len(l)):
                    sum += l[i]
                    accumulatedprobabilities.append(sum)
                r = random.random()
                for i in range(0, len(accumulatedprobabilities)):
                    if accumulatedprobabilities[i] >= r:
                        return i
            
            a = Environnement.actions[randomChoice(softmax)]
            # print(softmax)
            # print(agentPos)
            # print(a)
            
            message = agentId+";"+str(a)
            print("Sending : "+message)
            sock.SendData(message)

    time.sleep(0.0001)
