# -*- coding: utf-8 -*-
"""
Created on Tue Nov  9 00:20:22 2021

@author: adrien
"""

import numpy as np

class Environnement:
    actions = [(-1,-1),(-1,0),(-1,1),(0,-1),(0,1),(1,-1),(1,0),(1,1)]
    # 0 3 5
    # 1   6
    # 2 4 7
    
    def __init__(self,nodes):
        self.nbiter = 0
        self.nodes = nodes
        self.nodesTimes = {}
        for n in nodes:
            self.nodesTimes[n] = 0
        self.dimX = (min(n[0] for n in nodes),max(n[0] for n in nodes))
        self.dimY = (min(n[1] for n in nodes),max(n[1] for n in nodes))
    
    def GetNeighs(self,pos):
        l = []
        for a in Environnement.actions:
            n = (pos[0] + a[0],pos[1] + a[1])
            if(n in self.nodes):
                l.append(n)
        return l
    
    def Transition(self,posAgent,action):
        for n in self.nodesTimes:
            self.nodesTimes[n] = min(self.nodesTimes[n]+1,99)
        to = (posAgent[0] + action[0],posAgent[1] + action[1])
        if(to not in self.nodes):
            to = posAgent
        self.nodesTimes[to] = 0
        for n in self.GetNeighs(to):
            self.nodesTimes[n] = 0
        posAgent = to
        self.nbiter+=1
        return posAgent
    
    def Flatten(self,posAgent):
        observation = []
        observation.append(posAgent[0])
        observation.append(posAgent[1])
        # for i in self.nodes:
        #     observation.append(self.nodesTimes[i])
        for y in range(self.dimY[0],self.dimY[1]+1):
            for x in range(self.dimX[0],self.dimX[1]+1):
                if((x,y) in self.nodesTimes):
                    observation.append(self.nodesTimes[(x,y)])
        return np.array(observation)
    
    def Display(self):
        for y in range(self.dimY[0],self.dimY[1]+1):
            ligne = ""
            for x in range(self.dimX[0],self.dimX[1]+1):
                if((x,y) in self.nodesTimes):
                    ligne += str(self.nodesTimes[(x,y)])
                else : ligne += " "
                ligne += " | "
            print(ligne)
    
# environnement = Environnement([(0,0),(1,0),(2,0),(3,0),(4,0),
#                 (0,1),(1,1),(2,1),(4,1),
#                 (0,2),(1,2),(2,2),(3,2),(4,2)])
# posAgent = (1,0)
# posAgent = environnement.Transition(posAgent, Environnement.actions[1])
# test = environnement.Flatten(posAgent)
# environnement.Display()