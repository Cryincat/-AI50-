# -*- coding: utf-8 -*-
"""
Created on Mon Dec 20 14:08:36 2021

@author: Benjamin
"""

import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

# NE PAS CHANGER
nbLineBeforeData = 10

# Paramètre du nombre de données apparentes sur l'axe X et Y
nbDonneeX = 10
nbDonneeY = 8

# Lecture du fichier .txt, récupération seulement des données
path = "data3.txt"

#Récupération du nom de la simulation pour donner un nom au fichier .png
name = path.split(".")[0]

#On lit le fichier .txt
file = open(path, 'r')
dataBrut = file.readlines()
file.close()

#On clean la data récupéré pour se retrouver uniquement avec les données de simulation en temps réel
for i in range (nbLineBeforeData): dataBrut.pop(0)

X = []
Y = []

# Pour chaque donnée, on la split et on sépare le X de son Y
for elem in dataBrut:

    elemSplit = elem.split(' : ')
    X.append(float(elemSplit[0]))
    Y.append(float(elemSplit[1].replace(",",".")))


# Création du graphique
plt.title("Simulation of " + name)
plt.axis('on')
plt.ylabel("Medium Idleness")
plt.xlabel("Seconds")
xticks = plt.xticks(range(1,int(np.max(X)),int(np.max(X)/nbDonneeX)))
yticks = plt.yticks(range(1,int(np.max(Y)),int(np.max(Y)/nbDonneeY)))
plt.plot(X,Y)



plt.savefig(name + ".png")

