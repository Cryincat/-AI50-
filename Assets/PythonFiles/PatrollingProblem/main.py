# -*- coding: utf-8 -*-
"""
Created on Fri Nov 12 22:02:40 2021

@author: adrien
"""

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

nb_actions = 8
    
def build_model(environnement):
    print("Building Model ...")
    input_shape = (1,) + (environnement.Shape(),)
    model = Sequential()
    # model.add(Input(shape = input_shape))
    model.add(Flatten(input_shape = input_shape))
    # model.add(Dense(9))
    # model.add(Activation('relu'))
    # model.add(Dense(10))
    # model.add(Activation('sigmoid'))
    model.add(Dense((nb_actions+environnement.Shape())*2))
    model.add(Activation('relu'))
    model.add(Dense((nb_actions+environnement.Shape())))
    model.add(Activation('relu'))
    model.add(Dense((nb_actions+environnement.Shape())/2))
    model.add(Activation('relu'))
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
    memory = SequentialMemory(limit=1000, window_length=1)
    dqn = DQNAgent(model=model,memory=memory,policy=policy,enable_dueling_network=(True),
                    dueling_type='avg',nb_actions=nb_actions,nb_steps_warmup=10000)
    # dqn = DQNAgent(model=model,memory=memory,policy=policy,nb_actions=nb_actions)
    dqn.compile(Adam(learning_rate=0.00001))
    return dqn

def main():
    # e = Environnement([(0,0),(1,0),(2,0),
    #                 (0,1),(1,1),(2,1),
    #                 (0,2),(1,2),(2,2)])
    # e = Environnement([(0,0),(1,0),(2,0),(3,0),(4,0),(5,0),
    #                 (0,1),(1,1),(2,1),(3,1),(4,1),(5,1),
    #                 (0,2),(1,2),(2,2),(3,2),(4,2),(5,2),
    #                 (0,3),(1,3),(2,3),(3,3),(4,3),(5,3),
    #                 (0,4),(1,4),(2,4),(3,4),(4,4),(5,4),
    #                 (0,5),(1,5),(2,5),(3,5),(4,5),(5,5)])
    # e = Environnement([(0,0),(1,0),(2,0),(3,0),(4,0),(5,0),
    #                 (0,1),(1,1),(2,1),(5,1),
    #                 (0,2),(1,2),(2,2),(3,2),(4,2),(5,2)])
    e = Environnement([(1,1),(2,1),(3,1),
                       (1,2),(2,2),(3,2),
                       (1,3),(2,3),(3,3),(4,3),      (6,3),(7,3),
                             (2,4),                  (6,4),(7,4),
                       (1,5),(2,5),(3,5),(4,5),(5,5),(6,5),(7,5),
                             (2,6),
                       (1,7),(2,7),(3,7),
                       (1,8),(2,8),(3,8),
                       (1,9),(2,9),(3,9),(4,9)])
    posAgent = None#(0,0)
    
    nbiter_per_episode = 500
    env = PatrollingProblemEnv(e,posAgent,nbiter_per_episode)#gym.make('patrolling-v0')
    
    
    # print("Env shape : "+str(env.observation_space.shape))
    # input_shape = (1,) + (e.Shape(),)
    # #input_shape = (1,) + (len(e.nodes)+2,)#
    # print("input_shape is "+str(input_shape))
    
    model = build_model(e)
    dqn = build_agent(model, Environnement.actions)

    hist = dqn.fit(env, nb_steps=10000000, visualize=False, verbose=1)
    Y = [i / nbiter_per_episode for i in hist.history['episode_reward']]
    X = [x for x in range(len(Y))]
    
    from matplotlib import pyplot as plt
    plt.plot(X,Y)
    
    dqn.save_weights("Weights/3piecesCouloirV2.h5f")
    
    # test = [0,0,10,10,10,10,0,0,10,0,0]
    # test = np.asarray(test, dtype=np.float32)
    # print(str(test.shape))
    # test = np.expand_dims(test, axis=0)
    # test = np.expand_dims(test, axis=0)
    # print(str(test.shape))
    
    # a = model.predict(test)
    # b = model.get_weights()
    
    #scores = dqn.test(env,nb_episodes=1,visualize = True)
    # print(np.mean(scores.history['episode_reward']))
    
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
    
    for p in [(0,0)]:
        pos = p
        e.Reset()
        e.Display()
        for i in range(200):
            print("old pos : "+str(pos))
            flt = e.Flatten(pos)
            # flt = np.asarray(flt, dtype=np.float32)
            # flt = np.expand_dims(flt, axis=0)
            # flt = np.expand_dims(flt, axis=0)
            
            state = dqn.memory.get_recent_state(flt)
            q_values = np.array(dqn.compute_q_values(state))
            arr = np.array(q_values).astype('float64')
            arr -= np.min(arr)
            softmax = np.exp(arr)/sum(np.exp(arr))
            
            a = Environnement.actions[randomChoice(softmax)]
            # a = Environnement.actions[dqn.forward(e.Flatten(pos))]#Environnement.actions[np.argmax([model.predict(flt)])]
            pos = e.Transition(pos, a)
            
            e.Display()
        print("===========")
        
    # print(plot_model(model))
    # dqn.test(env,20,visualize=True)
    
    # for i_episode in range(200):
    #     observation = env.reset()
    #     for t in range(100):
              
    #         env.render(mode="nothuman")
    #         action = env.action_space.sample()
    #         observation, reward, done, info = env.step(action)
    #         all_states_DEBUG.append(info)
    #         if done:
    #             # print("Episode finished after {} timesteps".format(t+1))
    #             break
    env.close()
    print("End")

if __name__ == '__main__': main()
