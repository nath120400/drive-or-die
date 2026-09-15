## Scènes et flux de jeu

### Vue d'ensemble

Trois scènes composent la boucle de jeu : le Main Menu, la Game Scene et le Game Over.

- Le Main Menu envoie vers la Game Scene.
- La Game Scene envoie vers le Main Menu ou le Game Over.
- Le Game Over envoie vers le Main Menu ou la Game Scene.

### Main Menu

Trois choses :

- Un bouton Play, qui lance la Game Scene.
- Un bouton Quitter.
- Le leaderboard, chargé depuis un fichier JSON persistant.

### Game Over

Trois choses :

- Un bouton Retry, qui relance la Game Scene.
- Un bouton Main Menu.
- Le score de la run qui vient d'être perdue.
