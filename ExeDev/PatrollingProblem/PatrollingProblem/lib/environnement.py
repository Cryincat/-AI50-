# -*- coding: utf-8 -*-
"""
Created on Tue Nov  9 00:20:22 2021

@author: adrien
"""

import numpy as np
from gym import spaces

class Environnement:
    actions = [(-1,-1),(-1,0),(-1,1),(0,-1),(0,1),(1,-1),(1,0),(1,1)]
    # 0 3 5
    # 1   6
    # 2 4 7
    
    def __init__(self,agents,nodes,nodesTimes = None):
        self.nbiter = 0
        self.nodes = nodes.copy()
        self.nodesTimes = {}
        self.agents = agents.copy()
        # self.agents = sorted(self.agents, key=lambda tup: (tup[0],tup[1]) )
        
        for n in nodes:
            assert n not in self.nodesTimes
            if(nodesTimes == None) :
                self.nodesTimes[n] = 0
            else:
                self.nodesTimes[n] = nodesTimes[n]
                
        self.dimX = (min(n[0] for n in nodes),max(n[0] for n in nodes))
        self.dimY = (min(n[1] for n in nodes),max(n[1] for n in nodes))
        self.nbX = self.dimX[1]-self.dimX[0]+1
        self.nbY = self.dimY[1]-self.dimY[0]+1

    
    def GetNeighs(self,pos):
        l = []
        for a in Environnement.actions:
            n = (pos[0] + a[0],pos[1] + a[1])
            if(n in self.nodes):
                l.append(n)
        return l
    
    def Transition(self,agent,action):
        inc = 1.0 / len(self.agents) if(abs(action[0])+abs(action[1]) < 2) else 1.41 / len(self.agents)
        for n in self.nodesTimes:
            self.nodesTimes[n] = min(self.nodesTimes[n]+inc,99)
        to = (self.agents[agent][0] + action[0],self.agents[agent][1] + action[1])
        if(to not in self.nodes):
            to = self.agents[agent]
        self.agents[agent] = to
        
        delta = self.nodesTimes[to] 
        for n in self.GetNeighs(to):
            delta += self.nodesTimes[n]
            self.nodesTimes[n] = 0
        
        self.nodesTimes[to] = 0
        
        self.reward = - np.mean(list(self.nodesTimes.values()))
        
        posAgent = to
        self.nbiter+= 1.0 / len(self.agents)
        
        # self.agents = sorted(self.agents, key=lambda tup: (tup[0],tup[1]) )
        return self.reward

    
    def Flatten(self,playingAgent):
        observation = []
        observation.append(self.agents[playingAgent][0])
        observation.append(self.agents[playingAgent][1])
        for a in self.agents:
            observation.append(a[0])
            observation.append(a[1])
        # for a in Environnement.actions:
        #     n = (posAgent[0] + a[0],posAgent[1] + a[1])
        #     observation.append(self.nodesTimes[n] if n in self.nodesTimes else -1)
        for y in range(self.dimY[0],self.dimY[1]+1):
            for x in range(self.dimX[0],self.dimX[1]+1):
                if((x,y) in self.nodesTimes):
                    observation.append(self.nodesTimes[(x,y)])
                else :
                    observation.append(-1)
        return np.array(observation)
    
    def Reset(self):
        for n in self.nodes:
            self.nodesTimes[n] = 0
    
    def Display(self):
        for y in range(self.dimY[0],self.dimY[1]+1):
            ligne = ""
            for x in range(self.dimX[0],self.dimX[1]+1):
                if((x,y) in self.nodesTimes):
                    ligne += str(self.nodesTimes[(x,y)])
                else : ligne += " "
                ligne += " | "
            print(ligne)
    
    def Shape(self):
        return len(self.Flatten(0))
    
    def Space(self):
        space = []
        space.extend([spaces.Discrete(self.nbX),spaces.Discrete(self.nbY)])
        for i in self.agents:
            space.extend([spaces.Discrete(self.nbX),spaces.Discrete(self.nbY)])
        space.append(spaces.Box(-1, 100, shape = (self.nbX,self.nbY), dtype=np.uint8))
        space = tuple(space)
        return space
    
# environnement = Environnement([(0,0),(1,0),(2,0),(3,0),(4,0),
#                 (0,1),(1,1),(2,1),(4,1),
#                 (0,2),(1,2),(2,2),(3,2),(4,2)])
# posAgent = (1,0)
# posAgent = environnement.Transition(posAgent, Environnement.actions[1])
# test = environnement.Flatten(posAgent)
# environnement.Display()