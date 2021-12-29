# -*- coding: utf-8 -*-
"""
Created on Tue Nov  9 00:20:22 2021

@author: adrien
"""

import numpy as np
import gym
from gym import spaces
from ..lib.environnement import Environnement
import random

class PatrollingProblemEnv(gym.Env):
  """Custom Environment that follows gym interface"""
  metadata = {'render.modes': ['human']}
  def __init__(self,environnement,agents,agentPos,nbAgent,nbiter_per_episode):
    super(PatrollingProblemEnv, self).__init__()
    # Define action and observation space
    # They must be gym.spaces objects
    # Example when using discrete actions:
    self.nbAgent = nbAgent
    self.environnement = environnement
    self.environnement_memory = Environnement(agents,environnement.nodes)
    
    if(not(agents==None)):
        self.agents = agents
        self.agents_memory = agents
    else:
        self.agents_memory = None
        self.agents = random.sample(self.environnement.nodes, self.nbAgent)#random.choice(self.environnement.nodes)
    
    self.playingAgent = 0 if agentPos == None else agents.index(agentPos)
    
    self.nbiter_per_episode = nbiter_per_episode

    self.listTimes = [self.environnement.nodesTimes.copy()]
    
    self.action_space = spaces.Discrete(8)
    # Example for using image as input:
    self.observation_space = spaces.Tuple((spaces.Discrete(self.environnement.nbX)
                                          ,spaces.Discrete(self.environnement.nbY)
                                          ,spaces.Box(-1, 100, shape = (8,), dtype=np.uint8)
                                          ,spaces.Box(-1, 100, shape = (self.environnement.nbX,self.environnement.nbY), dtype=np.uint8)
                                          ))
    #spaces.Box(low=-1, high=100, shape=(3,3,3,3), dtype=np.uint8)
    
  def step(self, action):
       # Execute one time step within the environment
       self.reward = self.environnement.Transition(self.playingAgent, Environnement.actions[action])
       observation = self.environnement.Flatten(self.playingAgent)
       self.playingAgent = (self.playingAgent+1) % self.nbAgent
       # self.reward = - np.mean(list(self.environnement.nodesTimes.values()))#1.0 / max(0.1,np.mean(list(self.environnement.nodesTimes.values())))
       
       done = self.environnement.nbiter > self.nbiter_per_episode
       info = self.environnement.nodesTimes.copy()
       self.action = action
       return (observation,self.reward,done,info)
       
  def reset(self):
     # Reset the state of the environment to an initial state
     # self.agents = self.agents_memory.copy() if self.agents_memory != None else None
     # if(self.agents==None):
     self.agents = random.sample(self.environnement.nodes, self.nbAgent)
     self.environnement = Environnement(self.agents,self.environnement_memory.nodes)
     observation = self.environnement.Flatten(self.playingAgent)
     return observation

  def render(self, mode='human', close=False):
   # Render the environment to the screen
       self.listTimes.append(self.environnement.nodesTimes.copy())
       l = self.environnement.nodesTimes.copy()
       if(mode=='human'):
           print("reward : "+str(self.reward))
           print("old pos : "+str(self.oldPose))
           print("action : "+str(Environnement.actions[self.action]))
           print("new pos : "+str(self.posAgent))
           self.environnement.Display()
           
