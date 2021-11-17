# -*- coding: utf-8 -*-
"""
Created on Tue Nov  9 00:20:22 2021

@author: adrien
"""

import numpy as np
import pandas as pd
import gym
from gym import spaces

class Environnement:
    
    actions = [(-1,-1),(-1,0),(-1,1),(0,-1),(0,1),(1,-1),(1,0),(1,1)]
    
    def __init__(self,nodes):
        self.nodes = nodes
        self.nodesTimes = {}
        for n in nodes:
            self.nodesTimes[n] = 0
    
    def GetNeighs(self,pos):
        l = []
        for a in Environnement.actions:
            n = (pos[0] + a[0],pos[1] + a[1])
            if(n in self.nodes):
                l.append(n)
        return l
    
    def Transition(self,posAgent,action):
        for n in self.nodesTimes:
            self.nodesTimes[n] += 1
        to = (posAgent[0] + action[0],posAgent[1] + action[1])
        if(to not in self.nodes):
            to = posAgent
        self.nodesTimes[to] = 0
        for n in self.GetNeighs(to):
            self.nodesTimes[n] = 0
        posAgent = to
        return posAgent

env = Environnement([(0,0),(1,0),(2,0),
                     (0,1),(1,1),(2,1),
                     (0,2),(1,2),(2,2)])

posAgent = (1,0)
posAgent = env.Transition(posAgent, (-1,0))

class PatrollingProblemEnv(gym.Env):
  """Custom Environment that follows gym interface"""
  metadata = {'render.modes': ['human']}
  def __init__(self, environnement, posAgent):
    super(PatrollingProblemEnv, self).__init__()
    # Define action and observation space
    # They must be gym.spaces objects
    # Example when using discrete actions:
    self.environnement = environnement
    self.posAgent = posAgent
    self.listTimes = [environnement.nodesTimes.copy()]
    
    self.action_space = spaces.Discrete(8)
    # Example for using image as input:
    self.observation_space = spaces.Box(low=0, high=255, shape=
                    (3,3,3,3), dtype=np.uint8)

  def step(self, action):
       # Execute one time step within the environment
       self.posAgent = self.environnement.Transition(self.posAgent, Environnement.actions[action])

  # def reset(self):
  #   # Reset the state of the environment to an initial state

  def render(self, mode='human', close=False):
   # Render the environment to the screen
       self.listTimes.append(self.environnement.nodesTimes.copy())



env = gym.make('CartPole-v0')
# for i_episode in range(10):
#     observation = env.reset()
#     for t in range(100):
#         env.render()
#         print(observation)
#         action = env.action_space.sample()
#         observation, reward, done, info = env.step(action)
#         if done:
#             print("Episode finished after {} timesteps".format(t+1))
#             break
# env.close()