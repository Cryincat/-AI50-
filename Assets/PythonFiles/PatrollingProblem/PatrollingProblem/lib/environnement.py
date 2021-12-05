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
    
    def __init__(self,nodes,nodesTimes = None):
        self.nbiter = 0
        self.nodes = nodes.copy()
        self.nodesTimes = {}
        
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
    
    def Transition(self,posAgent,action):
        inc = 1 if(abs(action[0])+abs(action[1]) < 2) else 1.41
        for n in self.nodesTimes:
            self.nodesTimes[n] = min(self.nodesTimes[n]+inc,99)
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
        for a in Environnement.actions:
            n = (posAgent[0] + a[0],posAgent[1] + a[1])
            observation.append(self.nodesTimes[n] if n in self.nodesTimes else -1)
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
        return len(self.Flatten((0,0)))
    
# environnement = Environnement([(0,0),(1,0),(2,0),(3,0),(4,0),
#                 (0,1),(1,1),(2,1),(4,1),
#                 (0,2),(1,2),(2,2),(3,2),(4,2)])
# posAgent = (1,0)
# posAgent = environnement.Transition(posAgent, Environnement.actions[1])
# test = environnement.Flatten(posAgent)
# environnement.Display()