# -*- coding: utf-8 -*-
"""
Created on Tue Nov  9 00:20:22 2021

@author: adrien
"""

import numpy as np
import gym
from gym import spaces
from ..lib.environnement import Environnement
    
class PatrollingProblemEnv(gym.Env):
  """Custom Environment that follows gym interface"""
  metadata = {'render.modes': ['human']}
  def __init__(self,environnement,posAgent,nbiter_per_episode):
    super(PatrollingProblemEnv, self).__init__()
    # Define action and observation space
    # They must be gym.spaces objects
    # Example when using discrete actions:
    self.environnement = environnement
    self.environnement_memory = Environnement(environnement.nodes)
    self.posAgent = posAgent
    self.posAgent_memory = posAgent
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
       self.oldPose = self.posAgent
       self.posAgent = self.environnement.Transition(self.posAgent, Environnement.actions[action])
       observation = self.environnement.Flatten(self.posAgent)
       self.reward = - np.mean(list(self.environnement.nodesTimes.values()))#1.0 / max(0.1,np.mean(list(self.environnement.nodesTimes.values())))
       #if(self.reward == 0) : self.reward = 1
       done = self.environnement.nbiter > self.nbiter_per_episode
       info = self.environnement.nodesTimes.copy()
       self.action = action
       return (observation,self.reward,done,info)
       
  def reset(self):
     # Reset the state of the environment to an initial state
     self.environnement = Environnement(self.environnement_memory.nodes)
     self.posAgent = self.posAgent_memory
     observation = self.environnement.Flatten(self.posAgent)
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
           
